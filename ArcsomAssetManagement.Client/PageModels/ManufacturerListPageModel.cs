using ArcsomAssetManagement.Client.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ArcsomAssetManagement.Client.PageModels;

public partial class ManufacturerListPageModel : ObservableObject
{
    private readonly IRepository<Manufacturer> _manufacturerRepository;
    private readonly SyncService<Manufacturer> _syncService;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private List<Manufacturer> filteredManufacturers;
    public ObservableCollection<Manufacturer> AllManufacturers { get; set; }

    [ObservableProperty]
    private List<Manufacturer> _manufacturers = [];

    public ManufacturerListPageModel(IRepository<Manufacturer> manufacturerRepository, SyncService<Manufacturer> syncService)
    {
        _manufacturerRepository = manufacturerRepository;
        _syncService = syncService;
    }

    [RelayCommand]
    private async Task Appearing()
    {
        Manufacturers = await _manufacturerRepository.ListAsync();
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
            var filtered = Manufacturers
                .Where(p => p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                            (p.Contact?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
            FilteredManufacturers = new List<Manufacturer>(filtered);
        }
    }
    [RelayCommand]
    private async Task SyncManufacturers()
    {
        await _syncService.ProcessSyncQueueAsync();
        await Appearing();
    }
}
