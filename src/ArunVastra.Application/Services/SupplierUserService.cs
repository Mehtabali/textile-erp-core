using ArunVastra.Application.DTOs.Users.Supplier;
using ArunVastra.Application.Interfaces;

namespace ArunVastra.Application.Services;

public sealed class SupplierUserService : ISupplierUserService
{
    private readonly ISupplierUserRepository _supplierUserRepository;

    public SupplierUserService(ISupplierUserRepository supplierUserRepository)
    {
        _supplierUserRepository = supplierUserRepository;
    }

    public Task<SupplierUserListResponse> ListAsync(
        SupplierUserListRequest request,
        CancellationToken cancellationToken = default)
    {
        request.PageNumber = Math.Max(request.PageNumber, 1);
        request.PageSize = Math.Clamp(request.PageSize, 1, 1000);
        request.Filters ??= new SupplierUserListFiltersRequest();

        return _supplierUserRepository.ListAsync(request, cancellationToken);
    }

    public Task<SupplierUserAutocompleteResponse> GetAutocompleteValuesAsync(
        SupplierUserAutocompleteRequest request,
        CancellationToken cancellationToken = default)
    {
        request.Field = request.Field?.Trim().ToLowerInvariant();
        request.SearchKeyword = string.IsNullOrWhiteSpace(request.SearchKeyword) ? null : request.SearchKeyword.Trim();
        request.MaxResults = Math.Clamp(request.MaxResults, 1, 50);

        return _supplierUserRepository.GetAutocompleteValuesAsync(request, cancellationToken);
    }
}
