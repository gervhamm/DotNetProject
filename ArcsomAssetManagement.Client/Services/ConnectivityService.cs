using System.ComponentModel;

namespace ArcsomAssetManagement.Client.Services;

public class ConnectivityService : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private bool _isOnline = true;

    public bool IsOnline
    {
        get => _isOnline;

        set
        {
            if (_isOnline != value)
            {
                _isOnline = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsOnline)));
            }
        }
    }

    private bool _isOfflineMode = false;
    public bool IsOfflineMode
    {
        get => _isOfflineMode;
        set
        {
            _isOfflineMode = value;

            var access = _isOfflineMode ? NetworkAccess.None : Connectivity.NetworkAccess;
            UpdateConnectivity(access);            
        }
    }
    public ConnectivityService()
    {
        UpdateConnectivity(Connectivity.NetworkAccess);
        Connectivity.ConnectivityChanged += OnConnectivityChanged;
    }

    private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        var access = IsOfflineMode ? NetworkAccess.None : e.NetworkAccess;
        UpdateConnectivity(access);
    }

    private void UpdateConnectivity(NetworkAccess access)
    {
        IsOnline = access == NetworkAccess.Internet;
    }
}
