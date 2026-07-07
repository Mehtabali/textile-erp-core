using System.Security.Claims;
using ArunVastra.Application.DTOs.SupplierDashboard;
using ArunVastra.Application.Interfaces;
using ArunVastra.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ArunVastra.Api.Controllers.Supplier;

[ApiController]
[Authorize]
[Route("api/supplier/dashboard")]
[Produces("application/json")]
[SwaggerTag("Supplier dashboard endpoints.")]
public sealed class SupplierDashboardController(ISupplierDashboardService supplierDashboardService) : ControllerBase
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
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var roleValue = User.FindFirstValue(ClaimTypes.Role);

        if (!int.TryParse(userIdValue, out var userId))
        {
            throw new InvalidOperationException("Current user id is missing from token.");
        }

        _ = int.TryParse(roleValue, out var role);

        return new CurrentUserContext
        {
            UserId = userId,
            Role = role
        };
    }
}
