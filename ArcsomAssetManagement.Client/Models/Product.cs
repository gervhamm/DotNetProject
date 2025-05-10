using SQLite;

namespace ArcsomAssetManagement.Client.Models;

public class Product
{
    [PrimaryKey, AutoIncrement]
    public ulong Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Manufacturer? Manufacturer { get; set; }
}
