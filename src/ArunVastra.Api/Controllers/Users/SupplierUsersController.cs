using ArunVastra.Application.DTOs.Users.Supplier;
using ArunVastra.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ArunVastra.Api.Controllers.Users;

[ApiController]
[Route("api/users/suppliers")]
[Produces("application/json")]
[SwaggerTag("Supplier user listing endpoints. These endpoints read supplier users from dbo.USERVIEW with ROLE = 1.")]
public sealed class SupplierUsersController(ISupplierUserService supplierUserService) : ControllerBase
{
    private readonly ISupplierUserService _supplierUserService = supplierUserService;

    [HttpGet]
    [SwaggerOperation(
        Summary = "List supplier users",
        Description = "Returns the first page of supplier users from dbo.USERVIEW where ROLE = 1.")]
    [ProducesResponseType(typeof(SupplierUserListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<SupplierUserListResponse>> List(CancellationToken cancellationToken = default)
    {
        var users = await _supplierUserService.ListAsync(new SupplierUserListRequest(), cancellationToken);

        return Ok(users);
    }

    [HttpPost("search")]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "List supplier users",
        Description = "Returns paged supplier users from dbo.USERVIEW where ROLE = 1. Search, filters, sort, and paging are supplied in the request body.")]
    [ProducesResponseType(typeof(SupplierUserListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<SupplierUserListResponse>> List(
        [FromBody] SupplierUserListRequest request,
        CancellationToken cancellationToken = default)
    {
        var users = await _supplierUserService.ListAsync(request, cancellationToken);

        return Ok(users);
    }
}
