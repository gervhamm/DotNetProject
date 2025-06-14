﻿using SQLite;
using SQLiteNetExtensions.Attributes;

namespace ArcsomAssetManagement.Client.Models;

public class Manufacturer : IIdentifiable
{
    [PrimaryKey, AutoIncrement]
    public ulong Id { get; set; }
    [Unique]
    public string Name { get; set; } = null!;
    public string? Contact { get; set; }

    [OneToMany(CascadeOperations = CascadeOperation.All)]
    public ICollection<Product>? Products { get; set; }
    public override string ToString() => $"{Name}";
}
