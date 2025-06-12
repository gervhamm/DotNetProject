using CommunityToolkit.Mvvm.ComponentModel;

namespace ArcsomAssetManagement.Client.PageModels;

public abstract class BasePageModel : ObservableObject
{
    protected readonly AuthService _authService;

    public bool IsLoggedIn { get; private set; }

    public string CurrentUser { get; private set; }

    protected BasePageModel(AuthService authService)
    {
        _authService = authService;
    }
    public virtual async Task CheckAuthAsync()
    {
        IsLoggedIn = await _authService.IsLoggedInAsync();

        if (!IsLoggedIn)
        {
            await AppShell.DisplaySnackbarAsync("You are not logged in. No Access");
            await Shell.Current.GoToAsync("//main");
        }
        CurrentUser = await TokenStorage.GetCurrentUsername();
    }
}
