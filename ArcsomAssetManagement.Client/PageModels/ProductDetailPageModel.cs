using ArcsomAssetManagement.Client.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ArcsomAssetManagement.Client.PageModels;

public partial class ProductDetailPageModel : ObservableObject, IQueryAttributable
{
    public const string ManufacturerQueryKey = "manufacturer";

    private ProductRepository2 _productRepository;
    private ManufacturerRepository _manufacturerRepository;

    private readonly ModalErrorHandler _errorHandler;


    [ObservableProperty]
    private Product? _product = new Product();

    [ObservableProperty]
    private ObservableCollection<Manufacturer> _manufacturers = [];

    [ObservableProperty]
    private Manufacturer? _selectedManufacturer;

    [ObservableProperty]
    private int _selectedManufacturerIndex;

    [ObservableProperty]
    private bool _isSaveEnabled;

    [ObservableProperty]
    private string _manufacturerName = string.Empty;

    [ObservableProperty]
    bool _isBusy;
    public ProductDetailPageModel(ProductRepository2 productRepository, ManufacturerRepository manufacturerRepository, ModalErrorHandler errorHandler)
    {
        _productRepository = productRepository;
        _manufacturerRepository = manufacturerRepository;
        _errorHandler = errorHandler;
    }

    [RelayCommand]
    private async Task LoadData(int id)
    {
        try
        {
            IsBusy = true;

            Product = await _productRepository.GetAsync(id);    

            if (Product.IsNullOrNew())
            {
                _errorHandler.HandleError(new Exception($"Product with id {id} could not be found."));
                return;
            }
            Manufacturers = await _manufacturerRepository.ListAsync(); 
            Manufacturers.Insert(0, new Manufacturer { Id = 0, Name = "Unknown Manufacturer" });
            SelectedManufacturer = Product.Manufacturer;
            if (SelectedManufacturer != null)
            {
                SelectedManufacturerIndex = Manufacturers.ToList().FindIndex(m => string.Equals(m.Name, SelectedManufacturer.Name, StringComparison.OrdinalIgnoreCase));
                ManufacturerName = SelectedManufacturer.Name;
            }

        }
        catch (Exception e)
        {
            _errorHandler.HandleError(e);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadManufacturers() =>
        Manufacturers = await _manufacturerRepository.ListAsync();

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("id"))
        {
            int id = Convert.ToInt32(query["id"]);
            LoadData(id).FireAndForgetSafeAsync(_errorHandler);
        }
        else if (query.ContainsKey("refresh"))
        {
            RefreshData().FireAndForgetSafeAsync(_errorHandler);
        }
        else
        {
            Task.WhenAll(LoadManufacturers()).FireAndForgetSafeAsync(_errorHandler);
            _product = new();
        }
    }
    private async Task RefreshData()
    {
        if (Product.IsNullOrNew())
        {
            return;
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
        await _productRepository.SaveItemAsync(Product);

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
}
