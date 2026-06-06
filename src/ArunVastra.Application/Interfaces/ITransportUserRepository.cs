using ArunVastra.Application.DTOs.Users.Transport;
using ArunVastra.Application.Models.Users;

namespace ArunVastra.Application.Interfaces;

public interface ITransportUserRepository
{
    Task<TransportUserListResponse> ListAsync(
        TransportUserListRequest request,
        CancellationToken cancellationToken = default);

    Task<TransportUserResponse?> GetByIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(
        string email,
        int? excludingUserId = null,
        CancellationToken cancellationToken = default);

    Task<TransportUserResponse> CreateAsync(
        TransportUserCreateModel model,
        CancellationToken cancellationToken = default);

    Task<TransportUserResponse?> UpdateAsync(
        int userId,
        TransportUserUpdateModel model,
        CancellationToken cancellationToken = default);
}
