using ArcsomAssetManagement.Client.Models;
using AutoMapper;

namespace ArcsomAssetManagement.Client.Data;

public class SyncRepository<TDomain, TDto> : IRepository<TDomain> where TDomain : class, IIdentifiable, new() where TDto : class, new()
{
    private readonly IOnlineRepository<TDto> _onlineRepository;
    private readonly IOfflineRepository<TDomain> _offlineRepository;
    private readonly IMapper _mapper;

    private bool _isOnline;
    private DateTime _lastOnlineCheck = DateTime.MinValue;
    private readonly TimeSpan _onlineCheckInterval = TimeSpan.FromSeconds(30);

    public SyncRepository(IOnlineRepository<TDto> onlineRepository, IOfflineRepository<TDomain> offlineRepository, IMapper mapper)
    {
        _onlineRepository = onlineRepository ?? throw new ArgumentNullException(nameof(onlineRepository));
        _offlineRepository = offlineRepository ?? throw new ArgumentNullException(nameof(offlineRepository));
        _mapper = mapper;
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
    public async Task<List<TDomain>> ListAsync(ulong id)
    {
        if (await IsOnlineAsync())
        {
            var dtos = await _onlineRepository.ListAsync();
            var domainItems = _mapper.Map<List<TDomain>>(dtos);
            foreach (var item in domainItems) await _offlineRepository.SaveItemAsync(item, trackSync: false);
            return domainItems;
        }
        return await _offlineRepository.ListAsync();
    }

    public async Task<List<TDomain>> ListAsync()
    {
        if (await IsOnlineAsync())
        {
            var dtos = await _onlineRepository.ListAsync();
            var domainItems = _mapper.Map<List<TDomain>>(dtos);
            foreach (var item in domainItems) await _offlineRepository.SaveItemAsync(item, trackSync: false);
            return domainItems;
        }
        return await _offlineRepository.ListAsync();
    }

    public async Task<TDomain?> GetAsync(ulong id)
    {
        if (await IsOnlineAsync())
        {
            var dto = await _onlineRepository.GetAsync(id);
            var domainItem = _mapper.Map<TDomain>(dto);
            if (domainItem != null) await _offlineRepository.SaveItemAsync(domainItem, trackSync: false);
            return domainItem;
        }
        return await _offlineRepository.GetAsync(id);
    }

    public async Task<ulong> SaveItemAsync(TDomain item)
    {
        if (await IsOnlineAsync())
        {
            var dto = _mapper.Map<TDto>(item);
            return await _onlineRepository.SaveItemAsync(dto);
        }

        return await _offlineRepository.SaveItemAsync(item, trackSync: true);
    }

    public async Task<int> DeleteItemAsync(TDomain item)
    {
        if (await IsOnlineAsync())
        {
            var dto = _mapper.Map<TDto>(item);
            return await _onlineRepository.DeleteItemAsync(dto);
        }

        return await _offlineRepository.DeleteItemAsync(item);
    }
}