using ArcsomAssetManagement.Client.Models;
using Microsoft.Extensions.Logging;
using SQLite;
using System.Collections.ObjectModel;
using System.Net.Http.Json;

namespace ArcsomAssetManagement.Client.Data;

public class ManufacturerRepository : RepositoryBase<Manufacturer>
{
    private SQLiteAsyncConnection _database;

    public ManufacturerRepository(ILogger<ManufacturerRepository> logger) : base(logger) { }

    public override async Task<ObservableCollection<Manufacturer>> ListAsync()
    {
        if (await IsOnline())
        {
            var manufacturers = await _database.Table<Manufacturer>().ToListAsync();
            return new ObservableCollection<Manufacturer>(manufacturers);
        }

        await Init();
        var manufacturers = await _database.Table<Manufacturer>().ToListAsync();
        return new ObservableCollection<Manufacturer>(manufacturers);
    }
    public async Task<ObservableCollection<Manufacturer>> ListAsync()
    {
        await Init();
        var manufacturers = await _database.Table<Manufacturer>().ToListAsync();
        return new ObservableCollection<Manufacturer>(manufacturers);
    }

    public async Task<Manufacturer?> GetAsync(int id)
    {
        await Init();
        return await _database.Table<Manufacturer>().FirstOrDefaultAsync(m => m.Id == (ulong)id);
    }

    public async Task<ulong> SaveItemAsync(Manufacturer item)
    {
        await Init();

        if (item.Id == 0)
            await _database.InsertAsync(item);
        else
            await _database.UpdateAsync(item);

        return item.Id;
    }

    public async Task<int> DeleteItemAsync(Manufacturer item)
    {
        await Init();
        return await _database.DeleteAsync(item);
    }

    public async Task DropTableAsync()
    {
        await Init();
        await _database.DropTableAsync<Manufacturer>();
        _hasBeenInitialized = false;
    }

    private async Task<List<Manufacturer>> FetchFromApi()
    {
        var client = await GetOnlineClient();
        var response = await client.GetAsync("manufacturers");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<Manufacturer>>();
    }
}
