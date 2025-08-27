using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ChallengeBet.Application.Abstractions;
using ChallengeBet.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ChallengeBet.Infrastructure.Services;

public class JwtTokenService(IConfiguration config) : ITokenService
{
    public string GenerateToken(Player player)
    { 
        var issuer = config["Jwt:Issuer"] ?? "ChallengeBet";
        var key = config["Jwt:Key"] ?? "dev-key-change-me";
        var expiresMinutes = int.TryParse(config["Jwt:ExpiresMinutes"], out var m) ? m : 120;
        
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, player.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, player.Email),
            new Claim("name", player.Name)
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: null,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}