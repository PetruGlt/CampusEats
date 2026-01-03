namespace CampusEats.Service.Auth;

public interface ITokenService
{
    public string GenerateToken(string email, string username, Guid userId);
}