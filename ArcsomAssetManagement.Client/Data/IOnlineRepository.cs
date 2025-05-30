using ArcsomAssetManagement.Client.Models;

namespace ArcsomAssetManagement.Client.Data;

public interface IOnlineRepository<T>
{
    Task<List<T>> ListAsync();
    Task<T?> GetAsync(ulong id);
    Task<ulong> SaveItemAsync(T item);
    Task<int> DeleteItemAsync(T item);
    Task<bool> PingAsync();
}
