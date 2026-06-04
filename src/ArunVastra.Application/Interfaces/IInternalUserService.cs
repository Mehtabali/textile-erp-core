using ArunVastra.Application.DTOs.Users.Internal;

namespace ArunVastra.Application.Interfaces;

public interface IInternalUserService
{
    Task<InternalUserListResponse> ListAsync(
        InternalUserListRequest request,
        CancellationToken cancellationToken = default);

    Task<InternalUserResponse?> GetByIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<InternalUserResponse> CreateAsync(
        CreateInternalUserRequest request,
        CancellationToken cancellationToken = default);

    Task<InternalUserResponse?> UpdateAsync(
        int userId,
        UpdateInternalUserRequest request,
        CancellationToken cancellationToken = default);
}
