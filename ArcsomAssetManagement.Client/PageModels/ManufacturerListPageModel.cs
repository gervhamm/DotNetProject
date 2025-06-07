using ArcsomAssetManagement.Client.Models;
using ArcsomAssetManagement.Client.PageModels.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ArcsomAssetManagement.Client.PageModels;

public partial class ManufacturerListPageModel : ObservableObject
{
    private readonly ManufacturerRepository _manufacturerRepository;
    // private readonly SyncService<Manufacturer,ManufacturerDto> _syncService;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private List<Manufacturer> filteredManufacturers;
    public ObservableCollection<Manufacturer> AllManufacturers { get; set; }

    [ObservableProperty]
    private List<Manufacturer> _manufacturers = [];

    [ObservableProperty]
    private PaginationModel pagination;

    [ObservableProperty]
    private List<PageNumberItem> _pageNumbers = [];

    [ObservableProperty]
    private int selectedPage;

    public ManufacturerListPageModel(ManufacturerRepository manufacturerRepository)//, SyncService<Manufacturer, ManufacturerDto> syncService)
    {
        _manufacturerRepository = manufacturerRepository;
        //_syncService = syncService;
        pagination = new PaginationModel
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
        (Manufacturers, pagination) = await _manufacturerRepository.ListAsync(pageNumber: pagination.CurrentPage, pageSize: pagination.PageSize);
        FilteredManufacturers = Manufacturers;
        PageNumbers = PaginationHelper.SetPagenumbers(pagination.CurrentPage, pagination.TotalPages);
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
            (List<Manufacturer> filtered, pagination) = await _manufacturerRepository.ListAsync(pageNumber: pagination.CurrentPage, pageSize: pagination.PageSize, filter: SearchText);

            FilteredManufacturers = filtered;
        }
    }
    [RelayCommand]
    private async Task GoToPageAsync(string pageNumber)
    {
        var newPageNumber = pagination.CurrentPage;

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

    [RelayCommand]
    private async Task SyncManufacturers()
    {
        //await _syncService.ProcessSyncQueueAsync();
        await Appearing();
    }
}
