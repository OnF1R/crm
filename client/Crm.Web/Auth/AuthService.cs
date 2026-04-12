using System.Net.Http.Json;

namespace Crm.Web.Auth;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly AuthState _authState;

    public AuthService(HttpClient http, AuthState authState)
    {
        _http = http;
        _authState = authState;
    }

    public AuthState State => _authState;
    public bool IsAuthenticated => _authState.IsAuthenticated;

    public async Task<bool> LoginAsync(string email, string password)
    {
        var response = await _http.PostAsJsonAsync("/api/identity/auth/login", new { Email = email, Password = password });
        if (!response.IsSuccessStatusCode) return false;

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        if (result?.Data == null) return false;

        _authState.SetAuth(
            result.Data.AccessToken,
            result.Data.TokenType,
            result.Data.ExpiresIn,
            new UserInfo
            {
                Id = result.Data.User.Id,
                Email = result.Data.User.Email,
                FirstName = result.Data.User.FirstName,
                LastName = result.Data.User.LastName,
                FullName = result.Data.User.FullName,
                Role = result.Data.User.RoleName,
                Status = result.Data.User.Status
            });

        return true;
    }

    public async Task<bool> RegisterAsync(string email, string password, string firstName, string lastName, int role = 3)
    {
        var response = await _http.PostAsJsonAsync("/api/identity/auth/register", new
        {
            Email = email, Password = password, FirstName = firstName, LastName = lastName, Role = role
        });
        return response.IsSuccessStatusCode;
    }

    public void Logout() => _authState.Clear();

    private record LoginResponse(TokenData Data);
    private record TokenData(string AccessToken, string TokenType, int ExpiresIn, UserData User);
    private record UserData(Guid Id, string Email, string FirstName, string LastName, string FullName, int Role, string RoleName, string Status);
}
