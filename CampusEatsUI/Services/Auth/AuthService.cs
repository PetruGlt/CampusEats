using System.Net.Http.Json;
using System.Text.Json;
using CampusEatsUI.Models.Auth;
using CampusEatsUI.Models.Helpers;
using Microsoft.JSInterop;

namespace CampusEatsUI.Services.Auth;

public class AuthService(HttpClient _http, IJSRuntime _jsRuntime) : IAuthService
{
    private const string BaseUri = "http://localhost:5168/auth";
    
    public async Task<AuthResponse> LoginAsync(string email, string plainPassword)
    {
        var request = new LoginRequest(email, plainPassword);
        var response = await _http.PostAsJsonAsync($"{BaseUri}/login", request);
        return await response.Content.ReadFromJsonAsync<AuthResponse>() ?? new AuthResponse(string.Empty);
    }

    public async Task RegisterAsync(string username, string email, string plainPassword)
    {
        var request = new RegisterRequest(username, email, plainPassword);
        await _http.PostAsJsonAsync($"{BaseUri}/register", request);
    }

    public async Task LogoutAsync()
    {
        await _jsRuntime.InvokeVoidAsync("eval", "document.cookie = 'authToken=; max-age=0; path=/; secure; samesite=strict'");
    }

    public string GetTokenAsync(string cookie)
    {
        var cookies = cookie.Split(";");
        var authCookie = cookies.FirstOrDefault(c => c.Trim().StartsWith("authToken="));
        if (!string.IsNullOrEmpty(authCookie))
        {
            return authCookie.Split("=")[1];
        }
        else
        {
            return string.Empty;
        }
    }

    public UserSession ParseJwt(string token)
    {
        try
        {
            var payload = token.Split('.')[1];
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }

            var jsonBytes = Convert.FromBase64String(payload);
            var claims = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes);

            if (claims == null) return null;

            var user = new UserSession();

            if (claims.TryGetValue("Id", out var idVal) || claims.TryGetValue("id", out idVal) || claims.TryGetValue("sub", out idVal))
                if (Guid.TryParse(idVal.ToString(), out var id))
                    user.Id = id;

            if (claims.TryGetValue("Username", out var nameVal) || claims.TryGetValue("username", out nameVal) || claims.TryGetValue("unique_name", out nameVal))
                user.Username = nameVal.ToString();

            if (claims.TryGetValue("Email", out var emailVal) || claims.TryGetValue("email", out emailVal))
                user.Email = emailVal.ToString();

            return user;
        }
        catch
        {
            return null;
        }
    }
}