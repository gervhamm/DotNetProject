using ArcsomAssetManagement.Client.Models;

namespace ArcsomAssetManagement.Client.Services;

public class AppFaker
{
    private readonly AssetRepository _assetRepository;
    private readonly ProductRepository _productRepository;
    private readonly ManufacturerRepository _manufacturerRepository;

    public List<Manufacturer> Manufacturers { get; set; }
    public List<Product> Products { get; set; }
    public List<Asset> Assets { get; set; }
    public AppFaker(AssetRepository assetRepository, ProductRepository productRepository, ManufacturerRepository manufacturerRepository)
    {
        _assetRepository = assetRepository;
        _productRepository = productRepository;
        _manufacturerRepository = manufacturerRepository;
        Manufacturers = new List<Manufacturer>();
        Products = new List<Product>();
        Assets = new List<Asset>();
    }

    public async Task SeedManufacturers(CancellationToken stoppingToken)
    {
        Manufacturers.Clear();
        for (int i = 0; i < 10; i++)
        {
            // Seed data for the manufacturers table using Faker
            var manufacturer = new Manufacturer()
            {
                Name = Faker.Company.Name(),
                Contact = Faker.Internet.Email()
            };
            await _manufacturerRepository.SaveItemAsync(manufacturer, true);
            Manufacturers.Add(manufacturer);
        }
    }

    public async Task SeedProducts(CancellationToken stoppingToken)
    {
        Products.Clear();
        if (Manufacturers.Count == 0)
        {
            await SeedManufacturers(stoppingToken);
        }

        for (int i = 0; i < 100; i++)
        {
            // Seed data for the products table using Faker
            var manufacturer = Manufacturers[Faker.RandomNumber.Next(0, Manufacturers.Count - 1)];
            var product = new Product()
            {
                Name = Faker.Lorem.Words(1).FirstOrDefault(),
                Manufacturer = manufacturer,
                ManufacturerId = manufacturer.Id
            };
            await _productRepository.SaveItemAsync(product, true);
            Products.Add(product);
        }
    }

    public async Task SeedAssets(CancellationToken stoppingToken)
    {
        if(Products.Count == 0)
        {
            await SeedProducts(stoppingToken);
        }

        for (int i = 0; i < Products.Count; i++)
        {
            // Seed data for the assets table using Faker
            var product = Products[Faker.RandomNumber.Next(0, Products.Count - 1)];
            var asset = new Asset()
            {
                Name = "Asset " + Faker.Lorem.Words(1).FirstOrDefault() + " " + Faker.RandomNumber.Next(10).ToString(),
                Product = product,
                ProductId = product.Id,
            };
            await _assetRepository.SaveItemAsync(asset, true);
        }
    }
}
