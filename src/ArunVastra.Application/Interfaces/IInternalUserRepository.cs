using ArunVastra.Application.DTOs.Users.Internal;
using ArunVastra.Application.Models.Users;

namespace ArunVastra.Application.Interfaces;

public interface IInternalUserRepository
{
    Task<InternalUserListResponse> ListAsync(
        InternalUserListRequest request,
        CancellationToken cancellationToken = default);

    Task<InternalUserResponse?> GetByIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(
        string email,
        int? excludingUserId = null,
        CancellationToken cancellationToken = default);

    Task<InternalUserResponse> CreateAsync(
        InternalUserCreateModel model,
        CancellationToken cancellationToken = default);

    Task<InternalUserResponse?> UpdateAsync(
        int userId,
        InternalUserUpdateModel model,
        CancellationToken cancellationToken = default);
}
