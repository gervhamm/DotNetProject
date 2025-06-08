namespace ArcsomAssetManagement.Client.DTOs.Business;

public class AssetDto
{
    public ulong Id { get; set; }
    public string Name { get; set; }
    public ProductDto ProductDto { get; set; }
}
