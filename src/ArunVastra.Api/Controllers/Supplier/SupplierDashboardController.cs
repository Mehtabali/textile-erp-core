using ArunVastra.Api.Controllers;
using ArunVastra.Application.DTOs.SupplierDashboard;
using ArunVastra.Application.Interfaces;
using ArunVastra.Application.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ArunVastra.Api.Controllers.Supplier;

[Route("api/supplier/dashboard")]
[SwaggerTag("Supplier dashboard endpoints.")]
public sealed class SupplierDashboardController(ISupplierDashboardService supplierDashboardService) : ApiControllerBase
{
    private readonly ISupplierDashboardService _supplierDashboardService = supplierDashboardService;

    [HttpGet]
    [SwaggerOperation(
        Summary = "Get supplier dashboard",
        Description = "Returns supplier-specific sale voucher status counts and product activity counts.")]
    [ProducesResponseType(typeof(SupplierDashboardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SupplierDashboardResponse>> Get(CancellationToken cancellationToken)
    {
        try
        {
            var dashboard = await _supplierDashboardService.GetAsync(GetCurrentUser(), cancellationToken);

            return Ok(dashboard);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private CurrentUserContext GetCurrentUser()
    {
        if (!int.TryParse(CurrentUserId, out var userId))
        {
            throw new InvalidOperationException("Current user id is missing from token.");
        }

        _ = int.TryParse(CurrentUserRole, out var role);

        return new CurrentUserContext
        {
            UserId = userId,
            Role = role
        };
    }
}
