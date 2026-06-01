using ArunVastra.Application.Interfaces;
using ArunVastra.Application.Models;
using ArunVastra.Infrastructure.Data;
using ArunVastra.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArunVastra.Infrastructure.Repositories;

public sealed class RefreshTokenRepository(ArunVastraDbContext dbContext) : IRefreshTokenRepository
{
    private readonly ArunVastraDbContext _dbContext = dbContext;

    public async Task AddAsync(RefreshTokenModel refreshToken, CancellationToken cancellationToken = default)
    {
        _dbContext.UserRefreshTokens.Add(new UserRefreshToken
        {
            Userid = ParseUserId(refreshToken.UserId),
            Tokenhash = refreshToken.TokenHash,
            Expiresat = refreshToken.ExpiresAtUtc,
            Createdat = refreshToken.CreatedAtUtc
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<RefreshTokenModel?> GetActiveByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return await _dbContext.UserRefreshTokens
            .AsNoTracking()
            .Where(token =>
                token.Tokenhash == tokenHash &&
                token.Revokedat == null &&
                token.Expiresat > now &&
                !token.User.Locked)
            .Select(token => new RefreshTokenModel
            {
                Id = token.Id,
                UserId = token.Userid.ToString(),
                TokenHash = token.Tokenhash,
                ExpiresAtUtc = token.Expiresat,
                CreatedAtUtc = token.Createdat,
                RevokedAtUtc = token.Revokedat,
                User = new UserAuthModel
                {
                    UserId = token.User.Userid.ToString(),
                    Email = token.User.Email,
                    Role = token.User.Role.ToString(),
                    LegacyPassword = token.User.Pwhash,
                    PasswordHash = token.User.Passwordhash,
                    PasswordMigrated = token.User.Passwordmigrated,
                    PasswordResetRequired = token.User.Passwordresetrequired,
                    Locked = token.User.Locked
                }
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task RevokeAsync(long id, CancellationToken cancellationToken = default)
    {
        await _dbContext.UserRefreshTokens
            .Where(token => token.Id == id && token.Revokedat == null)
            .ExecuteUpdateAsync(
                updates => updates.SetProperty(token => token.Revokedat, DateTime.UtcNow),
                cancellationToken);
    }

    public async Task RevokeAllForUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var parsedUserId = ParseUserId(userId);

        await _dbContext.UserRefreshTokens
            .Where(token =>
                token.Userid == parsedUserId &&
                token.Revokedat == null &&
                token.Expiresat > DateTime.UtcNow)
            .ExecuteUpdateAsync(
                updates => updates.SetProperty(token => token.Revokedat, DateTime.UtcNow),
                cancellationToken);
    }

    private static int ParseUserId(string userId)
    {
        if (int.TryParse(userId, out var parsedUserId))
        {
            return parsedUserId;
        }

        throw new InvalidOperationException("Invalid user id.");
    }
}
