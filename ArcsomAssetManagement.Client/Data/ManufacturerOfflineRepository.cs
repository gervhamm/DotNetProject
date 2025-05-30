using ArcsomAssetManagement.Client.Models;
using Microsoft.Extensions.Logging;
using SQLite;
using System.Text.Json;

namespace ArcsomAssetManagement.Client.Data;

public class ManufacturerOfflineRepository : IOfflineRepository<Manufacturer>
{
    private readonly ILogger<ManufacturerOfflineRepository> _logger;
    private SQLiteAsyncConnection? _database;

    public ManufacturerOfflineRepository(ILogger<ManufacturerOfflineRepository> logger, SQLiteAsyncConnection database)
    {
        _logger = logger;
        _database = database;
    }

    private async Task Init()
    {
        if (_database is not null)
            return;

        _database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        await _database.CreateTableAsync<Manufacturer>();

    }
    public async Task<int> DeleteItemAsync(Manufacturer item)
    {
        await _database.InsertAsync(new SyncQueueItem
        {
            EntityType = item.Name,
            EntityId = item.Id,
            OperationType = OperationType.Delete,
            PayloadJson = JsonSerializer.Serialize(item)
        });
        return await _database.DeleteAsync(item);
    }

    public async Task<Manufacturer?> GetAsync(ulong id)
    {
        return await _database.FindAsync<Manufacturer>(id);
    }

    public async Task<List<Manufacturer>> ListAsync()
    {
        await Init();

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

    public async Task<ulong> SaveItemAsync(Manufacturer item, bool trackSync)
    {
        await Init();

        if (item.Id == 0)
        {
            try
            {
                await _database.InsertAsync(item);
            }
            catch (SQLiteException ex) when (ex.Result == SQLite3.Result.Constraint)
            {
                throw new Exception($"Failed to insert item due to constraint violation: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to insert item: {ex.Message}", ex);
            }

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
        }
        else
        {
            var result = await _database.UpdateAsync(item);

            if (result == 0)
            {
                try
                {
                    await _database.InsertOrReplaceAsync(item);
                }
                catch (SQLiteException ex) when (ex.Result == SQLite3.Result.Constraint)
                {
                    throw new Exception($"Failed to insert or replace item due to constraint violation: {ex.Message}", ex);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to insert or replace item: {ex.Message}", ex);
                }
            }
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
        }
        return (ulong)item.GetHashCode();
    }
}
