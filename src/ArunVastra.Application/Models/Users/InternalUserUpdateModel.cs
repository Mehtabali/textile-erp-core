namespace ArunVastra.Application.Models.Users;

public sealed class InternalUserUpdateModel
{
    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public int Role { get; set; }

    public string? Phone { get; set; }

    public string? Mobile { get; set; }

    public string? Gstin { get; set; }

    public string? BrandName { get; set; }

    public string? Remarks { get; set; }

    public bool Status { get; set; } = true;
}
