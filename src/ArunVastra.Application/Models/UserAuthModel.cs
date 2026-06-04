namespace ArunVastra.Application.Models;

public sealed class UserAuthModel
{
    public string UserId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Role { get; set; }

    public string? LegacyPassword { get; set; }

    public string? PasswordHash { get; set; }

    public bool PasswordMigrated { get; set; }

    public bool PasswordResetRequired { get; set; }

    public bool Locked { get; set; }
}
