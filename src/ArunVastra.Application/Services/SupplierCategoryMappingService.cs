using ArunVastra.Application.DTOs.SupplierCategoryMappings;
using ArunVastra.Application.Interfaces;

namespace ArunVastra.Application.Services;

public sealed class SupplierCategoryMappingService(
    ISupplierCategoryMappingRepository supplierCategoryMappingRepository) : ISupplierCategoryMappingService
{
    private readonly ISupplierCategoryMappingRepository _supplierCategoryMappingRepository =
        supplierCategoryMappingRepository;

    public Task<SupplierCategoryMappingListResponse> ListAsync(
        SupplierCategoryMappingListRequest request,
        CancellationToken cancellationToken = default)
    {
        request.PageNumber = Math.Max(request.PageNumber, 1);
        request.PageSize = Math.Clamp(request.PageSize, 1, 1000);
        request.Filters ??= new SupplierCategoryMappingListFiltersRequest();

        return _supplierCategoryMappingRepository.ListAsync(request, cancellationToken);
    }

    public Task<SupplierCategoryMappingResponse?> GetBySupplierUserIdAsync(
        int supplierUserId,
        CancellationToken cancellationToken = default)
    {
        ValidateId(supplierUserId, "Supplier user id");

        return _supplierCategoryMappingRepository.GetBySupplierUserIdAsync(supplierUserId, cancellationToken);
    }

    public Task<IReadOnlyList<SupplierCategoryMappingOptionResponse>> SearchSuppliersAsync(
        string? searchKeyword,
        int take,
        CancellationToken cancellationToken = default)
    {
        return _supplierCategoryMappingRepository.SearchSuppliersAsync(
            NormalizeNullable(searchKeyword, 100),
            NormalizeTake(take),
            cancellationToken);
    }

    public Task<IReadOnlyList<SupplierCategoryMappingOptionResponse>> SearchProductCategoriesAsync(
        string? searchKeyword,
        int take,
        CancellationToken cancellationToken = default)
    {
        return _supplierCategoryMappingRepository.SearchProductCategoriesAsync(
            NormalizeNullable(searchKeyword, 100),
            NormalizeTake(take),
            cancellationToken);
    }

    public async Task<SupplierCategoryMappingResponse> SaveAsync(
        SaveSupplierCategoryMappingRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateId(request.SupplierUserId, "Supplier user id");

        var productCategoryIds = request.ProductCategoryIds
            .Where(id => id > 0)
            .Distinct()
            .ToArray();

        if (productCategoryIds.Length == 0)
        {
            throw new InvalidOperationException("At least one product category is required.");
        }

        if (!await _supplierCategoryMappingRepository.SupplierExistsAsync(
                request.SupplierUserId,
                cancellationToken))
        {
            throw new InvalidOperationException("Supplier selection is invalid.");
        }

        var validProductCategoryIds = await _supplierCategoryMappingRepository.GetValidProductCategoryIdsAsync(
            productCategoryIds,
            cancellationToken);

        if (validProductCategoryIds.Count != productCategoryIds.Length)
        {
            throw new InvalidOperationException("One or more product category selections are invalid.");
        }

        return await _supplierCategoryMappingRepository.SaveAsync(
            new SaveSupplierCategoryMappingRequest
            {
                SupplierUserId = request.SupplierUserId,
                ProductCategoryIds = productCategoryIds
            },
            cancellationToken);
    }

    public async Task RemoveAsync(
        int supplierUserId,
        int productCategoryId,
        CancellationToken cancellationToken = default)
    {
        ValidateId(supplierUserId, "Supplier user id");
        ValidateId(productCategoryId, "Product category id");

        var removed = await _supplierCategoryMappingRepository.RemoveAsync(
            supplierUserId,
            productCategoryId,
            cancellationToken);

        if (!removed)
        {
            throw new InvalidOperationException("Supplier category mapping was not found.");
        }
    }

    private static void ValidateId(int id, string name)
    {
        if (id <= 0)
        {
            throw new InvalidOperationException($"{name} must be greater than zero.");
        }
    }

    private static int NormalizeTake(int take)
    {
        return Math.Clamp(take <= 0 ? 20 : take, 1, 50);
    }

    private static string? NormalizeNullable(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        return normalized.Length > maxLength ? normalized[..maxLength] : normalized;
    }
}

