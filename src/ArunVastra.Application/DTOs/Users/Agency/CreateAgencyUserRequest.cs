namespace ArunVastra.Application.DTOs.Users.Agency;

public sealed class CreateAgencyUserRequest
{
    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string ConfirmPassword { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Mobile { get; set; }

    public string? Gstin { get; set; }

    public string? BrandName { get; set; }

    public string? Address { get; set; }

    public int? StateId { get; set; }

    public int? CityId { get; set; }

    public string? Remarks { get; set; }

    public bool Status { get; set; } = true;
}
