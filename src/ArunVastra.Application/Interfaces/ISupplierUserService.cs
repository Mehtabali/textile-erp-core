using ArunVastra.Application.DTOs.Users.Supplier;

namespace ArunVastra.Application.Interfaces;

public interface ISupplierUserService
{
    Task<SupplierUserListResponse> ListAsync(
        SupplierUserListRequest request,
        CancellationToken cancellationToken = default);
}
