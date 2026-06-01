using System.Security.Cryptography;
using ArunVastra.Application.Interfaces;
using ArunVastra.Application.Models;
using Microsoft.Extensions.Configuration;

namespace ArunVastra.Infrastructure.Security;

public sealed class RefreshTokenService : IRefreshTokenService
{
    private readonly IConfiguration _configuration;

    public RefreshTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public RefreshTokenResult GenerateRefreshToken(string userId)
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        var token = Convert.ToBase64String(randomBytes);
        var expiryDays = _configuration.GetValue("RefreshToken:ExpiryDays", 30);

        return new RefreshTokenResult
        {
            Token = token,
            TokenHash = HashToken(token),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(expiryDays)
        };
    }

    public string HashToken(string refreshToken)
    {
        var tokenBytes = System.Text.Encoding.UTF8.GetBytes(refreshToken);
        var hashBytes = SHA256.HashData(tokenBytes);

        return Convert.ToBase64String(hashBytes);
    }
}
