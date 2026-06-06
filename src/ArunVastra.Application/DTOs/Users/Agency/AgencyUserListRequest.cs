namespace ArunVastra.Application.DTOs.Users.Agency;

public sealed class AgencyUserListRequest
{
    public string? SearchKeyword { get; set; }

    public bool IncludeLocked { get; set; } = true;

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public AgencyUserListFiltersRequest Filters { get; set; } = new();

    public AgencyUserListSortRequest? Sort { get; set; }
}

public sealed class AgencyUserListFiltersRequest
{
    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Mobile { get; set; }

    public string? Gstin { get; set; }

    public string? BrandName { get; set; }

    public string? StateName { get; set; }

    public string? CityName { get; set; }

    public string? Status { get; set; }
}

public sealed class AgencyUserListSortRequest
{
    public string? Field { get; set; }

    public string? Direction { get; set; }
}

public sealed class AgencyUserListResponse
{
    public IReadOnlyList<AgencyUserListItemResponse> Items { get; set; } = [];

    public int TotalRecords { get; set; }

    public int PageNumber { get; set; }

    public int PageSize { get; set; }
}
