using ArcsomAssetManagement.Client.Models;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ArcsomAssetManagement.Client.PageModels;

public partial class ProductDetailPageModel : ObservableObject, IQueryAttributable
{
    public const string ManufacturerQueryKey = "manufacturer";

    private ProductRepository _productRepository;
    private ManufacturerRepository _manufacturerRepository;

    private readonly ModalErrorHandler _errorHandler;


    [ObservableProperty]
    private Product? _product = new Product();

    [ObservableProperty]
    private Manufacturer? _manufacturer;

    [ObservableProperty]
    private ObservableCollection<Manufacturer> _manufacturers = [];

    [ObservableProperty]
    private Manufacturer? _selectedManufacturer;

    [ObservableProperty]
    private int _selectedManufacturerIndex;

    public ProductDetailPageModel(ProductRepository productRepository,ManufacturerRepository manufacturerRepository ,ModalErrorHandler errorHandler)
    {
        _productRepository = productRepository;
        _manufacturerRepository = manufacturerRepository;
        _errorHandler = errorHandler;
    }
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        LoadProductAsync(query).FireAndForgetSafeAsync(_errorHandler);
    }
    private async Task LoadProductAsync(IDictionary<string, object> query)
    {
        if (query.TryGetValue(ManufacturerQueryKey, out var manufacturer))
            Manufacturer = (Manufacturer)manufacturer;

        ulong productId = 0;

        if (query.ContainsKey("id"))
        {
            productId = Convert.ToUInt64(query["id"]);

            try
            {
                Product = await _productRepository.GetAsync(productId);

                if (Product.IsNullOrNew())
                {
                    _errorHandler.HandleError(new Exception($"Product with id {productId} could not be found."));
                    return;
                }
                await LoadManufacturers();

                SelectedManufacturer = Product.Manufacturer;
                if (SelectedManufacturer != null)
                {
                    SelectedManufacturerIndex = Manufacturers.ToList().FindIndex(m => string.Equals(m.Name, SelectedManufacturer.Name, StringComparison.OrdinalIgnoreCase));
                    Manufacturer = SelectedManufacturer;
                }
            }
            catch (Exception e)
            {
                _errorHandler.HandleError(e);
            }
        }
        else
        {
            await LoadManufacturers();
            _product = new();

            if (Manufacturer is not null) //Navigated here from Manufacturer Detail Page
            {
                SelectedManufacturer = Manufacturer;
                SelectedManufacturerIndex = Manufacturers.ToList().FindIndex(p => p.Id == Manufacturer.Id);
            }
            ;
        }
    }

    [RelayCommand]
    private async Task Save()
    {
        if (string.IsNullOrWhiteSpace(Product.Name) || SelectedManufacturer == null)
        {
            _errorHandler.HandleError(
                new Exception("Please fill in all fields. Cannot Save."));

            return;
        }

        Product.Manufacturer = SelectedManufacturer;
        Product.ManufacturerId = Product.Manufacturer.Id;
        try
        {
            await _productRepository.SaveItemAsync(Product, true);
        }
        catch (Exception e)
        {
            _errorHandler.HandleError(e);
        }

        await Shell.Current.GoToAsync("..");
        // TODO: await AppShell.DisplayToastAsync("Product saved");
    }

    [RelayCommand]
    private async Task Delete()
    {
        if (Product.IsNullOrNew())
        {
            await Shell.Current.GoToAsync("..");
            return;
        }

        await _productRepository.DeleteItemAsync(Product);
        await Shell.Current.GoToAsync("..");
        //TODO: await AppShell.DisplayToastAsync("Product deleted");
    }
    private async Task LoadManufacturers()
    {
        var manufacturersList = await _manufacturerRepository.ListAsync();
        Manufacturers = manufacturersList.ToObservableCollection();
        Manufacturers.Insert(0, new Manufacturer { Id = 0, Name = "--None--", Contact = null, Products = new List<Product>() });
    }
}
