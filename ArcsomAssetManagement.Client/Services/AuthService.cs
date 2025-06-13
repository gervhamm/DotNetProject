namespace ArcsomAssetManagement.Client.Services;

public class AuthService
{
    public async Task<bool> IsLoggedInAsync()
    {
        var token = await TokenStorage.GetTokenAsync();
        var tokenExpiry = await TokenStorage.GetTokenExpriyAsync();

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(tokenExpiry))
        {
            await Shell.Current.GoToAsync("//main");
            return false;
        }

        if (!DateTime.TryParse(tokenExpiry, out var expiry))
        {
            await Shell.Current.GoToAsync("//main");
            return false;
        }

        if (expiry < DateTime.UtcNow)
        {
            await AppShell.DisplaySnackbarAsync("User credentials have expired!");
            await LogoutAsync();
            return false;
        }

            return expiry > DateTime.UtcNow;
    }

    public async Task LogoutAsync()
    {
        TokenStorage.RemoveToken();

        await Shell.Current.GoToAsync("//main");
    }
}
