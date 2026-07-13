using ArunVastra.Api.Controllers;
using ArunVastra.Application.DTOs.SupplierCategoryMappings;
using ArunVastra.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ArunVastra.Api.Controllers.Admin;

[Route("api/admin/supplier-category-mapping")]
[SwaggerTag("Admin endpoints for mapping supplier users from dbo.USERS to product categories from dbo.PRODUCTS through dbo.SUPPRODUCTS.")]
public sealed class SupplierCategoryMappingController(
    ISupplierCategoryMappingService supplierCategoryMappingService) : ApiControllerBase
{
    private readonly ISupplierCategoryMappingService _supplierCategoryMappingService =
        supplierCategoryMappingService;

    [HttpGet]
    [SwaggerOperation(
        Summary = "List supplier category mappings",
        Description = "Returns the first page of supplier to product category mappings from dbo.SUPPRODUCTS.")]
    [ProducesResponseType(typeof(SupplierCategoryMappingListResponse), StatusCodes.Status200OK)]
    public Task<SupplierCategoryMappingListResponse> List(CancellationToken cancellationToken = default)
    {
        return _supplierCategoryMappingService.ListAsync(
            new SupplierCategoryMappingListRequest(),
            cancellationToken);
    }

    [HttpPost("list")]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "List supplier category mappings",
        Description = "Returns paged supplier to product category mappings. Search, filters, sort, and paging are supplied in the request body.")]
    [ProducesResponseType(typeof(SupplierCategoryMappingListResponse), StatusCodes.Status200OK)]
    public Task<SupplierCategoryMappingListResponse> List(
        [FromBody] SupplierCategoryMappingListRequest request,
        CancellationToken cancellationToken = default)
    {
        return _supplierCategoryMappingService.ListAsync(request, cancellationToken);
    }

    [HttpGet("{supplierUserId:int}")]
    [SwaggerOperation(
        Summary = "Get supplier category mapping",
        Description = "Returns mapped product categories for one supplier user.")]
    [ProducesResponseType(typeof(SupplierCategoryMappingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SupplierCategoryMappingResponse>> GetBySupplierUserId(
        int supplierUserId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var mapping = await _supplierCategoryMappingService.GetBySupplierUserIdAsync(
                supplierUserId,
                cancellationToken);

            if (mapping is null)
            {
                return NotFound();
            }

            return Ok(mapping);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("suppliers")]
    [SwaggerOperation(
        Summary = "Search supplier users",
        Description = "Returns supplier users where ROLE = 1 for supplier autocomplete.")]
    [ProducesResponseType(typeof(IReadOnlyList<SupplierCategoryMappingOptionResponse>), StatusCodes.Status200OK)]
    public Task<IReadOnlyList<SupplierCategoryMappingOptionResponse>> SearchSuppliers(
        [FromQuery] string? searchKeyword,
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        return _supplierCategoryMappingService.SearchSuppliersAsync(
            searchKeyword,
            take,
            cancellationToken);
    }

    [HttpGet("product-categories")]
    [SwaggerOperation(
        Summary = "Search product categories",
        Description = "Returns product categories from dbo.PRODUCTS for category autocomplete or multi-select.")]
    [ProducesResponseType(typeof(IReadOnlyList<SupplierCategoryMappingOptionResponse>), StatusCodes.Status200OK)]
    public Task<IReadOnlyList<SupplierCategoryMappingOptionResponse>> SearchProductCategories(
        [FromQuery] string? searchKeyword,
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        return _supplierCategoryMappingService.SearchProductCategoriesAsync(
            searchKeyword,
            take,
            cancellationToken);
    }

    [HttpPost]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "Save supplier category mapping",
        Description = "Synchronizes mapped product categories for one supplier. Existing mappings missing from the request are removed, existing selected mappings are kept, and new selections are inserted.")]
    [ProducesResponseType(typeof(SupplierCategoryMappingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SupplierCategoryMappingResponse>> Save(
        [FromBody] SaveSupplierCategoryMappingRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var mapping = await _supplierCategoryMappingService.SaveAsync(request, cancellationToken);

            return Ok(mapping);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{supplierUserId:int}/product-categories/{productCategoryId:int}")]
    [SwaggerOperation(
        Summary = "Remove supplier category mapping",
        Description = "Removes one product category mapping for one supplier user from dbo.SUPPRODUCTS.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Remove(
        int supplierUserId,
        int productCategoryId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _supplierCategoryMappingService.RemoveAsync(
                supplierUserId,
                productCategoryId,
                cancellationToken);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
