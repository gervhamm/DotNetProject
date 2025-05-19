using ArcsomAssetManagement.Client.Models;
using Microsoft.Extensions.Logging;
using SQLite;
using System.Collections.ObjectModel;

namespace ArcsomAssetManagement.Client.Data;

public class ProductRepository2
{
    private SQLiteAsyncConnection _database;
    private readonly ILogger _logger;
    private bool _hasBeenInitialized = false;

    public ProductRepository2(ILogger<ProductRepository2> logger)
    {
        _logger = logger;
    }

    private async Task Init()
    {
        if (_database is not null)
            return;

        _database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        await _database.CreateTableAsync<Manufacturer>();
        var result = await _database.CreateTableAsync<Product>();
        _hasBeenInitialized = true;

    }
    public async Task<ObservableCollection<Product>> ListAsync()
    {
        await Init();
        var products = await _database.Table<Product>().ToListAsync();
        return new ObservableCollection<Product>(products);
    }

    public async Task<List<Product>> ListAsync(ulong manufacturerId)
    {
        await Init();
        return await _database.Table<Product>()
                              .Where(p => p.ManufacturerId == manufacturerId)
                              .ToListAsync();
    }

    public async Task<Product?> GetAsync(int id)
    {
        await Init();
        var product = await _database.Table<Product>().FirstOrDefaultAsync(p => p.Id == (ulong)id);
        product.Manufacturer = await _database.Table<Manufacturer>().FirstOrDefaultAsync(m => m.Id == product.ManufacturerId);
        return product;
    }

    public async Task<ulong> SaveItemAsync(Product item)
    {
        await Init();
        item.ManufacturerId = item.Manufacturer.Id;

        if (item.Id == 0)
            await _database.InsertAsync(item);
        else
            await _database.UpdateAsync(item);

        return item.Id;
    }

    public async Task<int> DeleteItemAsync(Product item)
    {
        await Init();
        return await _database.DeleteAsync(item);
    }

    public async Task DropTableAsync()
    {
        await Init();
        await _database.DropTableAsync<Product>();
        _hasBeenInitialized = false;
    }
}
