using ArcsomAssetManagement.Client.DTOs.Business;
using ArcsomAssetManagement.Client.Models;
using AutoMapper;
using Microsoft.Extensions.Logging;
using SQLite;
using System.Net.Http.Json;
using System.Text.Json;

namespace ArcsomAssetManagement.Client.Data;

public class ManufacturerRepository
{
    private readonly ILogger<ManufacturerRepository> _logger;
    private SQLiteAsyncConnection? _database;
    private readonly HttpClient _httpClient;
    private readonly IMapper _mapper;
    private readonly string _apiUrl;

    public ManufacturerRepository(ILogger<ManufacturerRepository> logger, SQLiteAsyncConnection database, HttpClient httpClient, IMapper mapper)
    {
        _logger = logger;
        _database = database;
        _httpClient = httpClient;
        _mapper = mapper;
        _apiUrl = Environment.GetEnvironmentVariable("API_URL") + "/api/manufacturer" ?? string.Empty;
        _ = InitAsync();
    }

    private async Task InitAsync()
    {
        if (_database is not null)
            return;

        _database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        await _database.CreateTableAsync<Manufacturer>();
        await _database.CreateTableAsync<SyncQueueItem>();

    }

    public async Task<Manufacturer?> GetAsync(ulong id)
    {
        if (await IsOnlineAsync())
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiUrl}/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadFromJsonAsync<ManufacturerDto>();
                    var domainItem = _mapper.Map<Manufacturer>(json ?? new ManufacturerDto());
                    return domainItem;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get manufacturer online.");
            }
        }

        var manufacturer = await _database.FindAsync<Manufacturer>(id);
        var products = _database.Table<Product?>()
            .Where(p => p.ManufacturerId == manufacturer.Id).ToListAsync();
        manufacturer.Products = await products;

        return manufacturer;
    }

    public async Task<List<Manufacturer>> ListAsync()
    {
        if (await IsOnlineAsync())
        {
            try
            {
                var response = await _httpClient.GetAsync(_apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var dtos = await response.Content.ReadFromJsonAsync<IEnumerable<ManufacturerDto>>();
                    var domainItems = _mapper.Map<List<Manufacturer>>(dtos ?? new List<ManufacturerDto>());
                    return domainItems;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list manufacturers online.");
                return new List<Manufacturer>();
            }
        }

        try
        {
            return await _database.Table<Manufacturer>().ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing manufacturers from database");
            return new List<Manufacturer>();
        }
    }

    public async Task<(List<Manufacturer>, PaginationModel)> ListAsync(int pageNumber = 1, int pageSize = 3, string filter = null)
    {

        var pagination = new PaginationModel
        {
            PageSize = pageSize,
            CurrentPage = pageNumber,
            TotalItems = pageSize
        };

        if (await IsOnlineAsync())
        {
            try
            {
                var response = await _httpClient.GetAsync(_apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var dtos = await response.Content.ReadFromJsonAsync<IEnumerable<ManufacturerDto>>();
                    var domainItems = _mapper.Map<List<Manufacturer>>(dtos ?? new List<ManufacturerDto>());
                    return (domainItems, pagination);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list manufacturers online.");
                return (new List<Manufacturer>(), pagination);
            }
        }

        try
        {
            var totalItems = await _database.Table<Manufacturer>().CountAsync();

            AsyncTableQuery<Manufacturer> query = _database.Table<Manufacturer>();
            if (filter != null)
            {
                filter = filter.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(filter) ||
                                              p.Contact.ToLower().Contains(filter));
            }
            var manufacturers = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            pagination.TotalItems = totalItems;

            return (manufacturers, pagination);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing manufacturers from database");
            return (new List<Manufacturer>(), pagination);
        }
    }

    public async Task<ulong> SaveItemAsync(Manufacturer item, bool trackSync)
    {
        ManufacturerDto dto = _mapper.Map<ManufacturerDto>(item);
        if (item.Id == 0)
        {
            if (await IsOnlineAsync())
            {
                var response = await _httpClient.PostAsJsonAsync($"{_apiUrl}", dto);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Error while saving item: {response.StatusCode}");
                }
                return item.Id;
            }

            await _database.InsertAsync(item);

            if (trackSync)
            {
                await _database.InsertAsync(new SyncQueueItem
                {
                    EntityType = item.Name,
                    EntityId = item.Id,
                    OperationType = OperationType.Create,
                    PayloadJson = JsonSerializer.Serialize(item)
                });
            }
            return item.Id;
        }
        else
        {

            if (await IsOnlineAsync())
            {
                var response = await _httpClient.PatchAsJsonAsync($"{_apiUrl}/{item.Id}", dto);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Error while updating item: {response.StatusCode}");
                }
                return item.Id;
            }
            var result = await _database.UpdateAsync(item);
            if (result == 0) await _database.InsertOrReplaceAsync(item);

            if (trackSync)
            {
                await _database.InsertAsync(new SyncQueueItem
                {
                    EntityType = item.Name,
                    EntityId = item.Id,
                    OperationType = OperationType.Update,
                    PayloadJson = JsonSerializer.Serialize(item)
                });
            }
            return item.Id;
        }
    }
    public async Task<int> DeleteItemAsync(Manufacturer item)
    {
        if (await IsOnlineAsync())
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_apiUrl}/{item.Id}");
                if (response.IsSuccessStatusCode)
                {
                    await _database.DeleteAsync(item);
                    return 1;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete manufacturer online.");
            }
        }

        await _database.InsertAsync(new SyncQueueItem
        {
            EntityType = item.Name,
            EntityId = item.Id,
            OperationType = OperationType.Delete,
            PayloadJson = JsonSerializer.Serialize(item)
        });

        return await _database.DeleteAsync(item);
    }

    private async Task<bool> IsOnlineAsync()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        try
        {
            var uri = new Uri(_apiUrl);
            var pingUri = uri.GetLeftPart(UriPartial.Authority) + "/ping";
            var response = await _httpClient.GetAsync(pingUri, cts.Token);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
