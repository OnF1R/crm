using System.Text.Json;
using Microsoft.JSInterop;

namespace Crm.Web.Auth;

public class AuthState
{
    private const string StorageKey = "crm_auth";
    private readonly IJSInProcessRuntime _js;

    public string? AccessToken { get; set; }
    public string? TokenType { get; set; }
    public int ExpiresIn { get; set; }
    public string? RefreshToken { get; set; }
    public UserInfo? User { get; set; }
    public bool IsAuthenticated => !string.IsNullOrEmpty(AccessToken) && User is not null;

    public event Action? OnChange;

    public AuthState(IJSRuntime jsRuntime)
    {
        _js = jsRuntime as IJSInProcessRuntime
            ?? throw new InvalidOperationException("Требуется IJSInProcessRuntime");

        var json = _js.Invoke<string?>("localStorage.getItem", StorageKey);
        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                var saved = JsonSerializer.Deserialize<SavedState>(json);
                if (saved != null)
                {
                    AccessToken = saved.AccessToken;
                    TokenType = saved.TokenType;
                    ExpiresIn = saved.ExpiresIn;
                    User = saved.User;
                    RefreshToken = saved.RefreshToken;
                }
            }
            catch { }
        }
    }

    public void SetAuth(string accessToken, string tokenType, int expiresIn, UserInfo user, string? refreshToken = null)
    {
        AccessToken = accessToken;
        TokenType = tokenType;
        ExpiresIn = expiresIn;
        RefreshToken = refreshToken;
        User = user;
        Save();
        OnChange?.Invoke();
    }

    public void Clear()
    {
        AccessToken = null;
        TokenType = null;
        ExpiresIn = 0;
        RefreshToken = null;
        User = null;
        try { _js.InvokeVoid("localStorage.removeItem", StorageKey); } catch { }
        OnChange?.Invoke();
    }

    private void Save()
    {
        try
        {
            var json = JsonSerializer.Serialize(new SavedState(AccessToken, TokenType, ExpiresIn, User, RefreshToken));
            _js.InvokeVoid("localStorage.setItem", StorageKey, json);
        }
        catch { }
    }

    public record SavedState(string? AccessToken, string? TokenType, int ExpiresIn, UserInfo? User, string? RefreshToken = null);
}

public class UserInfo
{
    public Guid Id { get; set; }
    public string Email { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Role { get; set; } = "";
    public string Status { get; set; } = "";
}
