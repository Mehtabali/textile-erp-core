using ArunVastra.Application.Models;

namespace ArunVastra.Application.Interfaces;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshTokenModel refreshToken, CancellationToken cancellationToken = default);

    Task<RefreshTokenModel?> GetActiveByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    Task RevokeAsync(long id, CancellationToken cancellationToken = default);

    Task RevokeAllForUserAsync(string userId, CancellationToken cancellationToken = default);
}
