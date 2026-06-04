namespace ArunVastra.Application.DTOs.Users.Internal;

public sealed class InternalUserListRequest
{
    public string? SearchKeyword { get; set; }

    public bool IncludeLocked { get; set; } = true;

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public InternalUserListFiltersRequest Filters { get; set; } = new();

    public InternalUserListSortRequest? Sort { get; set; }
}

public sealed class InternalUserListFiltersRequest
{
    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Type { get; set; }

    public string? Phone { get; set; }

    public string? Mobile { get; set; }

    public string? Gstin { get; set; }

    public string? BrandName { get; set; }

    public string? Status { get; set; }
}

public sealed class InternalUserListSortRequest
{
    public string? Field { get; set; }

    public string? Direction { get; set; }
}

public sealed class InternalUserListResponse
{
    public IReadOnlyList<InternalUserListItemResponse> Items { get; set; } = [];

    public int TotalRecords { get; set; }

    public int PageNumber { get; set; }

    public int PageSize { get; set; }
}
