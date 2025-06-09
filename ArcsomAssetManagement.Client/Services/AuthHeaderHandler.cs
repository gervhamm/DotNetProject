using System.Diagnostics;

namespace ArcsomAssetManagement.Client.Services;

public class AuthHeaderHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!request.RequestUri.AbsolutePath.StartsWith("/auth", StringComparison.OrdinalIgnoreCase) &&
            !request.RequestUri.AbsolutePath.StartsWith("/ping", StringComparison.OrdinalIgnoreCase))
        {
            var token = await SecureStorage.GetAsync("auth_token");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }
        try
        {
            HttpResponseMessage responseMessage = await base.SendAsync(request, cancellationToken);
            return responseMessage;

        }
        catch (Exception e)
        {
            throw new Exception($"Error sending request to {request.RequestUri}: {e.Message}", e);
        }
    }
}
