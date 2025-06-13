using ArcsomAssetManagement.Client.DTOs.Business;
using ArcsomAssetManagement.Client.Models;
using AutoMapper;
using Microsoft.Extensions.Logging;
using SQLite;
using System.Net.Http.Json;
using System.Text.Json;

namespace ArcsomAssetManagement.Client.Data;

public class AssetRepository : IOnlineRepository<AssetDto>
{
    private readonly ILogger<AssetRepository> _logger;
    private SQLiteAsyncConnection? _database;
    private readonly HttpClient _httpClient;
    private readonly IMapper _mapper;
    private readonly ConnectivityService _connectivity;
    private readonly string _apiUrl;

    public AssetRepository(ILogger<AssetRepository> logger, SQLiteAsyncConnection database, IHttpClientFactory httpClientFactory, IMapper mapper, ConnectivityService connectivity)
    {
        _logger = logger;
        _database = database;
        _httpClient = httpClientFactory.CreateClient("AuthorizedClient");
        _mapper = mapper;
        _connectivity = connectivity;
        _apiUrl = "api/asset";
        _ = InitAsync();
    }

    private async Task InitAsync()
    {
        var tables = _database.GetTableInfoAsync("Asset").Result;
        if (_database is not null && tables.Count() > 0)
            return;

        _database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        await _database.CreateTableAsync<Asset>();
        await _database.CreateTableAsync<SyncQueueItem>();

    }

    public async Task<Asset?> GetAsync(ulong id)
    {
        if (_connectivity.IsOnline)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiUrl}/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadFromJsonAsync<AssetDto>();
                    var domainItem = _mapper.Map<Asset>(json ?? new AssetDto());
                    return domainItem;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get asset online.");
            }
        }

        Asset? asset = await _database.FindAsync<Asset>(id) ?? null;
        if (asset.ProductId > 0)
        {
            Product product = await _database.FindAsync<Product>(asset.ProductId);
            asset.Product = product;
        }
        return asset;
    }

    public async Task<List<Asset>> ListAsync()
    {
        if (_connectivity.IsOnline)
        {
            try
            {
                var response = await _httpClient.GetAsync(_apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var dtos = await response.Content.ReadFromJsonAsync<IEnumerable<AssetDto>>();
                    var domainItems = _mapper.Map<List<Asset>>(dtos ?? new List<AssetDto>());
                    return domainItems;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list assets online.");
                return new List<Asset>();
            }
        }

        try
        {
            var assets = await _database.Table<Asset>().ToListAsync();
            var products = await _database.Table<Product>().ToListAsync();

            foreach (var asset in assets)
            {
                asset.Product = products.FirstOrDefault(m => m.Id == asset.ProductId);
            }
            return assets;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing assets from database");
            return new List<Asset>();
        }
    }

    public async Task<(List<Asset>, PaginationModel)> ListAsync(int pageNumber = 1, int pageSize = 3, string filter = "")
    {
        var pagination = new PaginationModel
        {
            CurrentPage = pageNumber,
            PageSize = pageSize,
            TotalItems = pageSize
        };

        var apiUrlPaged = $"{_apiUrl}/Paged?pageNumber={pageNumber}&pageSize={pageSize}&filter={filter}";

        if (_connectivity.IsOnline)
        {
            try
            {
                var response = await _httpClient.GetAsync(apiUrlPaged);
                if (response.IsSuccessStatusCode)
                {
                    var dtos = await response.Content.ReadFromJsonAsync<IEnumerable<AssetDto>>();
                    var domainItems = _mapper.Map<List<Asset>>(dtos ?? new List<AssetDto>());
                    pagination.TotalItems = int.Parse(response.Headers.GetValues("X-Total-Count").FirstOrDefault() ?? "0");
                    return (domainItems, pagination);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list assets online.");
                return (new List<Asset>(), pagination);
            }
        }

        try
        {
            var lowerFilter = filter.ToLowerInvariant();
            var totalItems = await _database.Table<Asset>()
                                .Where(p => p.Name.ToLower().Contains(lowerFilter))
                                .CountAsync();

            AsyncTableQuery<Asset> query = _database.Table<Asset>();
            if (!string.IsNullOrWhiteSpace(filter))
            {
                query = query.Where(p => p.Name.ToLower().Contains(lowerFilter));
            }

            var assets = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            foreach(var asset in assets)
            {
                asset.Product = await _database.FindAsync<Product>(asset.ProductId);
            }

            pagination.TotalItems = totalItems;

            return (assets, pagination);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing assets from database");
            return (new List<Asset>(), pagination);
        }
    }
    public async Task<ulong> SaveItemAsync(Asset item, bool trackSync)
    {
        AssetDto dto = _mapper.Map<AssetDto>(item);
        if (item.Id == 0)
        {
            if (_connectivity.IsOnline)
            {
                await SaveItemOnlineAsync(dto);
                await _database.InsertAsync(item);
                return item.Id;
            }

            await _database.InsertAsync(item);

            if (trackSync)
            {
                await _database.InsertAsync(new SyncQueueItem
                {
                    EntityType = nameof(Asset),
                    EntityId = item.Id,
                    OperationType = OperationType.Create,
                    PayloadJson = JsonSerializer.Serialize(item)
                });
            }
            return item.Id;
        }
        else
        {

            if (_connectivity.IsOnline)
            {
                await UpdateItemOnlineAsync(dto);
                var updatedRows = await _database.UpdateAsync(item);
                if (updatedRows == 0) await _database.InsertOrReplaceAsync(item);
                return item.Id;
            }
            var result = await _database.UpdateAsync(item);
            if (result == 0) await _database.InsertOrReplaceAsync(item);

            if (trackSync)
            {
                await _database.InsertAsync(new SyncQueueItem
                {
                    EntityType = nameof(Asset),
                    EntityId = item.Id,
                    OperationType = OperationType.Update,
                    PayloadJson = JsonSerializer.Serialize(item)
                });
            }
            return item.Id;
        }
    }
    public async Task<int> DeleteItemAsync(Asset item)
    {
        if (_connectivity.IsOnline)
        {
            try
            {
                var isDeletedOnline = await DeleteItemOnlineAsync(item.Id);
                if (isDeletedOnline)
                {
                    await _database.DeleteAsync(item);
                    return 1;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete asset online.");
            }
        }

        await _database.InsertAsync(new SyncQueueItem
        {
            EntityType = nameof(Asset),
            EntityId = item.Id,
            OperationType = OperationType.Delete,
            PayloadJson = JsonSerializer.Serialize(item)
        });

        return await _database.DeleteAsync(item);
    }

    public async Task<IEnumerable<AssetDto>> ListOnlineAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(_apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var dtos = await response.Content.ReadFromJsonAsync<IEnumerable<AssetDto>>();
                return dtos;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list products online.");
            return new List<AssetDto>();
        }

        return new List<AssetDto>();
    }

    public async Task SaveItemOnlineAsync(AssetDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync($"{_apiUrl}", dto);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Error while saving item: {response.StatusCode}");
        }
    }

    public async Task UpdateItemOnlineAsync(AssetDto dto)
    {
        var response = await _httpClient.PatchAsJsonAsync($"{_apiUrl}/{dto.Id}", dto);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Error while updating item: {response.StatusCode}");
        }
    }
    public async Task<bool> DeleteItemOnlineAsync(ulong id)
    {
        var response = await _httpClient.DeleteAsync($"{_apiUrl}/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task SaveToOfflineAsync(IEnumerable<AssetDto> items)
    {
        var domainItems = _mapper.Map<List<Asset>>(items ?? new List<AssetDto>());

        foreach (var item in domainItems)
        {
            await _database.InsertAsync(item);

            //var existing = await _database.FindAsync<Asset>(item.Id);

            //if (existing == null)
            //{
            //    try
            //    {
            //        var response = await _database.InsertAsync(item);
            //    }
            //    catch (SQLiteException ex)
            //    {
            //        _logger.LogInformation(ex, $"Failed to insert asset online. Delete the offline asset.{item.Name}");
            //    }
            //}
            //else
            //{
            //    await _database.UpdateAsync(item);
            //}
        }
    }
    public async Task<bool> ClearTableAsync()
    {
        await InitAsync();

        if (_connectivity.IsOnline)
        {
            var isClearedOnline = await ClearTableOnlineAsync();

            if (!isClearedOnline)
            {
                throw new Exception("Failed to clear asset table online.");
            }
        }

        await _database.DeleteAllAsync<Asset>();
        await _database.ExecuteAsync("DELETE FROM sqlite_sequence WHERE name = 'Asset'");
        return true;
    }

    public async Task<bool> ClearTableOnlineAsync()
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{_apiUrl}/clear");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to drop asset table online.");
            return false;
        }
    }
}
