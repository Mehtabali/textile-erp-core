namespace ArunVastra.Application.DTOs.Auth;

public sealed class LoginResponse
{
    public bool Success { get; set; }

    public string? Token { get; set; }

    public DateTime? ExpiresAtUtc { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiresAtUtc { get; set; }

    public bool PasswordResetRequired { get; set; }

    public AuthenticatedUserDto? User { get; set; }

    public string? Message { get; set; }
}
