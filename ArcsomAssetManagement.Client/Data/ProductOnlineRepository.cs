
using ArcsomAssetManagement.Client.Models;

namespace ArcsomAssetManagement.Client.Data;

public class ProductOnlineRepository : IOnlineRepository<Product>
{
    public Task<int> DeleteItemAsync(Product item)
    {
        throw new NotImplementedException();
    }

    public Task<Product?> GetAsync(ulong id)
    {
        throw new NotImplementedException();
    }

    public Task<List<Product>> ListAsync()
    {
        throw new NotImplementedException();
    }

    public Task<bool> PingAsync()
    {
        throw new NotImplementedException();
    }

    public Task<ulong> SaveItemAsync(Product item)
    {
        throw new NotImplementedException();
    }
}
