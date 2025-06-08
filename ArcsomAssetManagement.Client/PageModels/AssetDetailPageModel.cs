using ArcsomAssetManagement.Client.Models;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ArcsomAssetManagement.Client.PageModels;

public partial class AssetDetailPageModel : ObservableObject, IQueryAttributable
{
    public const string ProductQueryKey = "product";

    private AssetRepository _assetRepository;
    private ProductRepository _productRepository;

    private readonly ModalErrorHandler _errorHandler;


    [ObservableProperty]
    private Asset? _asset = new Asset();

    [ObservableProperty]
    private Product? _product;

    [ObservableProperty]
    private ObservableCollection<Product> _products = [];

    [ObservableProperty]
    private Product? _selectedProduct;

    [ObservableProperty]
    private int _selectedProductIndex;

    public AssetDetailPageModel(AssetRepository assetRepository, ProductRepository productRepository, ModalErrorHandler errorHandler)
    {
        _assetRepository = assetRepository;
        _productRepository = productRepository;
        _errorHandler = errorHandler;
    }
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        LoadAssetAsync(query).FireAndForgetSafeAsync(_errorHandler);
    }
    private async Task LoadAssetAsync(IDictionary<string, object> query)
    {
        if (query.TryGetValue(ProductQueryKey, out var product))
            Product = (Product)product;

        ulong assetId = 0;

        if (query.ContainsKey("id"))
        {
            assetId = Convert.ToUInt64(query["id"]);

            try
            {
                Asset = await _assetRepository.GetAsync(assetId);

                if (Asset.IsNullOrNew())
                {
                    _errorHandler.HandleError(new Exception($"Asset with id {assetId} could not be found."));
                    return;
                }
                await LoadProducts();

                SelectedProduct = Asset.Product;
                if (SelectedProduct != null)
                {
                    SelectedProductIndex = Products.ToList().FindIndex(m => string.Equals(m.Name, SelectedProduct.Name, StringComparison.OrdinalIgnoreCase));
                    Product = SelectedProduct;
                }
            }
            catch (Exception e)
            {
                _errorHandler.HandleError(e);
            }
        }
        else
        {
            await LoadProducts();
            _asset = new();

            if (Product is not null) //Navigated here from Product Detail Page
            {
                SelectedProduct = Product;
                SelectedProductIndex = Products.ToList().FindIndex(p => p.Id == Product.Id);
            }
            ;
        }
    }

    [RelayCommand]
    private async Task Save()
    {
        if (string.IsNullOrWhiteSpace(Asset.Name) || SelectedProduct == null)
        {
            _errorHandler.HandleError(
                new Exception("Please fill in all fields. Cannot Save."));

            return;
        }

        Asset.Product = SelectedProduct;
        Asset.ProductId = Asset.Product.Id;
        try
        {
            await _assetRepository.SaveItemAsync(Asset, true);
        }
        catch (Exception e)
        {
            _errorHandler.HandleError(e);
        }

        await Shell.Current.GoToAsync("..");
        // TODO: await AppShell.DisplayToastAsync("Asset saved");
    }

    [RelayCommand]
    private async Task Delete()
    {
        if (Asset.IsNullOrNew())
        {
            await Shell.Current.GoToAsync("..");
            return;
        }

        await _assetRepository.DeleteItemAsync(Asset);
        await Shell.Current.GoToAsync("..");
        //TODO: await AppShell.DisplayToastAsync("Asset deleted");
    }
    private async Task LoadProducts()
    {
        var productsList = await _productRepository.ListAsync();
        Products = productsList.ToObservableCollection();
        Products.Insert(0, new Product { Id = 0, Name = "--None--", ManufacturerId = 0, Manufacturer = new Manufacturer() });
    }
}
