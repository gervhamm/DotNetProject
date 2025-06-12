using SQLite;
using SQLiteNetExtensions.Attributes;

namespace ArcsomAssetManagement.Client.Models;

public class Asset : IIdentifiable
{
    [PrimaryKey, AutoIncrement]
    public ulong Id { get; set; }
    public string Name { get; set; } = string.Empty;

    [ForeignKey(typeof(Product))]
    public ulong ProductId { get; set; }

    [ManyToOne]
    public Product? Product { get; set; }
    public override string ToString() => $"{Name}";
}
public class AssetsJson
{
    public List<Asset> Assets { get; set; } = [];
}