using ArcsomAssetManagement.Client.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ArcsomAssetManagement.Client.Services;

public class SeedDataService
{
    private readonly ProductRepository _productRepository;
    private readonly ManufacturerRepository _manufacturerRepository;
    private readonly AssetRepository _assetRepository;
    private readonly ModalErrorHandler _errorHandler;

    private readonly AppFaker _faker;

    private readonly string _seedDataFilePath = "SeedData.json";
    private readonly ILogger<SeedDataService> _logger;

    public SeedDataService(ProductRepository productRepository, ManufacturerRepository manufacturerRepository, AssetRepository assetRepository, AppFaker appFaker, ILogger<SeedDataService> logger, ModalErrorHandler errorHandler)
    {
        _productRepository = productRepository;
        _manufacturerRepository = manufacturerRepository;
        _assetRepository = assetRepository;
        _faker = appFaker;
        _logger = logger;
        _errorHandler = errorHandler;
    }

    public async Task LoadSeedDataAsync()
    {
        await ClearTables();      

        AssetsJson? payloadAssets = await DeserializeProducts(_seedDataFilePath);
        var source = new CancellationTokenSource();
        source.CancelAfter(TimeSpan.FromSeconds(10));
        var token = source.Token;
        try
        {
            await _faker.SeedManufacturers(token);
            await _faker.SeedProducts(token);
            await _faker.SeedAssets(token);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error saving seed data");
            throw;
        }
    }

    private async Task<AssetsJson?> DeserializeProducts(string seedDataFilePath)
    {
        await using Stream templateStreamProducts = await FileSystem.OpenAppPackageFileAsync(seedDataFilePath);

        AssetsJson? payload = null;
        try
        {
            payload = JsonSerializer.Deserialize(templateStreamProducts, JsonContext.Default.AssetsJson);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error deserializing seed data");
        }
        return payload;
    }

    private async Task ClearTables()
    {
        try
        {
            await _assetRepository.ClearTableAsync();
            await _productRepository.ClearTableAsync();
            await _manufacturerRepository.ClearTableAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}