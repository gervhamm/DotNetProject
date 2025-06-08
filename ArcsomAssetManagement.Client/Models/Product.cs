using SQLite;
using SQLiteNetExtensions.Attributes;

namespace ArcsomAssetManagement.Client.Models;

public class Product : IIdentifiable
{
    [PrimaryKey, AutoIncrement]
    public ulong Id { get; set; }
    public string Name { get; set; } = string.Empty;

    [ForeignKey(typeof(Manufacturer))]
    public ulong ManufacturerId { get; set; }

    [ManyToOne]
    public Manufacturer? Manufacturer { get; set; }
    public override string ToString() => $"{Name}";
}
public class ProductsJson
{
    public List<Product> Products { get; set; } = [];
}