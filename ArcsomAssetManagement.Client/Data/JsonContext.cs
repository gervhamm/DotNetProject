using ArcsomAssetManagement.Client.Models;
using System.Text.Json.Serialization;

[JsonSerializable(typeof(Product))]
[JsonSerializable(typeof(Manufacturer))]
[JsonSerializable(typeof(Asset))]
[JsonSerializable(typeof(ProductsJson))]
public partial class JsonContext : JsonSerializerContext
{
}