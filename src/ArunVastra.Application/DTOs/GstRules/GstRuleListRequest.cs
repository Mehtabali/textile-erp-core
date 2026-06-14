namespace ArunVastra.Application.DTOs.GstRules;

public sealed class GstRuleListRequest
{
    public string? SearchKeyword { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public GstRuleListFiltersRequest Filters { get; set; } = new();

    public GstRuleListSortRequest? Sort { get; set; }
}

public sealed class GstRuleListFiltersRequest
{
    public string? StockGroupName { get; set; }

    public string? GstValue { get; set; }

    public string? StartRange { get; set; }

    public string? EndRange { get; set; }

    public string? ApplyOrder { get; set; }
}

public sealed class GstRuleListSortRequest
{
    public string? Field { get; set; }

    public string? Direction { get; set; }
}
