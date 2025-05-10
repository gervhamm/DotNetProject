using SQLite;

namespace ArcsomAssetManagement.Client.Models;

public class Manufacturer
{
    [PrimaryKey, AutoIncrement]
    public ulong Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Contact { get; set; }
    public ICollection<Product>? Products { get; set; }
}
