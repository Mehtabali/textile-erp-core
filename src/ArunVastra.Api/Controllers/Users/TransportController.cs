using ArunVastra.Application.DTOs.Users.Transport;
using ArunVastra.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ArunVastra.Api.Controllers.Users;

[ApiController]
[Route("api/users/transport")]
[Produces("application/json")]
[SwaggerTag("Transport user management endpoints. These endpoints only read and write USERS rows with ROLE = 2.")]
public sealed class TransportController(ITransportUserService transportUserService) : ControllerBase
{
    private readonly ITransportUserService _transportUserService = transportUserService;

    [HttpGet]
    [SwaggerOperation(
        Summary = "List transport users",
        Description = "Returns the first page of transport users from dbo.USERS where ROLE = 2.")]
    [ProducesResponseType(typeof(TransportUserListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TransportUserListResponse>> List(CancellationToken cancellationToken = default)
    {
        var users = await _transportUserService.ListAsync(new TransportUserListRequest(), cancellationToken);

        return Ok(users);
    }

    [HttpPost("search")]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "List transport users",
        Description = "Returns paged transport users from dbo.USERS where ROLE = 2. Search, filters, sort, and paging are supplied in the request body.")]
    [ProducesResponseType(typeof(TransportUserListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TransportUserListResponse>> List(
        [FromBody] TransportUserListRequest request,
        CancellationToken cancellationToken = default)
    {
        var users = await _transportUserService.ListAsync(request, cancellationToken);

        return Ok(users);
    }

    [HttpGet("{userId:int}")]
    [SwaggerOperation(
        Summary = "Get transport user",
        Description = "Returns one transport user by USERID. Only ROLE = 2 records are returned.")]
    [ProducesResponseType(typeof(TransportUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransportUserResponse>> GetById(
        int userId,
        CancellationToken cancellationToken)
    {
        var user = await _transportUserService.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpPost]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "Create transport user",
        Description = "Creates a transport user in dbo.USERS with ROLE = 2. Password is stored in PASSWORDHASH and confirmPassword must match password.")]
    [ProducesResponseType(typeof(TransportUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TransportUserResponse>> Create(
        [FromBody] CreateTransportUserRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _transportUserService.CreateAsync(request, cancellationToken);

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
        Summary = "Update transport user",
        Description = "Updates an existing transport user. Password changes are intentionally not handled by this endpoint.")]
    [ProducesResponseType(typeof(TransportUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransportUserResponse>> Update(
        int userId,
        [FromBody] UpdateTransportUserRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _transportUserService.UpdateAsync(userId, request, cancellationToken);

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
