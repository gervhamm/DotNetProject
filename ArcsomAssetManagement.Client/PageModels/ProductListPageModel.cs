using ArcsomAssetManagement.Client.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ArcsomAssetManagement.Client.PageModels;

public partial class ProductListPageModel : ObservableObject
{
    private readonly ProductRepository _productRepository;
    
    private List<Manufacturer> _manufacturers = [];

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Product> filteredProducts;

    [ObservableProperty]
    private ObservableCollection<Product> _products = [];

    public ProductListPageModel(ProductRepository productRepository)
    {
        _productRepository = productRepository;
        //_manufacturerRepository = manufacturerRepository;
    }

    [RelayCommand]
    private async Task Appearing()
    {
        var productList = await _productRepository.ListAsync();
        Products = new ObservableCollection<Product>(productList);

        FilteredProducts = Products;
    }

    [RelayCommand]
    Task NavigateToProduct(Product product)
        => Shell.Current.GoToAsync($"product?id={product.Id}");

    [RelayCommand]
    async Task AddProduct()
    {
        await Shell.Current.GoToAsync($"product");
    }
    [RelayCommand]
    private async Task FilterProducts()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            await Appearing();
        }
        else
        {
            var filtered = Products
                .Where(p => p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                            (p.Manufacturer?.Name?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
            FilteredProducts = new ObservableCollection<Product>(filtered);
        }
    }
}
