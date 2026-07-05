using ArunVastra.Application.DTOs.Users.Supplier;

namespace ArunVastra.Application.Interfaces;

public interface ISupplierUserService
{
    Task<SupplierUserListResponse> ListAsync(
        SupplierUserListRequest request,
        CancellationToken cancellationToken = default);

    Task<SupplierUserAutocompleteResponse> GetAutocompleteValuesAsync(
        SupplierUserAutocompleteRequest request,
        CancellationToken cancellationToken = default);

    Task<SupplierUserResponse?> GetByIdAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<string> GetNextUserCodeAsync(
        CancellationToken cancellationToken = default);

    Task<SupplierUserResponse> CreateAsync(
        CreateSupplierUserRequest request,
        CancellationToken cancellationToken = default);

    Task<SupplierUserResponse?> UpdateAsync(
        int userId,
        UpdateSupplierUserRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task LockAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task ResetPasswordAsync(
        int userId,
        ResetSupplierPasswordRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SupplierOptionResponse>> GetAgencyOptionsAsync(
        string? searchKeyword,
        int take,
        CancellationToken cancellationToken = default);
}
