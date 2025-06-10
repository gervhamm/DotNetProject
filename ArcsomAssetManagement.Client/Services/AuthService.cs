namespace ArcsomAssetManagement.Client.Services;

public class AuthService
{
    public async Task<bool> IsLoggedInAsync()
    {
        var token = await TokenStorage.GetTokenAsync();
        var tokenExpiry = await TokenStorage.GetTokenExpriyAsync();

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(tokenExpiry))
            return false;

        if (!DateTime.TryParse(tokenExpiry, out var expiry))
            return false;

        return expiry > DateTime.UtcNow;
    }
}
