using ArcsomAssetManagement.Client.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace ArcsomAssetManagement.Client.Data
{
    public class SeedDataService
    {
        private readonly ProductRepository _productRepository;
        private readonly ManufacturerRepository _manufacturerRepository;

        private readonly ProjectRepository _projectRepository;
        private readonly TaskRepository _taskRepository;
        private readonly TagRepository _tagRepository;
        private readonly CategoryRepository _categoryRepository;
        private readonly string _seedDataFilePath = "SeedData.json";
        private readonly ILogger<SeedDataService> _logger;

        public SeedDataService(ProductRepository productRepository, ManufacturerRepository manufacturerRepository, ProjectRepository projectRepository, TaskRepository taskRepository, TagRepository tagRepository, CategoryRepository categoryRepository, ILogger<SeedDataService> logger)
        {
            _productRepository = productRepository;
            _manufacturerRepository = manufacturerRepository;
            _logger = logger;

            _projectRepository = projectRepository;
            _taskRepository = taskRepository;
            _tagRepository = tagRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task LoadSeedDataAsync()
        {
            ClearTables();

            await using Stream templateStream = await FileSystem.OpenAppPackageFileAsync(_seedDataFilePath);

            ProjectsJson? payload = null;
            try
            {
                payload = JsonSerializer.Deserialize(templateStream, JsonContext.Default.ProjectsJson);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error deserializing seed data");
            }

            try
            {
                if (payload is not null)
                {
                    foreach (var project in payload.Projects)
                    {
                        if (project is null)
                        {
                            continue;
                        }

                        if (project.Category is not null)
                        {
                            await _categoryRepository.SaveItemAsync(project.Category);
                            project.CategoryID = project.Category.ID;
                        }

                        await _projectRepository.SaveItemAsync(project);

                        if (project?.Tasks is not null)
                        {
                            foreach (var task in project.Tasks)
                            {
                                task.ProjectID = project.ID;
                                await _taskRepository.SaveItemAsync(task);
                            }
                        }

                        if (project?.Tags is not null)
                        {
                            foreach (var tag in project.Tags)
                            {
                                await _tagRepository.SaveItemAsync(tag, project.ID);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error saving seed data");
                throw;
            }

            ProductsJson? payloadProducts = await DeserializeProducts(_seedDataFilePath);

            try
            {
                if (payloadProducts is not null)
                {
                    var existingManufacturers = (await _manufacturerRepository.ListAsync()).ToDictionary(m => m.Name, m => m);

                    foreach (var product in payloadProducts.Products)
                    {
                        if (product is null)
                        {
                            continue;
                        }

                        if (product.Manufacturer is not null)
                        {
                            if (existingManufacturers.TryGetValue(product.Manufacturer.Name, out var existingManufacturer))
                            {
                                product.Manufacturer = existingManufacturer;
                            }
                            else
                            {
                                await _manufacturerRepository.SaveItemAsync(product.Manufacturer);
                                existingManufacturers[product.Manufacturer.Name] = product.Manufacturer;
                            }
                        }

                        await _productRepository.SaveItemAsync(product);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error saving seed data");
                throw;
            }
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
                await Task.WhenAll(
                    _projectRepository.DropTableAsync(),
                    _taskRepository.DropTableAsync(),
                    _tagRepository.DropTableAsync(),
                    _categoryRepository.DropTableAsync());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}