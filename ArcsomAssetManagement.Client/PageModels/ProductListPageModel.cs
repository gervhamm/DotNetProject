using ArcsomAssetManagement.Client.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ArcsomAssetManagement.Client.PageModels;

public partial class ProductListPageModel : ObservableObject
{
    private readonly ProductRepository _productRepository;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Product> filteredProducts;

    [ObservableProperty]
    private ObservableCollection<Product> _products = [];

    public ProductListPageModel(ProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    [RelayCommand]
    private async Task Appearing()
    {
        Products = await _productRepository.ListAsync();
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
        //if (string.IsNullOrWhiteSpace(SearchText))
        //{
        //    filteredProducts = await _productRepository.ListAsync();
        //}
        //else
        //{
        //    var filtered = Products
        //        .Where(p => p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
        //                    (p.Contact?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false))
        //        .ToList();
        //    FilteredProducts = new ObservableCollection<Product>(filtered);
        //}
    }
}
