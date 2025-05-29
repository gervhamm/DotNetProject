using ArcsomAssetManagement.Client.Models;
using SQLite;
using System.Text.Json;

namespace ArcsomAssetManagement.Client.Data;

public class OfflineRepository<T> : IOfflineRepository<T> where T : class, IIdentifiable, new ()
{
    private readonly SQLiteAsyncConnection _connection;

    public OfflineRepository(SQLiteAsyncConnection connection)
    {
        _connection = connection;
        _connection.CreateTableAsync<T>().ConfigureAwait(true);
    }
    public async Task<int> DeleteItemAsync(T item)
    {
        await _connection.InsertAsync(new SyncQueueItem
        {
            EntityType = typeof(T).Name,
            EntityId = item.Id,
            OperationType = OperationType.Delete,
            PayloadJson = JsonSerializer.Serialize(item)
        });
        return await _connection.DeleteAsync(item);
    }

    public async Task<T?> GetAsync(ulong id)
    {
        return await _connection.FindAsync<T>(id);
    }

    public async Task<List<T>> ListAsync()
    {
        return await _connection.Table<T>().ToListAsync();
    }

    public async Task<ulong> SaveItemAsync(T item, bool trackSync = true)
    {
        

        if (item.Id == 0)
        {
            try
            {
                await _connection.InsertAsync(item);
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
                await _connection.InsertAsync(new SyncQueueItem
                {
                    EntityType = typeof(T).Name,
                    EntityId = item.Id,
                    OperationType = OperationType.Create,
                    PayloadJson = JsonSerializer.Serialize(item)
                });
            }
        }
        else
        {
            var result = await _connection.UpdateAsync(item);

            if (result == 0)
            {
                try
                {
                    await _connection.InsertOrReplaceAsync(item);
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
                await _connection.InsertAsync(new SyncQueueItem
                {
                    EntityType = typeof(T).Name,
                    EntityId = item.Id,
                    OperationType = OperationType.Update,
                    PayloadJson = JsonSerializer.Serialize(item)
                });
            }
        }
        return (ulong)item.GetHashCode();
    }
}
