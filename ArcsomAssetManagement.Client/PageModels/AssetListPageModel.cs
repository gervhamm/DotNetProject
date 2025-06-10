using ArcsomAssetManagement.Client.Models;
using ArcsomAssetManagement.Client.PageModels.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ArcsomAssetManagement.Client.PageModels;

public partial class AssetListPageModel : BasePageModel
{
    private readonly AssetRepository _assetRepository;
    private readonly AuthService _authService;

    private List<Product> _products = [];

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private List<Asset> filteredAssets;

    [ObservableProperty]
    private List<Asset> _assets = [];

    [ObservableProperty]
    private PaginationModel _pagination;

    [ObservableProperty]
    private List<PageNumberItem> _pageNumbers = [];

    [ObservableProperty]
    private int selectedPage;

    public AssetListPageModel(AssetRepository assetRepository, AuthService authService) : base (authService)
    {
        _assetRepository = assetRepository;
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
        (Assets, _pagination) = await _assetRepository.ListAsync(pageNumber: _pagination.CurrentPage, pageSize: _pagination.PageSize);
        FilteredAssets = Assets;
        PageNumbers = PaginationHelper.SetPagenumbers(_pagination.CurrentPage, _pagination.TotalPages);
    }

    [RelayCommand]
    Task NavigateToAsset(Asset asset)
        => Shell.Current.GoToAsync($"asset?id={asset.Id}");

    [RelayCommand]
    async Task AddAsset()
    {
        await Shell.Current.GoToAsync($"asset");
    }
    [RelayCommand]
    private async Task FilterAssets()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            await Appearing();
        }
        else
        {
            (List<Asset> Assets, PaginationModel pagination) = await _assetRepository.ListAsync(pageNumber: _pagination.CurrentPage, pageSize: _pagination.PageSize, filter: SearchText);
            var filtered = Assets
                .Where(p => p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                            (p.Product?.Name?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
            FilteredAssets = filtered;
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
