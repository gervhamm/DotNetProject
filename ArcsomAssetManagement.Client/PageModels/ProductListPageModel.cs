using ArcsomAssetManagement.Client.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ArcsomAssetManagement.Client.PageModels;

public partial class ProductListPageModel : ObservableObject
{
    private readonly ProductRepository _productRepository;

    [ObservableProperty]
    private List<Product> _products = [];

    public ProductListPageModel(ProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    [RelayCommand]
    private async Task Appearing()
    {
        Products = await _productRepository.ListAsync();
    }
}
