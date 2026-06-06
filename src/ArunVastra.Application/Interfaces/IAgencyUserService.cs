using ArunVastra.Application.DTOs.Users.Agency;

namespace ArunVastra.Application.Interfaces;

public interface IAgencyUserService
{
    Task<AgencyUserListResponse> ListAsync(
        AgencyUserListRequest request,
        CancellationToken cancellationToken = default);

    Task<AgencyUserResponse?> GetByIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<AgencyUserResponse> CreateAsync(
        CreateAgencyUserRequest request,
        CancellationToken cancellationToken = default);

    Task<AgencyUserResponse?> UpdateAsync(
        int userId,
        UpdateAgencyUserRequest request,
        CancellationToken cancellationToken = default);
}
