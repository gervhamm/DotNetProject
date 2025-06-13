using ArcsomAssetManagement.Client.Models;
using ArcsomAssetManagement.Client.PageModels.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ArcsomAssetManagement.Client.PageModels;

public partial class AssetListPageModel : ObservableObject
{
    private readonly AssetRepository _assetRepository;

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

    public AssetListPageModel(AssetRepository assetRepository)
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
        SearchText = "";
        await LoadAssets(Pagination);
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
            var pagination = new PaginationModel
            {
                CurrentPage = 1,
                PageSize = Pagination.PageSize,
                TotalItems = 0
            };
            await LoadAssets(pagination, SearchText);
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
        await LoadAssets(Pagination, searchText);
    }
    private async Task LoadAssets(PaginationModel pagination, string searchText = "")
    {
        (Assets, Pagination) = await _assetRepository.ListAsync(pageNumber: pagination.CurrentPage, pageSize: pagination.PageSize, filter: searchText);
        FilteredAssets = Assets;
        PageNumbers = PaginationHelper.SetPagenumbers(Pagination.CurrentPage, Pagination.TotalPages);
    }
}
