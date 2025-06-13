using ArcsomAssetManagement.Client.Models;
using AutoMapper;
using SQLite;
using System.Text.Json;

namespace ArcsomAssetManagement.Client.Services;

public class EntitySyncService<TDomain, TDto> 
    where TDomain : class, IIdentifiable, new()
    where TDto : class, new()
{
    private readonly SQLiteAsyncConnection _database;
    private readonly IOnlineRepository<TDto> _onlineRepository;
    private readonly IMapper _mapper;

    public EntitySyncService(SQLiteAsyncConnection database, IOnlineRepository<TDto> onlineRepository, IMapper mapper)
    {
        _database = database;
        _onlineRepository = onlineRepository;
        _mapper = mapper;
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
            if(item.EntityType == typeof(TDomain).Name)
            {
                var entity = JsonSerializer.Deserialize<TDomain>(item.PayloadJson);
                var dto = _mapper.Map<TDto>(entity);
                switch (item.OperationType)
                {
                    case OperationType.Create:
                        await _onlineRepository.SaveItemOnlineAsync(dto);
                        break;
                    case OperationType.Update:
                        await _onlineRepository.UpdateItemOnlineAsync(dto);
                        break;
                    case OperationType.Delete:
                        await _onlineRepository.DeleteItemOnlineAsync(entity.Id);
                        break;
                }

                await _database.DeleteAsync(item);
            }
        }
    }

    public async Task PullLatestRemoteChanges()
    {
        await _database.DeleteAllAsync<TDomain>();
        await _database.ExecuteAsync("DELETE FROM sqlite_sequence WHERE name = ?", typeof(TDomain).Name);
        var dtos = await _onlineRepository.ListOnlineAsync();
        await _onlineRepository.SaveToOfflineAsync(dtos);
    }


}