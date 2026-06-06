using ArunVastra.Application.DTOs.Users.Agency;
using ArunVastra.Application.Models.Users;

namespace ArunVastra.Application.Interfaces;

public interface IAgencyUserRepository
{
    Task<AgencyUserListResponse> ListAsync(
        AgencyUserListRequest request,
        CancellationToken cancellationToken = default);

    Task<AgencyUserResponse?> GetByIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(
        string email,
        int? excludingUserId = null,
        CancellationToken cancellationToken = default);

    Task<AgencyUserResponse> CreateAsync(
        AgencyUserCreateModel model,
        CancellationToken cancellationToken = default);

    Task<AgencyUserResponse?> UpdateAsync(
        int userId,
        AgencyUserUpdateModel model,
        CancellationToken cancellationToken = default);
}
