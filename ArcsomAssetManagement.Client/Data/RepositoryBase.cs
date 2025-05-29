using Microsoft.Extensions.Logging;
using SQLite;
using System.Text.Json;

namespace ArcsomAssetManagement.Client.Data;

public abstract class RepositoryBase<T> where T :  class, new()
{
    protected readonly SQLiteAsyncConnection _database;
    protected readonly ILogger _logger;
    static HttpClient _httpClient;
    protected static string authorizationKey;
    private static string Url => Environment.GetEnvironmentVariable("API_URL") ?? string.Empty;

    protected static bool _hasBeenInitialized = false;

    protected RepositoryBase(ILogger logger)
    {
        _logger = logger;
        _database = new SQLiteAsyncConnection(Path.Combine(FileSystem.AppDataDirectory, Constants.DatabaseFilename), Constants.Flags);
        Init().ConfigureAwait(false);
    }

    private async Task Init()
    {
        if (_hasBeenInitialized)
            return;

        await _database.CreateTableAsync<T>();
        _hasBeenInitialized = true;
    }

    protected async Task<bool> IsOnline()
    {
        try
        {
            using var response = await _httpClient.GetAsync("https://localhost:7034");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking internet connection");
            return false;
        }
    }
    protected static async Task<HttpClient> GetOnlineClient()
    {
        if (_httpClient != null)
            return _httpClient;

        _httpClient = new HttpClient();

        // TODO: Add authorization header
        //if (string.IsNullOrEmpty(authorizationKey))
        //{
        //    authorizationKey = await _httpClient.GetStringAsync($"{Url}login");
        //    authorizationKey = JsonSerializer.Deserialize<string>(authorizationKey);
        //}

        //_httpClient.DefaultRequestHeaders.Add("Authorization", authorizationKey);
        //_httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

        return _httpClient;
    }
}
