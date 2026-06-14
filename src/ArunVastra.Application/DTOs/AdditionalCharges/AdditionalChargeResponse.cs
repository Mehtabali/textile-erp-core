namespace ArunVastra.Application.DTOs.AdditionalCharges;

public sealed class AdditionalChargeResponse
{
    public int Id { get; set; }

    public int StockGroupId { get; set; }

    public string StockGroupName { get; set; } = string.Empty;

    public decimal GstValue { get; set; }

    public int ApplyOrder { get; set; }

    public decimal StartRange { get; set; }

    public decimal? EndRange { get; set; }
}

