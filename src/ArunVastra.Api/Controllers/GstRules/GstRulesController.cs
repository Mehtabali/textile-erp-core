using ArunVastra.Api.Controllers;
using ArunVastra.Application.DTOs.GstRules;
using ArunVastra.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ArunVastra.Api.Controllers.GstRules;

[Route("api/gst-rules")]
[SwaggerTag("GST rule endpoints. These endpoints read and write dbo.GSTRULES.")]
public sealed class GstRulesController(IGstRuleService gstRuleService) : ApiControllerBase
{
    private readonly IGstRuleService _gstRuleService = gstRuleService;

    [HttpGet]
    [SwaggerOperation(
        Summary = "List GST rules",
        Description = "Returns the first page of GST rules from dbo.GSTRULES.")]
    [ProducesResponseType(typeof(GstRuleListResponse), StatusCodes.Status200OK)]
    public Task<GstRuleListResponse> List(CancellationToken cancellationToken = default)
    {
        return _gstRuleService.ListAsync(new GstRuleListRequest(), cancellationToken);
    }

    [HttpPost("list")]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "List GST rules",
        Description = "Returns paged GST rules. Search, filters, sort, and paging are supplied in the request body.")]
    [ProducesResponseType(typeof(GstRuleListResponse), StatusCodes.Status200OK)]
    public Task<GstRuleListResponse> List(
        [FromBody] GstRuleListRequest request,
        CancellationToken cancellationToken = default)
    {
        return _gstRuleService.ListAsync(request, cancellationToken);
    }

    [HttpGet("{gstRuleId:int}")]
    [SwaggerOperation(
        Summary = "Get GST rule",
        Description = "Returns one GST rule from dbo.GSTRULES by Id.")]
    [ProducesResponseType(typeof(GstRuleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GstRuleResponse>> GetById(
        int gstRuleId,
        CancellationToken cancellationToken)
    {
        try
        {
            var gstRule = await _gstRuleService.GetByIdAsync(gstRuleId, cancellationToken);

            if (gstRule is null)
            {
                return NotFound();
            }

            return Ok(gstRule);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("categories")]
    [SwaggerOperation(
        Summary = "List product categories for GST rules",
        Description = "Returns product categories from dbo.PRODUCTS for the GST rule product category dropdown.")]
    [ProducesResponseType(typeof(IReadOnlyList<GstRuleCategoryOptionResponse>), StatusCodes.Status200OK)]
    public Task<IReadOnlyList<GstRuleCategoryOptionResponse>> ListCategories(
        CancellationToken cancellationToken)
    {
        return _gstRuleService.ListCategoryOptionsAsync(cancellationToken);
    }

    [HttpPost]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "Create GST rule",
        Description = "Creates a GST rule in dbo.GSTRULES.")]
    [ProducesResponseType(typeof(GstRuleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GstRuleResponse>> Create(
        [FromBody] CreateGstRuleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var gstRule = await _gstRuleService.CreateAsync(request, cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { gstRuleId = gstRule.Id },
                gstRule);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{gstRuleId:int}")]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "Update GST rule",
        Description = "Updates an existing GST rule in dbo.GSTRULES.")]
    [ProducesResponseType(typeof(GstRuleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GstRuleResponse>> Update(
        int gstRuleId,
        [FromBody] UpdateGstRuleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var gstRule = await _gstRuleService.UpdateAsync(gstRuleId, request, cancellationToken);

            if (gstRule is null)
            {
                return NotFound();
            }

            return Ok(gstRule);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
