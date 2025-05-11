using ArcsomAssetManagement.Client.Models;
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
    private ObservableCollection<Manufacturer> filteredManufacturers;
    public ObservableCollection<Manufacturer> AllManufacturers { get; set; }

    [ObservableProperty]
    private ObservableCollection<Manufacturer> _manufacturers = [];

    public ManufacturerListPageModel(ManufacturerRepository manufacturerRepository)
    {
        _manufacturerRepository = manufacturerRepository;
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
            filteredManufacturers = await _manufacturerRepository.ListAsync();
        }
        else
        {
            var filtered = Manufacturers
                .Where(p => p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                            (p.Contact?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
            FilteredManufacturers = new ObservableCollection<Manufacturer>(filtered);
        }
    }
}
