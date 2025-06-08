namespace ArcsomAssetManagement.Api.Models;

public class Asset
{
    public ulong Id { get; set; }
    public string Name { get; set; } = null!;
    public Product? Product { get; set; }
}
