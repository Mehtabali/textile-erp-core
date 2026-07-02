using ArunVastra.Application.DTOs.Products;

namespace ArunVastra.Application.Interfaces;

public interface IProductListingRepository
{
    Task<ProductListResponse> ListAsync(
        ProductListRequest request,
        CancellationToken cancellationToken = default);

    Task<ProductAutocompleteResponse> GetAutocompleteValuesAsync(
        ProductAutocompleteRequest request,
        CancellationToken cancellationToken = default);
}
