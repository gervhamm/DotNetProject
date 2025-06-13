using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ArcsomAssetManagement.Client.PageModels;

public partial class MainPageModel : BasePageModel
{
    private readonly AuthRepository _authRepository;
    private readonly ModalErrorHandler _errorHandler;
    public MainPageModel(ModalErrorHandler errorHandler, AuthRepository authRepository, AuthService authService) : base(authService)
    {
        _authRepository = authRepository;
        _errorHandler = errorHandler;
    }

    [ObservableProperty]
    private string _username;

    [ObservableProperty]
    private string _password;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _displayLoggedIn;

    [ObservableProperty]
    private string _displayCurrentUser;

    [RelayCommand]
    public async Task TestAsync()
    {
        await Shell.Current.GoToAsync("//main");
        if (!OperatingSystem.IsWindows())
        {
            await AppShell.DisplayToastAsync("Toast test successfull!");
            return;
        }
        await AppShell.DisplaySnackbarAsync("Snackbar test successful!!");
    }

    [RelayCommand]
    public async Task LoginAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                await AppShell.Current.DisplayAlert("Notification", "Please enter both username and password.", "OK");
                return;
            }
            try
            {
                var response = await _authRepository.LoginAsync(Username, Password);
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex);
            }
            finally
            {
                await CheckAuthAsync();
                DisplayLoggedIn = IsLoggedIn;
            }

            DisplayCurrentUser = CurrentUser ?? string.Empty;
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task RegisterAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                await AppShell.Current.DisplayAlert("Notification", "Please enter both username and password.", "OK");
                return;
            }

            var response = await _authRepository.RegisterAsync(Username, Password);
            await AppShell.DisplaySnackbarAsync("Registration successful! You can now log in.");            
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task LogoutAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            await _authService.LogoutAsync();
            DisplayLoggedIn = false;
            DisplayCurrentUser = string.Empty;
            await AppShell.DisplaySnackbarAsync("You have been logged out.");
        }
        catch (Exception ex)
        {
            _errorHandler.HandleError(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private class TokenResponse
    {
        public string Token { get; set; }
    }
}