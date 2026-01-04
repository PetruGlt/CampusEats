using CampusEatsUI.Models.Auth;
using CampusEatsUI.Models.Helpers;

namespace CampusEatsUI.Services.Auth;

public interface IAuthenticationService
{
    Task<AuthResponse> LoginAsync(string email, string plainPassword);
    Task RegisterAsync(string username, string email, string plainPassword);
    Task LogoutAsync();
    string GetTokenAsync(string cookie);
    UserSession ParseJwt(string token);
}