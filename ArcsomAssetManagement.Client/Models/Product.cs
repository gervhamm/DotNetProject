using SQLite;

namespace ArcsomAssetManagement.Client.Models;

public class Product
{
    [PrimaryKey, AutoIncrement]
    public ulong Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Manufacturer? Manufacturer { get; set; }
    public override string ToString() => $"{Name}";
}
public class ProductsJson
{
    public List<Product> Products { get; set; } = [];
}