using ArcsomAssetManagement.Client.DTOs.Business;
using ArcsomAssetManagement.Client.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ArcsomAssetManagement.Client.PageModels;
//TODO: Rename "Meta"
public partial class ManageMetaPageModel : ObservableObject
{
    private readonly SeedDataService _seedDataService;
    private readonly ConnectivityService _connectivity;
    private readonly ModalErrorHandler _errorHandler;
    private readonly SyncService<Manufacturer, ManufacturerDto> _manufacturerSyncService;

    [ObservableProperty]
    private string _isOnlineColor = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Category> _categories = [];

    [ObservableProperty]
    private ObservableCollection<Tag> _tags = [];

    public ManageMetaPageModel(ConnectivityService connectivityService, SeedDataService seedDataService, ModalErrorHandler errorHandler, SyncService<Manufacturer, ManufacturerDto> syncService)
    {
        _connectivity = connectivityService;
        _seedDataService = seedDataService;
        _errorHandler = errorHandler;
        _manufacturerSyncService = syncService;
    }


    [RelayCommand]
    private async Task Appearing()
    {
        await LoadData();
    }

   

    [RelayCommand]
    private async Task Reset()
    {
        Preferences.Default.Remove("is_seeded");
        await _seedDataService.LoadSeedDataAsync();
        Preferences.Default.Set("is_seeded", true);
        await Shell.Current.GoToAsync("//main");
    }

    [RelayCommand]
    private async Task SyncAll()
    {
        try
        {
            await _manufacturerSyncService.ProcessSyncQueueAsync();
            await _manufacturerSyncService.PullLatestRemoteChanges();
        }
        catch (Exception e)
        {
            _errorHandler.HandleError(e);
        }
    }

    [RelayCommand]
    private async Task ToggleOnline()
    {
        _connectivity.IsOnline = !_connectivity.IsOnline;
        IsOnlineColor = _connectivity.IsOnline ? "Green" : "Red";
    }

    private async Task LoadData()
    {
        try
        {
            IsOnlineColor = _connectivity.IsOnline ? "Green" : "Red";
        }
        catch (Exception e)
        {
            _errorHandler.HandleError(e);
        }
    }
}
