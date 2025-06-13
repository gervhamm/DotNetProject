using ArcsomAssetManagement.Client.DTOs.Business;
using ArcsomAssetManagement.Client.Models;
using System.ComponentModel;

namespace ArcsomAssetManagement.Client.Services;

public class SyncManager
{
    private readonly EntitySyncService<Manufacturer, ManufacturerDto> _manufacturerSyncService;
    private readonly EntitySyncService<Product, ProductDto> _productSyncService;
    private readonly EntitySyncService<Asset, AssetDto> _assetSyncService;
    private readonly ConnectivityService _connectivity;
    private readonly ModalErrorHandler _errorHandler;

    private bool _hasSyncedAfterReconnection = false;

    public SyncManager(EntitySyncService<Manufacturer, ManufacturerDto> manufacturerSyncService,
                                EntitySyncService<Product, ProductDto> productSyncService,
                                EntitySyncService<Asset, AssetDto> assetSyncService,
                                ConnectivityService connectivity,
                                ModalErrorHandler errorHandler)
    {
        _manufacturerSyncService = manufacturerSyncService;
        _productSyncService = productSyncService;
        _assetSyncService = assetSyncService;
        _connectivity = connectivity;
        _errorHandler = errorHandler;

        _connectivity.PropertyChanged += OnConnectivityChanged;
    }

    public async Task SyncAllAsync()
    {
        try
        {
            await _manufacturerSyncService.ProcessSyncQueueAsync();
            await _manufacturerSyncService.PullLatestRemoteChanges();

            await _productSyncService.ProcessSyncQueueAsync();
            await _productSyncService.PullLatestRemoteChanges();

            await _assetSyncService.ProcessSyncQueueAsync();
            await _assetSyncService.PullLatestRemoteChanges();
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError(ex);
        }
    }
    private void OnConnectivityChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ConnectivityService.IsOnline))
        {
            if (_connectivity.IsOnline && !_hasSyncedAfterReconnection)
            {
                _hasSyncedAfterReconnection = true;
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await SyncAllAsync();
                });
            }
            else if (!_connectivity.IsOnline)
            {
                _hasSyncedAfterReconnection = false;
            }
        }
    }
}