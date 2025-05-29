using ArcsomAssetManagement.Client.Data;
using ArcsomAssetManagement.Client.Models;
using SQLite;
using System.Text.Json;

namespace ArcsomAssetManagement.Client.Services;

public class SyncService<T> where T : class, IIdentifiable, new()
{
    private readonly SQLiteAsyncConnection _database;
    private readonly IOnlineRepository<T> _onlineRepository;

    public SyncService(SQLiteAsyncConnection database, IOnlineRepository<T> onlineRepository)
    {
        _database = database;
        _onlineRepository = onlineRepository;
        _ = Init();
    }

    private async Task Init()
    {
        await _database.CreateTableAsync<SyncQueueItem>();
    }
    public async Task ProcessSyncQueueAsync()
    {
        var queueItems = await _database.Table<SyncQueueItem>().ToListAsync();

        foreach (var item in queueItems)
        {
            var entity = JsonSerializer.Deserialize<T>(item.PayloadJson);

            switch (item.OperationType)
            {
                case OperationType.Create:
                    await _onlineRepository.SaveItemAsync(entity);
                    break;
                case OperationType.Update:
                    await _onlineRepository.SaveItemAsync(entity);
                    break;
                case OperationType.Delete:
                    await _onlineRepository.DeleteItemAsync(entity);
                    break;
            }

            await _database.DeleteAsync(item);
        }
    }
}