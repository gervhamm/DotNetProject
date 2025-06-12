using ArcsomAssetManagement.Client.DTOs.Business;
using ArcsomAssetManagement.Client.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;

namespace ArcsomAssetManagement.Client.PageModels;
//TODO: Rename "Meta"
public partial class ManageMetaPageModel : ObservableObject, INotifyPropertyChanged
{
    private readonly SeedDataService _seedDataService;
    private readonly ConnectivityService _connectivity;
    private readonly ModalErrorHandler _errorHandler;
    private readonly SyncService<Manufacturer, ManufacturerDto> _manufacturerSyncService;

    [ObservableProperty]
    private string _isOnlineColor = string.Empty;

    public ManageMetaPageModel(ConnectivityService connectivityService, SeedDataService seedDataService, ModalErrorHandler errorHandler, SyncService<Manufacturer, ManufacturerDto> syncService)
    {
        _connectivity = connectivityService;
        _seedDataService = seedDataService;
        _errorHandler = errorHandler;
        _manufacturerSyncService = syncService;

        _connectivity.PropertyChanged += Connectivity_PropertyChanged;

        UpdateOnlineColor();
    }

    [RelayCommand]
    private async Task Appearing()
    {
        UpdateOnlineColor();
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
        _connectivity.IsOfflineMode = !_connectivity.IsOfflineMode;
        UpdateOnlineColor();
    }

    private void Connectivity_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ConnectivityService.IsOnline))
        {
            UpdateOnlineColor();
        }
    }
    private void UpdateOnlineColor()
    {
        IsOnlineColor = _connectivity.IsOnline ? "Green" : "Red";
    }
}
