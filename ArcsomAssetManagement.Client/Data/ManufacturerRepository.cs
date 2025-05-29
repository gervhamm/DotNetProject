using ArcsomAssetManagement.Client.Models;
using Microsoft.Extensions.Logging;
using SQLite;
using System.Collections.ObjectModel;

namespace ArcsomAssetManagement.Client.Data;

public class ManufacturerRepository2
{
    private SQLiteAsyncConnection _database;
    private readonly ILogger _logger;
    private bool _hasBeenInitialized = false;

    public ManufacturerRepository2(ILogger<ManufacturerRepository2> logger)
    {
        _logger = logger;
    }

    private async Task Init()
    {
        if (_database is not null)
            return;

        _database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        var result = await _database.CreateTableAsync<Manufacturer>();
        _hasBeenInitialized = true;

    }
    public async Task<ObservableCollection<Manufacturer>> ListAsync()
    {
        await Init();
        var manufacturers = await _database.Table<Manufacturer>().ToListAsync();
        return new ObservableCollection<Manufacturer>(manufacturers);
    }

    public async Task<List<Product>> ListAsync(ulong manufacturerId)
    {
        await Init();
        return await _database.Table<Product>()
                              .Where(p => p.Manufacturer.Id == manufacturerId)
                              .ToListAsync();
    }

    public async Task<Product?> GetAsync(int id)
    {
        await Init();
        return await _database.Table<Product>().FirstOrDefaultAsync(p => p.Id == (ulong)id);
    }

    public async Task<ulong> SaveItemAsync(Product item)
    {
        await Init();

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
