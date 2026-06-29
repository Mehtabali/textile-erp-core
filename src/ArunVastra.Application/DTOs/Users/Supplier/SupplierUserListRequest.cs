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
