using ArunVastra.Application.DTOs.Users.Supplier;

namespace ArunVastra.Application.Interfaces;

public interface ISupplierUserRepository
{
    Task<SupplierUserListResponse> ListAsync(
        SupplierUserListRequest request,
        CancellationToken cancellationToken = default);

    Task<SupplierUserAutocompleteResponse> GetAutocompleteValuesAsync(
        SupplierUserAutocompleteRequest request,
        CancellationToken cancellationToken = default);
}
