using ArcsomAssetManagement.Client.DTOs.Business;
using ArcsomAssetManagement.Client.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ArcsomAssetManagement.Client.PageModels;

public partial class ManufacturerListPageModel : ObservableObject
{
    private readonly ManufacturerRepository _manufacturerRepository;
    //private readonly SyncService<Manufacturer,ManufacturerDto> _syncService;

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
    private int selectedPage;

    public ManufacturerListPageModel(ManufacturerRepository manufacturerRepository)//, SyncService<Manufacturer, ManufacturerDto> syncService)
    {
        _manufacturerRepository = manufacturerRepository;
        //_syncService = syncService;
        pagination = new PaginationModel
        {
            CurrentPage = 1,
            PageSize = 10,
            TotalItems = 10
        };

    }

    [RelayCommand]
    private async Task Appearing()
    {
        (Manufacturers, pagination) = await _manufacturerRepository.ListAsync(pageNumber: 1, pageSize: 3);
        FilteredManufacturers = Manufacturers;
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
            (List<Manufacturer> filtered, pagination) = await _manufacturerRepository.ListAsync(pageNumber: 1, pageSize: 3, filter: SearchText);

            FilteredManufacturers = filtered;
        }
    }
    [RelayCommand]
    private async Task GoToPageAsync(int pageNumber)
    {
        var page = (int)pageNumber;
        Appearing();
    }

    [RelayCommand]
    private async Task SyncManufacturers()
    {
        //await _syncService.ProcessSyncQueueAsync();
        await Appearing();
    }
}
