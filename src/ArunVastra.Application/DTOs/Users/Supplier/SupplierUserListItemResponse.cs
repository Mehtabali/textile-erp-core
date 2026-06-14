namespace ArunVastra.Application.DTOs.Users.Supplier;

public sealed class SupplierUserListItemResponse
{
    public int UserId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Brand { get; set; }

    public int? Dhara { get; set; }

    public string? Gstin { get; set; }

    public string? Phone { get; set; }

    public string? Mobile { get; set; }

    public string? City { get; set; }

    public string Email { get; set; } = string.Empty;

    public string? Password { get; set; }

    public string? Agent { get; set; }

    public string Status { get; set; } = string.Empty;
}
