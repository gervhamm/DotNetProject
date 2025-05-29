using ArcsomAssetManagement.Client.Models;
using Microsoft.Extensions.Logging;
using SQLite;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace ArcsomAssetManagement.Client.Data;

public class ProductOfflineRepository : IOfflineRepository<Product>
{
    private readonly ILogger<ProductOfflineRepository> _logger;
    private SQLiteAsyncConnection? _database;
    public ProductOfflineRepository(ILogger<ProductOfflineRepository> logger, SQLiteAsyncConnection database)
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
        var result = await _database.CreateTableAsync<Product>();

    }
    public async Task<int> DeleteItemAsync(Product item)
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

    public async Task<Product?> GetAsync(ulong id)
    {
        return await _database.FindAsync<Product>(id);
    }

    public async Task<List<Product>> ListAsync()
    {
        await Init();

        try
        {
            return await _database.Table<Product>().ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing products from database");
            return new List<Product>();
        }
    }

    public async Task<ulong> SaveItemAsync(Product item, bool trackSync)
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
