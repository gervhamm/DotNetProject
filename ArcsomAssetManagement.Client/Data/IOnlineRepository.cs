namespace ArcsomAssetManagement.Client.Data;

public interface IOnlineRepository<T> where T : class, new()
{
    public Task<IEnumerable<T>> ListOnlineAsync();
    public Task SaveItemOnlineAsync(T dto);
    public Task UpdateItemOnlineAsync(T dto);
    public Task<bool> DeleteItemOnlineAsync(ulong id);
    public Task SaveToOfflineAsync(IEnumerable<T> items);
}
