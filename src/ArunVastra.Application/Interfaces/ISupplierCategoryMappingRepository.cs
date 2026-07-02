using ArunVastra.Application.DTOs.SupplierCategoryMappings;

namespace ArunVastra.Application.Interfaces;

public interface ISupplierCategoryMappingRepository
{
    Task<SupplierCategoryMappingListResponse> ListAsync(
        SupplierCategoryMappingListRequest request,
        CancellationToken cancellationToken = default);

    Task<SupplierCategoryMappingResponse?> GetBySupplierUserIdAsync(
        int supplierUserId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SupplierCategoryMappingOptionResponse>> SearchSuppliersAsync(
        string? searchKeyword,
        int take,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SupplierCategoryMappingOptionResponse>> SearchProductCategoriesAsync(
        string? searchKeyword,
        int take,
        CancellationToken cancellationToken = default);

    Task<bool> SupplierExistsAsync(
        int supplierUserId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlySet<int>> GetValidProductCategoryIdsAsync(
        IReadOnlyCollection<int> productCategoryIds,
        CancellationToken cancellationToken = default);

    Task<SupplierCategoryMappingResponse> SaveAsync(
        SaveSupplierCategoryMappingRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> RemoveAsync(
        int supplierUserId,
        int productCategoryId,
        CancellationToken cancellationToken = default);
}
