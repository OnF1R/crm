using System.Net.Http.Headers;
using System.Net.Http.Json;
using Crm.Web.Auth;

namespace Crm.Web.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly AuthState _authState;

    public ApiClient(HttpClient http, AuthState authState)
    {
        _http = http;
        _authState = authState;
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string url)
    {
        var request = new HttpRequestMessage(method, url);
        if (_authState.IsAuthenticated)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.AccessToken);
        return request;
    }

    public async Task<T?> GetAsync<T>(string url, CancellationToken ct = default)
    {
        var request = CreateRequest(HttpMethod.Get, url);
        var response = await _http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(ct);
        return wrapper is { Success: true } ? wrapper.Data : default;
    }

    public async Task<T?> PostAsync<T>(string url, object data, CancellationToken ct = default)
    {
        var request = CreateRequest(HttpMethod.Post, url);
        request.Content = JsonContent.Create(data);
        var response = await _http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(ct);
        return wrapper is { Success: true } ? wrapper.Data : default;
    }

    public async Task<T?> PutAsync<T>(string url, object data, CancellationToken ct = default)
    {
        var request = CreateRequest(HttpMethod.Put, url);
        request.Content = JsonContent.Create(data);
        var response = await _http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(ct);
        return wrapper is { Success: true } ? wrapper.Data : default;
    }

    public async Task<bool> DeleteAsync(string url, CancellationToken ct = default)
    {
        var request = CreateRequest(HttpMethod.Delete, url);
        var response = await _http.SendAsync(request, ct);
        return response.IsSuccessStatusCode;
    }

    public async Task<T?> UploadFileAsync<T>(string url, string fileName, Stream stream, string contentType, Dictionary<string, string>? formData = null, CancellationToken ct = default)
    {
        var request = CreateRequest(HttpMethod.Post, url);
        using var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        content.Add(fileContent, "file", fileName);
        if (formData != null)
        {
            foreach (var kv in formData)
                content.Add(new StringContent(kv.Value), kv.Key);
        }
        request.Content = content;
        var response = await _http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        var wrapper = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(ct);
        return wrapper is { Success: true } ? wrapper.Data : default;
    }

    public async Task<byte[]?> DownloadFileAsync(string url, CancellationToken ct = default)
    {
        var request = CreateRequest(HttpMethod.Get, url);
        var response = await _http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadAsByteArrayAsync(ct);
    }

    public async Task<string?> GetRawAsync(string url, CancellationToken ct = default)
    {
        var request = CreateRequest(HttpMethod.Get, url);
        var response = await _http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(ct);
    }

    private record ApiResponse<T>(T? Data, bool Success, string? Message);
}
