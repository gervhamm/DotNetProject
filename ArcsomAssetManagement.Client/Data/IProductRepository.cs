using ArcsomAssetManagement.Client.Models;

namespace ArcsomAssetManagement.Client.Data;

public interface IProductRepository : IRepository<Product>
{
    Task<List<Product>> ListAsync(ulong manufacturerId);
}
