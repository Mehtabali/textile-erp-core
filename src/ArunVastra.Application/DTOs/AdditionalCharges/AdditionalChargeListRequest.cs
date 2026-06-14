namespace ArunVastra.Application.DTOs.AdditionalCharges;

public sealed class AdditionalChargeListRequest
{
    public string? SearchKeyword { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public AdditionalChargeListFiltersRequest Filters { get; set; } = new();

    public AdditionalChargeListSortRequest? Sort { get; set; }
}

public sealed class AdditionalChargeListFiltersRequest
{
    public string? StockGroupName { get; set; }

    public string? GstValue { get; set; }

    public string? StartRange { get; set; }

    public string? EndRange { get; set; }

    public string? ApplyOrder { get; set; }
}

public sealed class AdditionalChargeListSortRequest
{
    public string? Field { get; set; }

    public string? Direction { get; set; }
}

