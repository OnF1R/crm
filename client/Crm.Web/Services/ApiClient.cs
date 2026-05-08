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

    public async Task<bool> PostActionAsync(string url, object data, CancellationToken ct = default)
    {
        var request = CreateRequest(HttpMethod.Post, url);
        request.Content = JsonContent.Create(data);
        var response = await _http.SendAsync(request, ct);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> PutActionAsync(string url, object data, CancellationToken ct = default)
    {
        var request = CreateRequest(HttpMethod.Put, url);
        request.Content = JsonContent.Create(data);
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

    // Tag methods
    public async Task<IReadOnlyList<TagDto>?> GetAllTagsAsync(CancellationToken ct = default)
    {
        return await GetAsync<IReadOnlyList<TagDto>>("/api/clients/tags", ct);
    }

    public async Task<TagDto?> GetTagByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await GetAsync<TagDto>($"/api/clients/tags/{id}", ct);
    }

    public async Task<TagDto?> CreateTagAsync(CreateTagDto dto, CancellationToken ct = default)
    {
        return await PostAsync<TagDto>("/api/clients/tags", dto, ct);
    }

    public async Task<TagDto?> UpdateTagAsync(Guid id, UpdateTagDto dto, CancellationToken ct = default)
    {
        return await PutAsync<TagDto>($"/api/clients/tags/{id}", dto, ct);
    }

    public async Task<bool> DeleteTagAsync(Guid id, CancellationToken ct = default)
    {
        return await DeleteAsync($"/api/clients/tags/{id}", ct);
    }

    public async Task<bool> AddTagToClientAsync(Guid clientId, Guid tagId, CancellationToken ct = default)
    {
        return await PostActionAsync($"/api/clients/clients/{clientId}/tags/{tagId}", new { }, ct);
    }

    public async Task<bool> RemoveTagFromClientAsync(Guid clientId, Guid tagId, CancellationToken ct = default)
    {
        return await DeleteAsync($"/api/clients/clients/{clientId}/tags/{tagId}", ct);
    }

    // Refusal reason methods
    public async Task<IReadOnlyList<RefusalReasonDto>?> GetAllRefusalReasonsAsync(CancellationToken ct = default)
    {
        return await GetAsync<IReadOnlyList<RefusalReasonDto>>("/api/deals/refusal-reasons", ct);
    }

    public async Task<RefusalReasonDto?> GetRefusalReasonByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await GetAsync<RefusalReasonDto>($"/api/deals/refusal-reasons/{id}", ct);
    }

    public async Task<RefusalReasonDto?> CreateRefusalReasonAsync(CreateRefusalReasonDto dto, CancellationToken ct = default)
    {
        return await PostAsync<RefusalReasonDto>("/api/deals/refusal-reasons", dto, ct);
    }

    public async Task<RefusalReasonDto?> UpdateRefusalReasonAsync(Guid id, UpdateRefusalReasonDto dto, CancellationToken ct = default)
    {
        return await PutAsync<RefusalReasonDto>($"/api/deals/refusal-reasons/{id}", dto, ct);
    }

    public async Task<bool> DeleteRefusalReasonAsync(Guid id, CancellationToken ct = default)
    {
        return await DeleteAsync($"/api/deals/refusal-reasons/{id}", ct);
    }

    public async Task<bool> SetDealRefusalReasonAsync(Guid dealId, Guid? refusalReasonId, CancellationToken ct = default)
    {
        return await PutActionAsync($"/api/deals/deals/{dealId}/refusal-reason", new { RefusalReasonId = refusalReasonId }, ct);
    }

    private record ApiResponse<T>(T? Data, bool Success, string? Message);
}

public record TagDto(Guid Id, string Name, string? Description, string? Color, DateTime CreatedAt, DateTime? UpdatedAt);
public record CreateTagDto(string Name, string? Description = null, string? Color = null);
public record UpdateTagDto(string Name, string? Description = null, string? Color = null);

public record RefusalReasonDto(Guid Id, string Name, string? Description, bool IsActive, DateTime CreatedAt, DateTime? UpdatedAt);
public record CreateRefusalReasonDto(string Name, string? Description = null);
public record UpdateRefusalReasonDto(string Name, string? Description = null, bool? IsActive = null);
