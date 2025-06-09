using ArcsomAssetManagement.Client.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ArcsomAssetManagement.Client.Services;

public class SeedDataService
{
    private readonly ProductRepository _productRepository;
    private readonly ManufacturerRepository _manufacturerRepository;

    private readonly string _seedDataFilePath = "SeedData.json";
    private readonly ILogger<SeedDataService> _logger;

    public SeedDataService(ProductRepository productRepository, ManufacturerRepository manufacturerRepository, ILogger<SeedDataService> logger)
    {
        _productRepository = productRepository;
        _manufacturerRepository = manufacturerRepository;
        _logger = logger;
;
    }

    public async Task LoadSeedDataAsync()
    {
        ClearTables();      

        ProductsJson? payloadProducts = await DeserializeProducts(_seedDataFilePath);

        //try
        //{
        //    if (payloadProducts is not null)
        //    {
        //        var existingManufacturers = (await _manufacturerRepository.ListAsync()).ToDictionary(m => m.Name, m => m);

        //        foreach (var product in payloadProducts.Products)
        //        {
        //            if (product is null)
        //            {
        //                continue;
        //            }

        //            if (product.Manufacturer is not null)
        //            {
        //                if (existingManufacturers.TryGetValue(product.Manufacturer.Name, out var existingManufacturer))
        //                {
        //                    product.Manufacturer = existingManufacturer;
        //                }
        //                else
        //                {
        //                    await _manufacturerRepository.SaveItemAsync(product.Manufacturer);
        //                    existingManufacturers[product.Manufacturer.Name] = product.Manufacturer;
        //                }
        //            }

        //            await _productRepository.SaveItemAsync(product);
        //        }
        //    }
        //}
        //catch (Exception e)
        //{
        //    _logger.LogError(e, "Error saving seed data");
        //    throw;
        //}
    }

    private async Task<ProductsJson?> DeserializeProducts(string seedDataFilePath)
    {
        await using Stream templateStreamProducts = await FileSystem.OpenAppPackageFileAsync(seedDataFilePath);

        ProductsJson? payload = null;
        try
        {
            payload = JsonSerializer.Deserialize(templateStreamProducts, JsonContext.Default.ProductsJson);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error deserializing seed data");
        }
        return payload;
    }

    private async void ClearTables()
    {
        try
        {
            //await Task.WhenAll(
            //    _projectRepository.DropTableAsync());
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}