using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

namespace CampusEats.Services.Auth;

public interface ITokenService
{
    string GenerateToken(string email, string username, Guid userId);
}

public class TokenService(IConfiguration configuration) : Service.Auth.ITokenService
{
    public string GenerateToken(string email, string username, Guid userId)
    {
        var secretKey = configuration["JwtSettings:SecretKey"] ?? "super_secret_key_must_be_at_least_32_chars";
        var issuer = configuration["JwtSettings:Issuer"] ?? "CampusEats";
        var audience = configuration["JwtSettings:Audience"] ?? "CampusEatsUI";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("username", username)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}