using SQLite;
using SQLiteNetExtensions.Attributes;

namespace ArcsomAssetManagement.Client.Models;

public class Manufacturer
{
    [PrimaryKey, AutoIncrement]
    public ulong Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Contact { get; set; }

    [OneToMany(CascadeOperations = CascadeOperation.All)]
    public ICollection<Product>? Products { get; set; }
    public override string ToString() => $"{Name}";
}
