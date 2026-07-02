using ArunVastra.Application.DTOs.SupplierTransportMappings;

namespace ArunVastra.Application.Interfaces;

public interface ISupplierTransportMappingRepository
{
    Task<SupplierTransportMappingListResponse> ListAsync(
        SupplierTransportMappingListRequest request,
        CancellationToken cancellationToken = default);

    Task<SupplierTransportMappingResponse?> GetBySupplierUserIdAsync(
        int supplierUserId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SupplierTransportMappingOptionResponse>> SearchSuppliersAsync(
        string? searchKeyword,
        int take,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SupplierTransportMappingOptionResponse>> SearchTransportsAsync(
        string? searchKeyword,
        int take,
        CancellationToken cancellationToken = default);

    Task<bool> SupplierExistsAsync(
        int supplierUserId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlySet<int>> GetValidTransportIdsAsync(
        IReadOnlyCollection<int> transportIds,
        CancellationToken cancellationToken = default);

    Task<SupplierTransportMappingResponse> SaveAsync(
        SaveSupplierTransportMappingRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> RemoveAsync(
        int supplierUserId,
        int transportUserId,
        CancellationToken cancellationToken = default);
}
