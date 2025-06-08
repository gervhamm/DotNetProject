namespace ArcsomAssetManagement.Client.DTOs.Business;

public class ProductDto
{
    public ulong Id { get; set; }
    public string Name { get; set; }
    public ManufacturerDto ManufacturerDto { get; set; }
}
