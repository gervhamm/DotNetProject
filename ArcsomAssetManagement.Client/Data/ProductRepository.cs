using ArcsomAssetManagement.Client.DTOs.Business;
using ArcsomAssetManagement.Client.Models;
using AutoMapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using SQLite;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ArcsomAssetManagement.Client.Data;

public class ProductRepository : IOnlineRepository<ProductDto>
{
    private readonly ILogger<ProductRepository> _logger;
    private SQLiteAsyncConnection? _database;
    private readonly HttpClient _httpClient;
    private readonly IMapper _mapper;
    private readonly ConnectivityService _connectivity;
    private readonly string _apiUrl;

    public ProductRepository(ILogger<ProductRepository> logger, SQLiteAsyncConnection database, IHttpClientFactory httpClientFactory, IMapper mapper, ConnectivityService connectivity)
    {
        _logger = logger;
        _database = database;
        _httpClient = httpClientFactory.CreateClient("AuthorizedClient");
        _mapper = mapper;
        _connectivity = connectivity;
        _apiUrl = "api/product";
        _ = InitAsync();
        _connectivity = connectivity;
    }

    private async Task InitAsync()
    {
        var tables = _database.GetTableInfoAsync("Product").Result;
        if (_database is not null && tables.Count() > 0)
            return;

        _database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        await _database.CreateTableAsync<Product>();
        await _database.CreateTableAsync<SyncQueueItem>();

    }

    public async Task<Product?> GetAsync(ulong id)
    {        
        if (_connectivity.IsOnline)
        {
            var response = await _httpClient.GetAsync($"{_apiUrl}/{id}");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new HttpRequestException(HttpStatusCode.Unauthorized.ToString() + " - " + _apiUrl);
            }
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadFromJsonAsync<ProductDto>();
                var domainItem = _mapper.Map<Product>(json ?? new ProductDto());
                return domainItem;
            }

            throw new Exception($"Error while fetching product: {response.StatusCode} - {response.ReasonPhrase}");
        }

        Product? product = await _database.FindAsync<Product>(id) ?? null;
        if (product.ManufacturerId > 0)
        { 
            Manufacturer manufacturer = await _database.FindAsync<Manufacturer>(product.ManufacturerId);
            product.Manufacturer = manufacturer;
        }
        return product;
    }

    public async Task<List<Product>> ListAsync()
    {
        if (_connectivity.IsOnline)
        {
            try
            {
                var response = await _httpClient.GetAsync(_apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var dtos = await response.Content.ReadFromJsonAsync<IEnumerable<ProductDto>>();
                    var domainItems = _mapper.Map<List<Product>>(dtos ?? new List<ProductDto>());
                    return domainItems;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list products online.");
                return new List<Product>();
            }
        }

        try
        {
            var products = await _database.Table<Product>().ToListAsync();
            var manufacturers = await _database.Table<Manufacturer>().ToListAsync();

            foreach (var product in products)
            {
                product.Manufacturer = manufacturers.FirstOrDefault(m => m.Id == product.ManufacturerId);
            }
            return products;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing products from database");
            return new List<Product>();
        }
    }

    public async Task<(List<Product>, PaginationModel)> ListAsync(int pageNumber = 1, int pageSize = 3, string filter = "")
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
                    var dtos = await response.Content.ReadFromJsonAsync<IEnumerable<ProductDto>>();
                    var domainItems = _mapper.Map<List<Product>>(dtos ?? new List<ProductDto>());
                    pagination.TotalItems = int.Parse(response.Headers.GetValues("X-Total-Count").FirstOrDefault() ?? "0");
                    return (domainItems, pagination);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list products online.");
                return (new List<Product>(), pagination);
            }
        }

        try
        {
            var lowerFilter = filter.ToLowerInvariant();
            var totalItems = await _database.Table<Product>()
                                .Where(p => p.Name.ToLower().Contains(lowerFilter))
                                .CountAsync();

            AsyncTableQuery<Product> query = _database.Table<Product>();
            if (!string.IsNullOrWhiteSpace(filter))
            {
                query = query.Where(p => p.Name.ToLower().Contains(lowerFilter));
            }

            var products = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var manufacturers = await _database.Table<Manufacturer>().ToListAsync();

            foreach (var product in products)
            {
                product.Manufacturer = manufacturers.FirstOrDefault(m => m.Id == product.ManufacturerId);
            }

            pagination.TotalItems = totalItems;

            return (products, pagination);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing products from database");
            return (new List<Product>(), pagination);
        }
    }
    public async Task<ulong> SaveItemAsync(Product item, bool trackSync)
    {
        ProductDto dto = _mapper.Map<ProductDto>(item);
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
                    EntityType = nameof(Product),
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
                    EntityType = nameof(Product),
                    EntityId = item.Id,
                    OperationType = OperationType.Update,
                    PayloadJson = JsonSerializer.Serialize(item)
                });
            }
            return item.Id;
        }
    }
    public async Task<int> DeleteItemAsync(Product item)
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
                _logger.LogError(ex, "Failed to delete product online.");
            }
        }

        await _database.InsertAsync(new SyncQueueItem
        {
            EntityType = nameof(Product),
            EntityId = item.Id,
            OperationType = OperationType.Delete,
            PayloadJson = JsonSerializer.Serialize(item)
        });

        return await _database.DeleteAsync(item);
    }

    public async Task<IEnumerable<ProductDto>> ListOnlineAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync(_apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var dtos = await response.Content.ReadFromJsonAsync<IEnumerable<ProductDto>>();
                return dtos;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list manufacturers online.");
            return new List<ProductDto>();
        }

        return new List<ProductDto>();
    }

    public async Task SaveItemOnlineAsync(ProductDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync($"{_apiUrl}", dto);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Error while saving item: {response.StatusCode}");
        }
    }

    public async Task UpdateItemOnlineAsync(ProductDto dto)
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

    public async Task SaveToOfflineAsync(IEnumerable<ProductDto> items)
    {
        var domainItems = _mapper.Map<List<Product>>(items ?? new List<ProductDto>());

        foreach (var item in domainItems)
        {
            await _database.InsertAsync(item);

            //var existing = await _database.FindAsync<Product>(item.Id);

            //if (existing == null)
            //{
            //    try
            //    {
            //        var response = await _database.InsertAsync(item);
            //    }
            //    catch (SQLiteException ex)
            //    {
            //        _logger.LogInformation(ex, $"Failed to insert product online. Delete the offline product.{item.Name}");
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
                throw new Exception("Failed to clear product table online.");
            }
        }

        await _database.DeleteAllAsync<Product>();
        await _database.ExecuteAsync("DELETE FROM sqlite_sequence WHERE name = 'Product'");
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
            _logger.LogError(ex, "Failed to drop product table online.");
            return false;
        }
    }
}
