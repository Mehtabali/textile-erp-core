using ArunVastra.Application.DTOs.SupplierTransportMappings;
using ArunVastra.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ArunVastra.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/supplier-transport-mapping")]
[Produces("application/json")]
[SwaggerTag("Admin endpoints for mapping supplier users from dbo.USERS to transport users from dbo.USERS.")]
public sealed class SupplierTransportMappingController(
    ISupplierTransportMappingService supplierTransportMappingService) : ControllerBase
{
    private readonly ISupplierTransportMappingService _supplierTransportMappingService =
        supplierTransportMappingService;

    [HttpGet]
    [SwaggerOperation(
        Summary = "List supplier transport mappings",
        Description = "Returns the first page of supplier to transport mappings from dbo.SUPPLIERTRANSPORTMAPPING.")]
    [ProducesResponseType(typeof(SupplierTransportMappingListResponse), StatusCodes.Status200OK)]
    public Task<SupplierTransportMappingListResponse> List(CancellationToken cancellationToken = default)
    {
        return _supplierTransportMappingService.ListAsync(
            new SupplierTransportMappingListRequest(),
            cancellationToken);
    }

    [HttpPost("list")]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "List supplier transport mappings",
        Description = "Returns paged supplier to transport mappings. Search, filters, sort, and paging are supplied in the request body.")]
    [ProducesResponseType(typeof(SupplierTransportMappingListResponse), StatusCodes.Status200OK)]
    public Task<SupplierTransportMappingListResponse> List(
        [FromBody] SupplierTransportMappingListRequest request,
        CancellationToken cancellationToken = default)
    {
        return _supplierTransportMappingService.ListAsync(request, cancellationToken);
    }

    [HttpGet("{supplierUserId:int}")]
    [SwaggerOperation(
        Summary = "Get supplier transport mapping",
        Description = "Returns mapped transports for one supplier user.")]
    [ProducesResponseType(typeof(SupplierTransportMappingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SupplierTransportMappingResponse>> GetBySupplierUserId(
        int supplierUserId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var mapping = await _supplierTransportMappingService.GetBySupplierUserIdAsync(
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
    [ProducesResponseType(typeof(IReadOnlyList<SupplierTransportMappingOptionResponse>), StatusCodes.Status200OK)]
    public Task<IReadOnlyList<SupplierTransportMappingOptionResponse>> SearchSuppliers(
        [FromQuery] string? searchKeyword,
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        return _supplierTransportMappingService.SearchSuppliersAsync(
            searchKeyword,
            take,
            cancellationToken);
    }

    [HttpGet("transports")]
    [SwaggerOperation(
        Summary = "Search transports",
        Description = "Returns transport users where ROLE = 2 for transport autocomplete or multi-select.")]
    [ProducesResponseType(typeof(IReadOnlyList<SupplierTransportMappingOptionResponse>), StatusCodes.Status200OK)]
    public Task<IReadOnlyList<SupplierTransportMappingOptionResponse>> SearchTransports(
        [FromQuery] string? searchKeyword,
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        return _supplierTransportMappingService.SearchTransportsAsync(
            searchKeyword,
            take,
            cancellationToken);
    }

    [HttpPost]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "Save supplier transport mapping",
        Description = "Synchronizes mapped transports for one supplier. Existing mappings missing from the request are removed, existing selected mappings are kept, and new selections are inserted.")]
    [ProducesResponseType(typeof(SupplierTransportMappingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SupplierTransportMappingResponse>> Save(
        [FromBody] SaveSupplierTransportMappingRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var mapping = await _supplierTransportMappingService.SaveAsync(request, cancellationToken);

            return Ok(mapping);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{supplierUserId:int}/transports/{transportUserId:int}")]
    [SwaggerOperation(
        Summary = "Remove supplier transport mapping",
        Description = "Removes one transport mapping for one supplier user from dbo.SUPPLIERTRANSPORTMAPPING.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Remove(
        int supplierUserId,
        int transportUserId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _supplierTransportMappingService.RemoveAsync(
                supplierUserId,
                transportUserId,
                cancellationToken);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
