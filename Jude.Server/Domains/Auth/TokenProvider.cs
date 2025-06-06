using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Jude.Server.Config;
using Microsoft.IdentityModel.Tokens;

namespace Jude.Server.Domains.Auth;

public interface ITokenProvider
{
    string GenerateToken(string userId, string roleId);
}

public class TokenProvider : ITokenProvider
{
    public string GenerateToken(string userId, string roleId)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, roleId.ToString()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AppConfig.JwtConfig.Secret));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: AppConfig.JwtConfig.Issuer,
            audience: AppConfig.JwtConfig.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(14),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
