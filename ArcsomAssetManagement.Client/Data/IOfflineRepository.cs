using ArcsomAssetManagement.Client.Models;

namespace ArcsomAssetManagement.Client.Data;

public interface IOfflineRepository<T> where T : class, IIdentifiable, new()
{
    Task<List<T>> ListAsync();
    Task<T?> GetAsync(ulong id);
    Task<ulong> SaveItemAsync(T item, bool trackSync);
    Task<int> DeleteItemAsync(T item);
}
