using ArcsomAssetManagement.Client.Models;
using ArcsomAssetManagement.Client.PageModels.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ArcsomAssetManagement.Client.PageModels;

public partial class ManufacturerListPageModel : ObservableObject
{
    private readonly ManufacturerRepository _manufacturerRepository;
 
    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private List<Manufacturer> filteredManufacturers;
    public ObservableCollection<Manufacturer> AllManufacturers { get; set; }

    [ObservableProperty]
    private List<Manufacturer> _manufacturers = [];

    [ObservableProperty]
    private PaginationModel _pagination;

    [ObservableProperty]
    private List<PageNumberItem> _pageNumbers = [];

    private bool _orderByDescending = false;

    [ObservableProperty]
    private int selectedPage;

    public ManufacturerListPageModel(ManufacturerRepository manufacturerRepository)
    {
        _manufacturerRepository = manufacturerRepository;
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
        searchText = "";
        await LoadManufacturers(Pagination);
    }

    [RelayCommand]
    Task NavigateToManufacturer(Manufacturer manufacturer)
        => Shell.Current.GoToAsync($"manufacturer?id={manufacturer.Id}");

    [RelayCommand]
    async Task AddManufacturer()
    {
        await Shell.Current.GoToAsync($"manufacturer");
    }
    [RelayCommand]
    private async Task FilterManufacturers()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            FilteredManufacturers = await _manufacturerRepository.ListAsync();
        }
        else
        {
            var pagination = new PaginationModel
            {
                CurrentPage = 1,
                PageSize = Pagination.PageSize,
                TotalItems = 0
            };
            await LoadManufacturers(pagination, SearchText);
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
        await LoadManufacturers(Pagination, searchText);
    }
    [RelayCommand]
    private async Task SortNameAsync()
    {
        _orderByDescending = !_orderByDescending;
        Appearing();
    }
    private async Task LoadManufacturers(PaginationModel pagination, string searchText = "")
    {
        (Manufacturers, Pagination) = await _manufacturerRepository.ListAsync(pageNumber: pagination.CurrentPage, pageSize: pagination.PageSize, filter: searchText);
        FilteredManufacturers = Manufacturers;
        PageNumbers = PaginationHelper.SetPagenumbers(Pagination.CurrentPage, Pagination.TotalPages);
    }
}
