using ArunVastra.Application.DTOs.AdditionalCharges;
using ArunVastra.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ArunVastra.Api.Controllers.AdditionalCharges;

[ApiController]
[Route("api/additional-charges")]
[Produces("application/json")]
[SwaggerTag("Additional charge endpoints. These endpoints read and write dbo.ADDITIONALCHARGES.")]
public sealed class AdditionalChargesController(IAdditionalChargeService additionalChargeService) : ControllerBase
{
    private readonly IAdditionalChargeService _additionalChargeService = additionalChargeService;

    [HttpGet]
    [SwaggerOperation(
        Summary = "List additional charges",
        Description = "Returns the first page of additional charges from dbo.AdditionalCharges.")]
    [ProducesResponseType(typeof(AdditionalChargeListResponse), StatusCodes.Status200OK)]
    public Task<AdditionalChargeListResponse> List(CancellationToken cancellationToken = default)
    {
        return _additionalChargeService.ListAsync(new AdditionalChargeListRequest(), cancellationToken);
    }

    [HttpPost("list")]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "List additional charges",
        Description = "Returns paged additional charges. Search, filters, sort, and paging are supplied in the request body.")]
    [ProducesResponseType(typeof(AdditionalChargeListResponse), StatusCodes.Status200OK)]
    public Task<AdditionalChargeListResponse> List(
        [FromBody] AdditionalChargeListRequest request,
        CancellationToken cancellationToken = default)
    {
        return _additionalChargeService.ListAsync(request, cancellationToken);
    }

    [HttpGet("{additionalChargeId:int}")]
    [SwaggerOperation(
        Summary = "Get additional charge",
        Description = "Returns one additional charge from dbo.ADDITIONALCHARGES by Id.")]
    [ProducesResponseType(typeof(AdditionalChargeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdditionalChargeResponse>> GetById(
        int additionalChargeId,
        CancellationToken cancellationToken)
    {
        try
        {
            var additionalCharge = await _additionalChargeService.GetByIdAsync(additionalChargeId, cancellationToken);

            if (additionalCharge is null)
            {
                return NotFound();
            }

            return Ok(additionalCharge);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("categories")]
    [SwaggerOperation(
        Summary = "List product categories for additional charges",
        Description = "Returns product categories from dbo.PRODUCTS for the additional charge product category dropdown.")]
    [ProducesResponseType(typeof(IReadOnlyList<AdditionalChargeCategoryOptionResponse>), StatusCodes.Status200OK)]
    public Task<IReadOnlyList<AdditionalChargeCategoryOptionResponse>> ListCategories(
        CancellationToken cancellationToken)
    {
        return _additionalChargeService.ListCategoryOptionsAsync(cancellationToken);
    }

    [HttpPost]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "Create additional charge",
        Description = "Creates an additional charge in dbo.ADDITIONALCHARGES.")]
    [ProducesResponseType(typeof(AdditionalChargeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AdditionalChargeResponse>> Create(
        [FromBody] CreateAdditionalChargeRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var additionalCharge = await _additionalChargeService.CreateAsync(request, cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { additionalChargeId = additionalCharge.Id },
                additionalCharge);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{additionalChargeId:int}")]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "Update additional charge",
        Description = "Updates an existing additional charge in dbo.ADDITIONALCHARGES.")]
    [ProducesResponseType(typeof(AdditionalChargeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdditionalChargeResponse>> Update(
        int additionalChargeId,
        [FromBody] UpdateAdditionalChargeRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var additionalCharge = await _additionalChargeService.UpdateAsync(additionalChargeId, request, cancellationToken);

            if (additionalCharge is null)
            {
                return NotFound();
            }

            return Ok(additionalCharge);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

