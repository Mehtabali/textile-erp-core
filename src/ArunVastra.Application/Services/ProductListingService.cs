using ArunVastra.Application.DTOs.Products;
using ArunVastra.Application.Interfaces;

namespace ArunVastra.Application.Services;

public sealed class ProductListingService(IProductListingRepository productListingRepository) : IProductListingService
{
    private readonly IProductListingRepository _productListingRepository = productListingRepository;

    public Task<ProductListResponse> ListAsync(
        ProductListRequest request,
        CancellationToken cancellationToken = default)
    {
        request.PageNumber = Math.Max(request.PageNumber, 1);
        request.PageSize = Math.Clamp(request.PageSize, 1, 1000);
        request.Filters ??= new ProductListFiltersRequest();

        return _productListingRepository.ListAsync(request, cancellationToken);
    }

    public Task<ProductAutocompleteResponse> GetAutocompleteValuesAsync(
        ProductAutocompleteRequest request,
        CancellationToken cancellationToken = default)
    {
        request.Field = request.Field?.Trim().ToLowerInvariant();
        request.SearchKeyword = string.IsNullOrWhiteSpace(request.SearchKeyword) ? null : request.SearchKeyword.Trim();
        request.MaxResults = Math.Clamp(request.MaxResults, 1, 50);

        return _productListingRepository.GetAutocompleteValuesAsync(request, cancellationToken);
    }
}
