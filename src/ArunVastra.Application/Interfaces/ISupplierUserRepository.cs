using ArunVastra.Application.DTOs.Users.Supplier;
using ArunVastra.Application.Models.Users;

namespace ArunVastra.Application.Interfaces;

public interface ISupplierUserRepository
{
    Task<SupplierUserListResponse> ListAsync(
        SupplierUserListRequest request,
        CancellationToken cancellationToken = default);

    Task<SupplierUserAutocompleteResponse> GetAutocompleteValuesAsync(
        SupplierUserAutocompleteRequest request,
        CancellationToken cancellationToken = default);

    Task<SupplierUserResponse?> GetByIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<string> GetNextUserCodeAsync(CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(string email, int? excludingUserId = null, CancellationToken cancellationToken = default);

    Task<SupplierUserResponse> CreateAsync(SupplierUserCreateModel model, CancellationToken cancellationToken = default);

    Task<SupplierUserResponse?> UpdateAsync(int userId, SupplierUserUpdateModel model, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int userId, CancellationToken cancellationToken = default);

    Task<bool> LockAsync(int userId, CancellationToken cancellationToken = default);

    Task<bool> ResetPasswordAsync(int userId, string legacyPassword, string passwordHash, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SupplierOptionResponse>> GetAgencyOptionsAsync(string? searchKeyword, int take, CancellationToken cancellationToken = default);
}
