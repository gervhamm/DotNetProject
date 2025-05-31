using Newtonsoft.Json;

namespace ArcsomAssetManagement.Client.DTOs.Business;

public class ManufacturerDto
{
    public ulong Id { get; set; }
    public string Name { get; set; }
    public string Contact { get; set; }
    public List<ProductDto> ProductDtos {get; set;}
}
