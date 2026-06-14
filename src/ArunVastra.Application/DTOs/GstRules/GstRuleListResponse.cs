namespace ArunVastra.Application.DTOs.GstRules;

public sealed class GstRuleListResponse
{
    public IReadOnlyList<GstRuleResponse> Items { get; set; } = [];

    public int TotalRecords { get; set; }

    public int PageNumber { get; set; }

    public int PageSize { get; set; }
}
