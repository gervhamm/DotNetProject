using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ArcsomAssetManagement.Client.Services;

public static class TokenStorage
{
    private const string TokenKey = "auth_token";
    private const string TokenExpiryKey = "auth_token_expiry";

    public static async Task SaveTokenAsync(string token)
    {
        await SecureStorage.SetAsync(TokenKey, token);

        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(token);
        var tokenExpiration = jwtSecurityToken.ValidTo;

        await SecureStorage.SetAsync(TokenExpiryKey, tokenExpiration.ToString());
    }

    public static async Task<string> GetTokenAsync()
    {
        return await SecureStorage.GetAsync(TokenKey) ?? string.Empty;
    }
    public static async Task<string> GetTokenExpriyAsync()
    {
        return await SecureStorage.GetAsync(TokenExpiryKey) ?? string.Empty;
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

    public static async Task<string> GetCurrentUsername()
    {
        var jwt = await GetTokenAsync();
        return ReadTokenClaims(jwt).TryGetValue(ClaimTypes.Name, out var name) ? name : null;
    }
        

    public static IDictionary<string, string> ReadTokenClaims(string jwt)
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwt);

        return token.Claims.ToDictionary(c => c.Type, c => c.Value);
    }
}