namespace ArunVastra.Application.DTOs.Users.Supplier;

public sealed class SupplierUserListRequest
{
    public string? SearchKeyword { get; set; }

    public bool IncludeLocked { get; set; } = true;

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public SupplierUserListFiltersRequest Filters { get; set; } = new();

    public SupplierUserListSortRequest? Sort { get; set; }
}

public sealed class SupplierUserListFiltersRequest
{
    public string? Code { get; set; }

    public string? Name { get; set; }

    public string? Brand { get; set; }

    public string? Dhara { get; set; }

    public string? Gstin { get; set; }

    public string? Phone { get; set; }

    public string? Mobile { get; set; }

    public string? City { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public string? Agent { get; set; }

    public string? Status { get; set; }
}

public sealed class SupplierUserListSortRequest
{
    public string? Field { get; set; }

    public string? Direction { get; set; }
}

public sealed class SupplierUserListResponse
{
    public IReadOnlyList<SupplierUserListItemResponse> Items { get; set; } = [];

    public int TotalRecords { get; set; }

    public int PageNumber { get; set; }

    public int PageSize { get; set; }
}

public sealed class SupplierUserAutocompleteRequest
{
    public string? Field { get; set; }

    public string? SearchKeyword { get; set; }

    public int MaxResults { get; set; } = 10;

    public bool IncludeLocked { get; set; } = true;
}

public sealed class SupplierUserAutocompleteResponse
{
    public IReadOnlyList<string> Values { get; set; } = [];
}

public sealed class SupplierUserResponse
{
    public int UserId { get; set; }

    public string? Code { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Password { get; set; }

    public string? Phone { get; set; }

    public string? Mobile { get; set; }

    public string? Gstin { get; set; }

    public string? BrandName { get; set; }

    public int? AgencyId { get; set; }

    public string? AgencyName { get; set; }

    public int? StateId { get; set; }

    public string? StateName { get; set; }

    public int? CityId { get; set; }

    public string? CityName { get; set; }

    public string? Address { get; set; }

    public int? DharaProfit { get; set; }

    public decimal? ExtraCharges { get; set; }

    public decimal? Discount { get; set; }

    public int? TransportId { get; set; }

    public string? TransportName { get; set; }

    public IReadOnlyList<SupplierOptionResponse> Transports { get; set; } = [];

    public int? ProductCategoryId { get; set; }

    public string? ProductCategoryName { get; set; }

    public string? Remarks { get; set; }

    public bool Locked { get; set; }
}

public sealed class CreateSupplierUserRequest
{
    public string? UserCode { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Mobile { get; set; }

    public string? Gstin { get; set; }

    public string? BrandName { get; set; }

    public int? AgencyId { get; set; }

    public int? StateId { get; set; }

    public int? CityId { get; set; }

    public string? Address { get; set; }

    public int? DharaProfit { get; set; }

    public decimal? ExtraCharges { get; set; }

    public decimal? Discount { get; set; }

    public int? TransportId { get; set; }

    public IReadOnlyList<int> TransportIds { get; set; } = [];

    public int? ProductCategoryId { get; set; }

    public string? Remarks { get; set; }
}

public sealed class UpdateSupplierUserRequest
{
    public string? UserCode { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Password { get; set; }

    public string? Phone { get; set; }

    public string? Mobile { get; set; }

    public string? Gstin { get; set; }

    public string? BrandName { get; set; }

    public int? AgencyId { get; set; }

    public int? StateId { get; set; }

    public int? CityId { get; set; }

    public string? Address { get; set; }

    public int? DharaProfit { get; set; }

    public decimal? ExtraCharges { get; set; }

    public decimal? Discount { get; set; }

    public int? TransportId { get; set; }

    public IReadOnlyList<int> TransportIds { get; set; } = [];

    public int? ProductCategoryId { get; set; }

    public string? Remarks { get; set; }
}

public sealed class ResetSupplierPasswordRequest
{
    public string Password { get; set; } = string.Empty;
}

public sealed class SupplierOptionResponse
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}
