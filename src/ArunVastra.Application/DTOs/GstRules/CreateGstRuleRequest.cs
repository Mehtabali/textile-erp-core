namespace ArunVastra.Application.DTOs.GstRules;

public sealed class CreateGstRuleRequest
{
    public int StockGroupId { get; set; }

    public decimal? GstValue { get; set; }

    public decimal? StartRange { get; set; }

    public decimal? EndRange { get; set; }
}
