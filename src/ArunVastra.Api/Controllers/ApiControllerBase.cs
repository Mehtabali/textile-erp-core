using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ArunVastra.Api.Controllers;

[ApiController]
[Authorize]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    protected string? CurrentUserId =>
       User.FindFirstValue(ClaimTypes.NameIdentifier)
       ?? User.FindFirstValue("sub");

    protected string? CurrentUserEmail =>
        User.FindFirstValue(ClaimTypes.Email)
        ?? User.FindFirstValue("email");

    protected string? CurrentUserRole =>
        User.FindFirstValue(ClaimTypes.Role)
        ?? User.FindFirstValue("role");
}
