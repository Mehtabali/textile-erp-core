using ArunVastra.Application.Interfaces;
using ArunVastra.Application.Models;
using ArunVastra.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ArunVastra.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly ArunVastraDbContext _dbContext;

    public UserRepository(ArunVastraDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserAuthModel?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Where(user => user.Email == email)
            .Select(user => new UserAuthModel
            {
                UserId = user.Userid.ToString(),
                Email = user.Email,
                Role = user.Role.ToString(),
                LegacyPassword = user.Pwhash,
                PasswordHash = user.Passwordhash,
                PasswordMigrated = user.Passwordmigrated,
                PasswordResetRequired = user.Passwordresetrequired,
                Locked = user.Locked
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<UserAuthModel?> GetByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var parsedUserId = ParseUserId(userId);

        return await _dbContext.Users
            .AsNoTracking()
            .Where(user => user.Userid == parsedUserId)
            .Select(user => new UserAuthModel
            {
                UserId = user.Userid.ToString(),
                Email = user.Email,
                Role = user.Role.ToString(),
                LegacyPassword = user.Pwhash,
                PasswordHash = user.Passwordhash,
                PasswordMigrated = user.Passwordmigrated,
                PasswordResetRequired = user.Passwordresetrequired,
                Locked = user.Locked
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task UpdatePasswordMigrationAsync(
        string userId,
        string passwordHash,
        CancellationToken cancellationToken = default)
    {
        var parsedUserId = ParseUserId(userId);

        await _dbContext.Users
            .Where(user => user.Userid == parsedUserId)
            .ExecuteUpdateAsync(
                updates => updates
                    .SetProperty(user => user.Passwordhash, passwordHash)
                    .SetProperty(user => user.Passwordmigrated, true)
                    .SetProperty(user => user.Updatedat, DateTime.UtcNow),
                cancellationToken);
    }

    public async Task UpdateLastLoginAsync(string userId, CancellationToken cancellationToken = default)
    {
        var parsedUserId = ParseUserId(userId);
        var now = DateTime.UtcNow;

        await _dbContext.Users
            .Where(user => user.Userid == parsedUserId)
            .ExecuteUpdateAsync(
                updates => updates
                    .SetProperty(user => user.Lastloginat, now)
                    .SetProperty(user => user.Updatedat, now),
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
