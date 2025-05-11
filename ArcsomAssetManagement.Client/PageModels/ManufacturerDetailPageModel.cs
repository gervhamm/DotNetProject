using ArcsomAssetManagement.Client.Models;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ArcsomAssetManagement.Client.PageModels;

public partial class ManufacturerDetailPageModel : ObservableObject, IQueryAttributable
{
    private Manufacturer? _manufacturer;
    private ManufacturerRepository _manufacturerRepository;
    private ProductRepository _productRepository;

    private readonly ModalErrorHandler _errorHandler;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _contact = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Product> _products = [];

    [ObservableProperty]
    bool _isBusy;
    public ManufacturerDetailPageModel(ManufacturerRepository manufacturerRepository, ProductRepository productRepository, ModalErrorHandler errorHandler)
    {
        _manufacturerRepository = manufacturerRepository;
        _productRepository = productRepository;

        _errorHandler = errorHandler;
    }

    [RelayCommand]
    private async Task LoadData(int id)
    {
        try
        {
            IsBusy = true;

            _manufacturer = await _manufacturerRepository.GetAsync(id);

            if (_manufacturer.IsNullOrNew())
            {
                _errorHandler.HandleError(new Exception($"Manufacturer with id {id} could not be found."));
                return;
            }

            Name = _manufacturer.Name;
            Contact = _manufacturer.Contact;
            Products = new ObservableCollection<Product>(await _productRepository.ListAsync(_manufacturer.Id));
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
            _manufacturer = new();
            _manufacturer.Products = [];
            Products = _manufacturer.Products.ToObservableCollection();
        }
    }
    private async Task RefreshData()
    {
        if (_manufacturer.IsNullOrNew())
        {
            if (_manufacturer is not null)
                Products = new(_manufacturer.Products);

            return;
        }

        Products = new ObservableCollection<Product>( await _productRepository.ListAsync(_manufacturer.Id));
        _manufacturer.Products = Products;
    }

    [RelayCommand]
    private async Task Delete()
    {
        if (_manufacturer.IsNullOrNew())
        {
            await Shell.Current.GoToAsync("..");
            return;
        }

        await _manufacturerRepository.DeleteItemAsync(_manufacturer);
        await Shell.Current.GoToAsync("..");
        //TODO: await AppShell.DisplayToastAsync("Manufacturer deleted");
    }
}
