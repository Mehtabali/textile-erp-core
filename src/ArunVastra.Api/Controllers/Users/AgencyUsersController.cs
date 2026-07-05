using ArunVastra.Application.DTOs.Users.Agency;
using ArunVastra.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ArunVastra.Api.Controllers.Users;

[ApiController]
[Route("api/users/agency")]
[Produces("application/json")]
[SwaggerTag("Agency user management endpoints. These endpoints only read and write USERS rows with ROLE = 4.")]
public sealed class AgencyUsersController(IAgencyUserService agencyUserService) : ControllerBase
{
    private readonly IAgencyUserService _agencyUserService = agencyUserService;

    [HttpGet]
    [SwaggerOperation(
        Summary = "List agency users",
        Description = "Returns the first page of agency users from dbo.USERS where ROLE = 4.")]
    [ProducesResponseType(typeof(AgencyUserListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AgencyUserListResponse>> List(CancellationToken cancellationToken = default)
    {
        var users = await _agencyUserService.ListAsync(new AgencyUserListRequest(), cancellationToken);

        return Ok(users);
    }

    [HttpPost("search")]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "List agency users",
        Description = "Returns paged agency users from dbo.USERS where ROLE = 4. Search, filters, sort, and paging are supplied in the request body.")]
    [ProducesResponseType(typeof(AgencyUserListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AgencyUserListResponse>> List(
        [FromBody] AgencyUserListRequest request,
        CancellationToken cancellationToken = default)
    {
        var users = await _agencyUserService.ListAsync(request, cancellationToken);

        return Ok(users);
    }

    [HttpGet("{userId:int}")]
    [SwaggerOperation(
        Summary = "Get agency user",
        Description = "Returns one agency user by USERID. Only ROLE = 4 records are returned.")]
    [ProducesResponseType(typeof(AgencyUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AgencyUserResponse>> GetById(
        int userId,
        CancellationToken cancellationToken)
    {
        var user = await _agencyUserService.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpPost]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "Create agency user",
        Description = "Creates an agency user in dbo.USERS with ROLE = 4. A default password is assigned on the backend and is not returned in API responses.")]
    [ProducesResponseType(typeof(AgencyUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AgencyUserResponse>> Create(
        [FromBody] CreateAgencyUserRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _agencyUserService.CreateAsync(request, cancellationToken);

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
        Summary = "Update agency user",
        Description = "Updates an existing agency user. Password changes are intentionally not handled by this endpoint.")]
    [ProducesResponseType(typeof(AgencyUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AgencyUserResponse>> Update(
        int userId,
        [FromBody] UpdateAgencyUserRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _agencyUserService.UpdateAsync(userId, request, cancellationToken);

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
