namespace ArunVastra.Application.DTOs.Users.Transport;

public sealed class TransportUserListItemResponse
{
    public int UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public int Role { get; set; }

    public string? Phone { get; set; }

    public string? Mobile { get; set; }

    public string? Gstin { get; set; }

    public string? BrandName { get; set; }

    public int? StateId { get; set; }

    public string? StateName { get; set; }

    public int? CityId { get; set; }

    public string? CityName { get; set; }

    public string? Remarks { get; set; }

    public bool Status { get; set; }

    public DateTime? LastLoginAt { get; set; }
}
