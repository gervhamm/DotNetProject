using ArcsomAssetManagement.Client.Models;

namespace ArcsomAssetManagement.Client.Data;

public class SyncRepository<T> : IRepository<T> where T : class, IIdentifiable, new()
{
    private readonly IOnlineRepository<T> _onlineRepository;
    private readonly IOfflineRepository<T> _offlineRepository;

    private bool _isOnline;
    private DateTime _lastOnlineCheck = DateTime.MinValue;
    private readonly TimeSpan _onlineCheckInterval = TimeSpan.FromSeconds(30);

    public SyncRepository(IOnlineRepository<T> onlineRepository, IOfflineRepository<T> offlineRepository)
    {
        _onlineRepository = onlineRepository ?? throw new ArgumentNullException(nameof(onlineRepository));
        _offlineRepository = offlineRepository ?? throw new ArgumentNullException(nameof(offlineRepository));
    }

    private async Task<bool> IsOnlineAsync()
    {

        if (DateTime.UtcNow - _lastOnlineCheck < _onlineCheckInterval)
            return _isOnline;

        try
        {
            _isOnline = await _onlineRepository.PingAsync();// (Connectivity.NetworkAccess == NetworkAccess.Internet) if app is using internet
        }
        catch
        {
            _isOnline = false;
        }

        _lastOnlineCheck = DateTime.UtcNow;
        return _isOnline;
    }

    public async Task<List<T>> ListAsync()
    {
        if (await IsOnlineAsync())
        {
            var data = await _onlineRepository.ListAsync();
            foreach (var item in data) await _offlineRepository.SaveItemAsync(item, trackSync: false);
            return data;
        }
        return await _offlineRepository.ListAsync();
    }

    public async Task<T?> GetAsync(ulong id)
    {
        if (await IsOnlineAsync())
        {
            var item = await _onlineRepository.GetAsync(id);
            if (item != null) await _offlineRepository.SaveItemAsync(item, trackSync: false);
            return item;
        }
        return await _offlineRepository.GetAsync(id);
    }

    public async Task<ulong> SaveItemAsync(T item)
    {
        if (await IsOnlineAsync())
            return await _onlineRepository.SaveItemAsync(item);

        return await _offlineRepository.SaveItemAsync(item, trackSync: true);
    }

    public async Task<int> DeleteItemAsync(T item)
    {
        if (await IsOnlineAsync())
            return await _onlineRepository.DeleteItemAsync(item);

        return await _offlineRepository.DeleteItemAsync(item);
    }
}