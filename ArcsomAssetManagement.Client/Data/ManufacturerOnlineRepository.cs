using ArcsomAssetManagement.Client.DTOs.Business;
using System.Net.Http.Json;

namespace ArcsomAssetManagement.Client.Data;

public class ManufacturerOnlineRepository : IOnlineRepository<ManufacturerDto>
{
    private readonly HttpClient _httpClient;
    private readonly string _apiUrl;

    public ManufacturerOnlineRepository(HttpClient httpClient, string apiUrl)
    {
        _httpClient = httpClient;
        _apiUrl = apiUrl;
    }
    public async Task<int> DeleteItemAsync(ManufacturerDto item)
    {
        var response = await _httpClient.DeleteAsync($"{_apiUrl}/{item.Id}");
        return response.IsSuccessStatusCode ? 1 : 0;
    }

    public async Task<ManufacturerDto?> GetAsync(ulong id)
    {
        return await _httpClient.GetFromJsonAsync<ManufacturerDto>($"{_apiUrl}/{id}");
    }

    public async Task<List<ManufacturerDto>> ListAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<List<ManufacturerDto>>($"{_apiUrl}");
        return response ?? new List<ManufacturerDto>();
    }

    public async Task<bool> PingAsync()
    {
        // TODO: figure out how to not duplicate the pingAsync for every repository
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        try
        {
            var uri = new Uri(_apiUrl);
            var pingUri = uri.GetLeftPart(UriPartial.Authority) + "/ping";
            var response = await _httpClient.GetAsync(pingUri, cts.Token);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<ulong> SaveItemAsync(ManufacturerDto item)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{_apiUrl}", item);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to save item: {response.StatusCode} - {errorContent}");
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                if (ulong.TryParse(content, out var id))
                {
                    return id;
                }
                return 0;
            }

        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Network error while saving item: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to save item: {ex.Message}", ex);
        }
    }
}
