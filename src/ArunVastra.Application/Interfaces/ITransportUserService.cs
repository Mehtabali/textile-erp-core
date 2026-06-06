using ArunVastra.Application.DTOs.Users.Transport;

namespace ArunVastra.Application.Interfaces;

public interface ITransportUserService
{
    Task<TransportUserListResponse> ListAsync(
        TransportUserListRequest request,
        CancellationToken cancellationToken = default);

    Task<TransportUserResponse?> GetByIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<TransportUserResponse> CreateAsync(
        CreateTransportUserRequest request,
        CancellationToken cancellationToken = default);

    Task<TransportUserResponse?> UpdateAsync(
        int userId,
        UpdateTransportUserRequest request,
        CancellationToken cancellationToken = default);
}
