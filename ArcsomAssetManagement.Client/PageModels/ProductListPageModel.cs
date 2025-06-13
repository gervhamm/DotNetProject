using ArcsomAssetManagement.Client.Models;
using ArcsomAssetManagement.Client.PageModels.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ArcsomAssetManagement.Client.PageModels;

public partial class ProductListPageModel : ObservableObject
{
    private readonly ProductRepository _productRepository;

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

    public ProductListPageModel(ProductRepository productRepository)
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
        SearchText = "";
        await LoadProducts(Pagination);
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
            var pagination = new PaginationModel
            {
                CurrentPage = 1,
                PageSize = Pagination.PageSize,
                TotalItems = 0
            };
            await LoadProducts(pagination, SearchText);
        }
    }
    [RelayCommand]
    private async Task GoToPageAsync(string pageNumber)
    {
        var newPageNumber = Pagination.CurrentPage;

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
        await LoadProducts(Pagination, searchText);
    }
    private async Task LoadProducts(PaginationModel pagination, string searchText = "")
    {
        (Products, Pagination) = await _productRepository.ListAsync(pageNumber: pagination.CurrentPage, pageSize: pagination.PageSize, filter: searchText);
        FilteredProducts = Products;
        PageNumbers = PaginationHelper.SetPagenumbers(Pagination.CurrentPage, Pagination.TotalPages); 
    }
}
