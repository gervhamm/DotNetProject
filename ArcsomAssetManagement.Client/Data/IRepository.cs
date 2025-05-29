using ArcsomAssetManagement.Client.Models;

namespace ArcsomAssetManagement.Client.Data;

public interface IRepository<T> where T : class, IIdentifiable, new()
{
    Task<List<T>> ListAsync();
    Task<T?> GetAsync(ulong id);
    Task<ulong> SaveItemAsync(T item);
    Task<int> DeleteItemAsync(T item);
}
