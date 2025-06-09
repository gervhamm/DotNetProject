namespace ArcsomAssetManagement.Client.Services;

public static class TokenStorage
{
    private const string TokenKey = "auth_token";

    public static async Task SaveTokenAsync(string token)
    {
        await SecureStorage.SetAsync(TokenKey, token);
    }

    public static async Task<string> GetTokenAsync()
    {
        return await SecureStorage.GetAsync(TokenKey) ?? string.Empty;
    }

    public static void RemoveToken()
    {
        SecureStorage.Remove(TokenKey);
    }

    public static async Task SaveTokenUserAsync(string tokenKey, string token)
    {
        await SecureStorage.SetAsync(tokenKey, token);
    }

    public static async Task<string> GetTokenUserAsync(string tokenKey)
    {
        return await SecureStorage.GetAsync(tokenKey) ?? string.Empty;
    }

    public static void RemoveUserToken(string tokenKey)
    {
        SecureStorage.Remove(tokenKey);
    }
}