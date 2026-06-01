namespace ArunVastra.Application.DTOs.Auth;

public sealed class AuthenticatedUserDto
{
    public string UserId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Role { get; set; }
}
