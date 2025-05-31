using ArcsomAssetManagement.Client.DTOs.Business;
using ArcsomAssetManagement.Client.Models;
using AutoMapper;
using Microsoft.Extensions.Logging;
using SQLite;
using System.Net.Http.Json;
using System.Text.Json;

namespace ArcsomAssetManagement.Client.Data;

public class ProductRepositoryTest
{
    private readonly ILogger<ProductRepositoryTest> _logger;
    private SQLiteAsyncConnection? _database;
    private readonly HttpClient _httpClient;
    private readonly IMapper _mapper;
    private readonly string _apiUrl;

    public ProductRepositoryTest(ILogger<ProductRepositoryTest> logger, SQLiteAsyncConnection database, HttpClient httpClient, IMapper mapper)
    {
        _logger = logger;
        _database = database;
        _httpClient = httpClient;
        _mapper = mapper;
        _apiUrl = Environment.GetEnvironmentVariable("API_URL") + "/api/product" ?? string.Empty;
        _ = InitAsync();
    }

    private async Task InitAsync()
    {
        if (_database is not null)
            return;

        _database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        await _database.CreateTableAsync<Product>();
        await _database.CreateTableAsync<SyncQueueItem>();

    }

    public async Task<Product?> GetAsync(ulong id)
    {
        if (await IsOnlineAsync())
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiUrl}/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadFromJsonAsync<ProductDto>();
                    var domainItem = _mapper.Map<Product>(json ?? new ProductDto());
                    return domainItem;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get product online.");
            }
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
        if (await IsOnlineAsync())
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

    public async Task<ulong> SaveItemAsync(Product item, bool trackSync)
    {
        ProductDto dto = _mapper.Map<ProductDto>(item);
        if (item.Id == 0)
        {
            if (await IsOnlineAsync())
            {
                var response = await _httpClient.PostAsJsonAsync($"{_apiUrl}", dto);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Error while updating item: {response.StatusCode}");
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
    public async Task<int> DeleteItemAsync(Product item)
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
                _logger.LogError(ex, "Failed to delete product online.");
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
