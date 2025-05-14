using ArcsomAssetManagement.Client.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ArcsomAssetManagement.Client.PageModels;

public partial class ProductDetailPageModel : ObservableObject, IQueryAttributable
{
    public const string ManufacturerQueryKey = "manufacturer";

    private Product? _product;
    private ProductRepository _productRepository;

    private readonly ModalErrorHandler _errorHandler;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _manufacturerName = string.Empty;

    [ObservableProperty]
    bool _isBusy;
    public ProductDetailPageModel(ProductRepository productRepository, ModalErrorHandler errorHandler)
    {
        _productRepository = productRepository;

        _errorHandler = errorHandler;
    }

    [RelayCommand]
    private async Task LoadData(int id)
    {
        try
        {
            IsBusy = true;

            _product = await _productRepository.GetAsync(id);

            if (_product.IsNullOrNew())
            {
                _errorHandler.HandleError(new Exception($"Product with id {id} could not be found."));
                return;
            }

            Name = _product.Name;
            ManufacturerName = _product.Manufacturer.Name ?? "Unknown";
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
            _product = new();
        }
    }
    private async Task RefreshData()
    {
        if (_product.IsNullOrNew())
        {
            return;
        }
    }
    [RelayCommand]
    private async Task Save()
    {
        if (_product is null)
        {
            _errorHandler.HandleError(
                new Exception("Product is null. Cannot Save."));

            return;
        }

        _product.Name = Name;
        _product.Manufacturer = new Manufacturer
        {
            Name = "TODO",
            Contact= "TODO"
        };
        await _productRepository.SaveItemAsync(_product);

        await Shell.Current.GoToAsync("..");
        // TODO: await AppShell.DisplayToastAsync("Product saved");
    }

    [RelayCommand]
    private async Task Delete()
    {
        if (_product.IsNullOrNew())
        {
            await Shell.Current.GoToAsync("..");
            return;
        }

        await _productRepository.DeleteItemAsync(_product);
        await Shell.Current.GoToAsync("..");
        //TODO: await AppShell.DisplayToastAsync("Product deleted");
    }
}
