using ArcsomAssetManagement.Client.Models;
using ArcsomAssetManagement.Client.PageModels.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ArcsomAssetManagement.Client.PageModels;

public partial class ProductListPageModel : BasePageModel
{
    private readonly ProductRepository _productRepository;
    
    private List<Manufacturer> _manufacturers = [];

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private List<Product> filteredProducts;

    [ObservableProperty]
    private List<Product> _products = [];

    [ObservableProperty]
    private PaginationModel _pagination;

    [ObservableProperty]
    private List<PageNumberItem> _pageNumbers = [];

    [ObservableProperty]
    private int selectedPage;

    public ProductListPageModel(ProductRepository productRepository, AuthService authService) : base(authService)
    {
        _productRepository = productRepository;
        _pagination = new PaginationModel
        {
            CurrentPage = 1,
            PageSize = 3,
            TotalItems = 10
        };
        PageNumbers = [new PageNumberItem { Number = "1", IsCurrent = true }];
    }

    [RelayCommand]
    private async Task Appearing()
    {
        await CheckAuthAsync();
        (Products, _pagination) = await _productRepository.ListAsync(pageNumber: _pagination.CurrentPage, pageSize: _pagination.PageSize);
        FilteredProducts = Products;
        PageNumbers = PaginationHelper.SetPagenumbers(_pagination.CurrentPage, _pagination.TotalPages);
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
            (List<Product> Products, PaginationModel pagination) = await _productRepository.ListAsync(pageNumber: _pagination.CurrentPage, pageSize: _pagination.PageSize, filter: SearchText);
            var filtered = Products
                .Where(p => p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                            (p.Manufacturer?.Name?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
            FilteredProducts = filtered;
        }
    }
    [RelayCommand]
    private async Task GoToPageAsync(string pageNumber)
    {
        var newPageNumber = _pagination.CurrentPage;

        switch (pageNumber)
        {
            case "Next":
                if (Pagination.CurrentPage < Pagination.TotalPages)
                    newPageNumber = Pagination.CurrentPage + 1;
                else
                    return;
                break;
            case "Previous":
                if (Pagination.CurrentPage > 1)
                    newPageNumber = Pagination.CurrentPage - 1;
                else
                    return;
                break;
            default:
                if (!int.TryParse(pageNumber, out _))
                {
                    return;
                }
                newPageNumber = int.Parse(pageNumber);
                break;
        }
        Pagination.CurrentPage = newPageNumber;
        Appearing();
    }
}
