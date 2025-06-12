using ArcsomAssetManagement.Client.DTOs.Auth;
using ArcsomAssetManagement.Client.DTOs.Business;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using SQLite;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Cryptography;

namespace ArcsomAssetManagement.Client.Data;

public class AuthRepository
{
    private readonly HttpClient _httpClient;
    private SQLiteAsyncConnection _database;
    private readonly ConnectivityService _connectivity;

    private readonly string _apiUrl;
    private bool _isOnline = false;

    public AuthRepository(IHttpClientFactory httpClientFactory, SQLiteAsyncConnection database, ConnectivityService connectivity)
    {
        _httpClient = httpClientFactory.CreateClient("AuthorizedClient");
        _apiUrl = "auth/login";
        _database = database;
        _connectivity = connectivity;
        _ = InitAsync();
        _connectivity = connectivity;
    }

    private async Task InitAsync()
    {
        var tables = _database.GetTableInfoAsync("CashedUser").Result;
        if (_database is not null && tables.Count() > 0)
            return;

        _database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        await _database.CreateTableAsync<CashedUser>();

    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        _isOnline = _connectivity.IsOnline;
        if (_isOnline)
        {
            var response = await _httpClient.PostAsJsonAsync(_apiUrl, new UserDto { Username = username, Password = password });
            response.EnsureSuccessStatusCode();

            var token = await response.Content.ReadAsStringAsync();
            await TokenStorage.SaveTokenAsync(token);

            await StoreHashedPasswordAndTokenOfflineAsync(username, password, token);

            return true;
        }
        else
        {
            var response = await _database.GetAsync<CashedUser>(c => c.Username == username);
            if (response is null)
            {
                throw new Exception("User not found in offline storage.");
            }
            if (!await VerifyPassword(password, response.PasswordHash, response.Salt))
            {
                throw new Exception("Invalid password.");
            }
            else
            {
                var token = await TokenStorage.GetTokenUserAsync($"token_{username}");
                await TokenStorage.SaveTokenAsync(token);

                return true;
            }
        }
    }

    private async Task<(string,string)> HashAsync(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(128 / 8); // divide by 8 to convert bits to bytes
        Console.WriteLine($"Salt: {Convert.ToBase64String(salt)}");

        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password!,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));

        string saltString = Convert.ToBase64String(salt);
        return (hashed, saltString);
    }

    private async Task<bool> VerifyPassword(string password, string passwordHash, string salt)
    {
        byte[] saltBytes = Convert.FromBase64String(salt);
        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: saltBytes,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));
        return hashed == passwordHash;
    }

    private async Task StoreHashedPasswordAndTokenOfflineAsync(string username, string password, string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(token);
        var tokenExpiration = jwtSecurityToken.ValidTo;

        await TokenStorage.SaveTokenUserAsync($"token_{username}", token);

        var (hashed, salt) = await HashAsync(password);
        var cashedUser = new CashedUser
        {
            Username = username,
            PasswordHash = hashed,
            Salt = salt,
            TokenExpiration = tokenExpiration
        };
        await _database.InsertOrReplaceAsync(cashedUser);
    }
}