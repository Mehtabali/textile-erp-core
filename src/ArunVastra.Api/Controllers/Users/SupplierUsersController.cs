using ArunVastra.Api.Controllers;
using ArunVastra.Application.DTOs.Users.Supplier;
using ArunVastra.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ArunVastra.Api.Controllers.Users;

[Route("api/users/suppliers")]
[SwaggerTag("Supplier user listing endpoints. These endpoints read supplier users from dbo.USERVIEW with ROLE = 1.")]
public sealed class SupplierUsersController(ISupplierUserService supplierUserService) : ApiControllerBase
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

    [HttpPost("autocomplete")]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "Get supplier autocomplete values",
        Description = "Returns distinct supplier values for supported columns from dbo.USERVIEW where ROLE = 1.")]
    [ProducesResponseType(typeof(SupplierUserAutocompleteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SupplierUserAutocompleteResponse>> Autocomplete(
        [FromBody] SupplierUserAutocompleteRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!IsSupportedAutocompleteField(request.Field))
        {
            return BadRequest(new { message = "Unsupported supplier autocomplete field." });
        }

        var values = await _supplierUserService.GetAutocompleteValuesAsync(request, cancellationToken);

        return Ok(values);
    }

    [HttpGet("next-code")]
    [SwaggerOperation(
        Summary = "Get next supplier user code",
        Description = "Returns the next USERCODE using legacy-compatible max(USERVIEW.USERCODE) + 1 with 4 digit padding.")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<ActionResult<object>> GetNextUserCode(CancellationToken cancellationToken = default)
    {
        var userCode = await _supplierUserService.GetNextUserCodeAsync(cancellationToken);

        return Ok(new { userCode });
    }

    [HttpGet("{userId:int}")]
    [SwaggerOperation(Summary = "Get supplier user", Description = "Returns one supplier user by USERID. Only ROLE = 1 records are returned.")]
    [ProducesResponseType(typeof(SupplierUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SupplierUserResponse>> GetById(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _supplierUserService.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpGet("agencies")]
    [SwaggerOperation(Summary = "Search agencies", Description = "Returns agency users where ROLE = 4 for agency dropdown.")]
    [ProducesResponseType(typeof(IReadOnlyList<SupplierOptionResponse>), StatusCodes.Status200OK)]
    public Task<IReadOnlyList<SupplierOptionResponse>> GetAgencyOptions(
        [FromQuery] string? searchKeyword,
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        return _supplierUserService.GetAgencyOptionsAsync(searchKeyword, take, cancellationToken);
    }

    [HttpPost]
    [Consumes("application/json")]
    [SwaggerOperation(Summary = "Create supplier user", Description = "Creates a supplier user in dbo.USERS with ROLE = 1.")]
    [ProducesResponseType(typeof(SupplierUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SupplierUserResponse>> Create(
        [FromBody] CreateSupplierUserRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _supplierUserService.CreateAsync(request, cancellationToken);

            return CreatedAtAction(nameof(GetById), new { userId = user.UserId }, user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{userId:int}")]
    [Consumes("application/json")]
    [SwaggerOperation(Summary = "Update supplier user", Description = "Updates an existing supplier user.")]
    [ProducesResponseType(typeof(SupplierUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SupplierUserResponse>> Update(
        int userId,
        [FromBody] UpdateSupplierUserRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _supplierUserService.UpdateAsync(userId, request, cancellationToken);

            if (user is null)
            {
                return NotFound();
            }

            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{userId:int}")]
    [SwaggerOperation(Summary = "Delete supplier user", Description = "Deletes one supplier user and its current supplier mapping rows.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _supplierUserService.DeleteAsync(userId, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{userId:int}/lock")]
    [SwaggerOperation(Summary = "Toggle supplier user lock", Description = "Toggles USERS.LOCKED for the supplier.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Lock(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _supplierUserService.LockAsync(userId, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{userId:int}/reset-password")]
    [Consumes("application/json")]
    [SwaggerOperation(Summary = "Reset supplier password", Description = "Stores plain password in PWHASH and updates PASSWORDHASH using current password hashing.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(
        int userId,
        [FromBody] ResetSupplierPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _supplierUserService.ResetPasswordAsync(userId, request, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private static bool IsSupportedAutocompleteField(string? field)
    {
        return string.Equals(field, "name", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(field, "agent", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(field, "city", StringComparison.OrdinalIgnoreCase);
    }
}
