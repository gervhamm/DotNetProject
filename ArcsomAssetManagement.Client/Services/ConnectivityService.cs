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
    //public ConnectivityService()
    //{
    //    UpdateConnectivity(Connectivity.NetworkAccess);
    //    Connectivity.ConnectivityChanged += OnConnectivityChanged;
    //}

    //private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    //{
    //    UpdateConnectivity(e.NetworkAccess);
    //}

    //private void UpdateConnectivity(NetworkAccess access)
    //{
    //    IsOnline = access == NetworkAccess.Internet;
    //}
}
