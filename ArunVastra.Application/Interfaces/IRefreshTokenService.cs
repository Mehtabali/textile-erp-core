using ArunVastra.Application.Models;

namespace ArunVastra.Application.Interfaces;

public interface IRefreshTokenService
{
    RefreshTokenResult GenerateRefreshToken(string userId);

    string HashToken(string refreshToken);
}
