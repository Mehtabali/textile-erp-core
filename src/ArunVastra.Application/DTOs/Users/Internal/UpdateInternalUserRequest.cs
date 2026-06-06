namespace ArunVastra.Application.DTOs.Users.Internal;

public sealed class UpdateInternalUserRequest
{
    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public int Role { get; set; } = 6;

    public string? Phone { get; set; }

    public string? Mobile { get; set; }

    public string? Remarks { get; set; }

    public bool Status { get; set; } = true;
}
