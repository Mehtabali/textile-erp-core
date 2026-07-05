namespace ArunVastra.Application.Models.Users;

public sealed class SupplierUserUpdateModel
{
    public string? UserCode { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Mobile { get; set; }

    public string? Gstin { get; set; }

    public string? BrandName { get; set; }

    public int? AgencyId { get; set; }

    public string? AgencyName { get; set; }

    public int? CityId { get; set; }

    public string? Address { get; set; }

    public int? DharaProfit { get; set; }

    public decimal? ExtraCharges { get; set; }

    public decimal? Discount { get; set; }

    public string? Remarks { get; set; }
}
