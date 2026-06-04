using ArunVastra.Application.Models;

namespace ArunVastra.Application.Interfaces;

public interface IUserRepository
{
    Task<UserAuthModel?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<UserAuthModel?> GetByIdAsync(string userId, CancellationToken cancellationToken = default);

    Task UpdatePasswordMigrationAsync(
        string userId,
        string passwordHash,
        CancellationToken cancellationToken = default);

    Task UpdateLastLoginAsync(string userId, CancellationToken cancellationToken = default);
}
