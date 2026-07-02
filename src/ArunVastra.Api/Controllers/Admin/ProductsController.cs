using ArunVastra.Application.DTOs.Products;
using ArunVastra.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ArunVastra.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/products")]
[Produces("application/json")]
[SwaggerTag("Product listing endpoints. These endpoints read supplier products from dbo.SUPITEMVIEW.")]
public sealed class ProductsController(IProductListingService productListingService) : ControllerBase
{
    private readonly IProductListingService _productListingService = productListingService;

    [HttpGet]
    [SwaggerOperation(
        Summary = "List products",
        Description = "Returns the first page of products from dbo.SUPITEMVIEW.")]
    [ProducesResponseType(typeof(ProductListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductListResponse>> List(CancellationToken cancellationToken = default)
    {
        var products = await _productListingService.ListAsync(new ProductListRequest(), cancellationToken);

        return Ok(products);
    }

    [HttpPost("search")]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "Search products",
        Description = "Returns paged products from dbo.SUPITEMVIEW. Search, filters, sort, and paging are supplied in the request body.")]
    [ProducesResponseType(typeof(ProductListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductListResponse>> Search(
        [FromBody] ProductListRequest request,
        CancellationToken cancellationToken = default)
    {
        var products = await _productListingService.ListAsync(request, cancellationToken);

        return Ok(products);
    }

    [HttpPost("autocomplete")]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "Get product autocomplete values",
        Description = "Returns distinct product values for supported columns from dbo.SUPITEMVIEW.")]
    [ProducesResponseType(typeof(ProductAutocompleteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductAutocompleteResponse>> Autocomplete(
        [FromBody] ProductAutocompleteRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!IsSupportedAutocompleteField(request.Field))
        {
            return BadRequest(new { message = "Unsupported product autocomplete field." });
        }

        var values = await _productListingService.GetAutocompleteValuesAsync(request, cancellationToken);

        return Ok(values);
    }

    private static bool IsSupportedAutocompleteField(string? field)
    {
        return string.Equals(field, "barcode", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(field, "description", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(field, "company", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(field, "saleRate", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(field, "salerate", StringComparison.OrdinalIgnoreCase);
    }
}
