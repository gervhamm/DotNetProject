using ArcsomAssetManagement.Client.Models;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ArcsomAssetManagement.Client.PageModels;

public partial class ManufacturerDetailPageModel : BasePageModel, IQueryAttributable
{
    private Manufacturer? _manufacturer;
    private ManufacturerRepository _manufacturerRepository;

    private readonly ModalErrorHandler _errorHandler;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _contact = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Product> _products = [];

    [ObservableProperty]
    bool _isBusy;
    public ManufacturerDetailPageModel(ManufacturerRepository manufacturerRepository, ModalErrorHandler errorHandler, AuthService authService) : base(authService)
    {
        _manufacturerRepository = manufacturerRepository;

        _errorHandler = errorHandler;
    }

    [RelayCommand]
    private async Task Appearing()
    {
        await CheckAuthAsync();
    }

    [RelayCommand]
    private async Task LoadData(int id)
    {
        try
        {
            IsBusy = true;

            _manufacturer = await _manufacturerRepository.GetAsync((ulong)id);

            if (_manufacturer.IsNullOrNew())
            {
                _errorHandler.HandleError(new Exception($"Manufacturer with id {id} could not be found."));
                return;
            }

            Name = _manufacturer.Name;
            Contact = _manufacturer.Contact;
            Products = _manufacturer.Products.ToObservableCollection();
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

        //Products = new ObservableCollection<Product>( await _productRepository.ListAsync(_manufacturer.Id));
//_manufacturer.Products = Products;
    }
    [RelayCommand]
    private async Task Save()
    {
        if (_manufacturer is null)
        {
            _errorHandler.HandleError(
                new Exception("Manufacturer is null. Cannot Save."));

            return;
        }

        _manufacturer.Name = Name;
        _manufacturer.Contact = Contact;
        try
        {
            await _manufacturerRepository.SaveItemAsync(_manufacturer, true);
        }
        catch (Exception ex)
        {
            // await AppShell.Current.DisplayAlert("Notification", $"An error occurred: {ex.Message}", "OK");
            _errorHandler.HandleError(ex);
            return;
        }

        //if (_manufacturer.IsNullOrNew())
        //{
        //    foreach (var product in Products)
        //    {
        //        product.Manufacturer = _manufacturer;
        //        await _productRepository.SaveItemAsync(product);
        //    }
        //}

        await Shell.Current.GoToAsync("..");
        // TODO: await AppShell.DisplayToastAsync("Manufacturer saved");
    }

    [RelayCommand]
    private async Task AddProduct()
    {
        if (_manufacturer is null)
        {
            _errorHandler.HandleError(
                new Exception("Manufacturer is null. Cannot navigate to product."));

            return;
        }

        // Pass the manufacturer so if this is a new manufacturer we can just add
        // the products to the manufacturer and then save them all from here.
        await Shell.Current.GoToAsync($"product",
            new ShellNavigationQueryParameters(){
                {ProductDetailPageModel.ManufacturerQueryKey, _manufacturer}
            });
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

    [RelayCommand]
    Task NavigateToProduct(Product product)
        => Shell.Current.GoToAsync($"product?id={product.Id}");
}
