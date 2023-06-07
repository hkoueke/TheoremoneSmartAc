using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartAc.Application.Abstractions.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SmartAc.Application.Constants;

namespace SmartAc.Infrastructure.Services;

public sealed class SmartAcJwtService : ISmartAcJwtService
{
    private readonly IConfiguration _configuration;

    public SmartAcJwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string tokenId, string token) GenerateJwtFor(string targetId, string role)
    {
        var tokenId = Guid.NewGuid().ToString();

        var claimsIdentity = new ClaimsIdentity(new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, targetId),
            new Claim(JwtRegisteredClaimNames.Jti, tokenId),
            new Claim(ClaimTypes.Role, JwtServiceConstants.JwtScopeApiKey),
            new Claim(ClaimTypes.Role, role)
        });

        byte[] encodedKey = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = claimsIdentity,
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            IssuedAt = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddDays(180),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(encodedKey), SecurityAlgorithms.HmacSha256)
        };

        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        SecurityToken? token = tokenHandler.CreateToken(tokenDescriptor);
        var jwt = tokenHandler.WriteToken(token);

        return (tokenId, jwt);
    }
}