using ArunVastra.Application.DTOs.Users.Internal;
using ArunVastra.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ArunVastra.Api.Controllers.Users;

[ApiController]
[Route("api/users/internal")]
[Produces("application/json")]
[SwaggerTag("Internal user management endpoints. These endpoints only read and write USERS rows with ROLE = 5, ROLE = 6, or ROLE = 7.")]
public sealed class InternalUsersController(IInternalUserService internalUserService) : ControllerBase
{
    private readonly IInternalUserService _internalUserService = internalUserService;

    [HttpGet]
    [SwaggerOperation(
        Summary = "List internal users",
        Description = "Returns the first page of internal users from dbo.USERS where ROLE = 5, ROLE = 6, or ROLE = 7.")]
    [ProducesResponseType(typeof(InternalUserListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<InternalUserListResponse>> List(CancellationToken cancellationToken = default)
    {
        var users = await _internalUserService.ListAsync(new InternalUserListRequest(), cancellationToken);

        return Ok(users);
    }

    [HttpPost("search")]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "List internal users",
        Description = "Returns paged internal users from dbo.USERS where ROLE = 5, ROLE = 6, or ROLE = 7. Search, filters, sort, and paging are supplied in the request body.")]
    [ProducesResponseType(typeof(InternalUserListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<InternalUserListResponse>> List(
        [FromBody] InternalUserListRequest request,
        CancellationToken cancellationToken = default)
    {
        var users = await _internalUserService.ListAsync(request, cancellationToken);

        return Ok(users);
    }

    [HttpGet("{userId:int}")]
    [SwaggerOperation(
        Summary = "Get internal user",
        Description = "Returns one internal user by USERID. Only ROLE = 5, ROLE = 6, or ROLE = 7 records are returned.")]
    [ProducesResponseType(typeof(InternalUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InternalUserResponse>> GetById(
        int userId,
        CancellationToken cancellationToken)
    {
        var user = await _internalUserService.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpPost]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "Create internal user",
        Description = "Creates an internal user in dbo.USERS with ROLE = 5, ROLE = 6, or ROLE = 7. Password is stored in PASSWORDHASH and confirmPassword must match password.")]
    [ProducesResponseType(typeof(InternalUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<InternalUserResponse>> Create(
        [FromBody] CreateInternalUserRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _internalUserService.CreateAsync(request, cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { userId = user.UserId },
                user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{userId:int}")]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "Update internal user",
        Description = "Updates an existing internal user. Password changes are intentionally not handled by this endpoint.")]
    [ProducesResponseType(typeof(InternalUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InternalUserResponse>> Update(
        int userId,
        [FromBody] UpdateInternalUserRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _internalUserService.UpdateAsync(userId, request, cancellationToken);

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
}
