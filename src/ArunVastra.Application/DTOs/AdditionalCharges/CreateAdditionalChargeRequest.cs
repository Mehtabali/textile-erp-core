namespace ArunVastra.Application.DTOs.AdditionalCharges;

public sealed class CreateAdditionalChargeRequest
{
    public int StockGroupId { get; set; }

    public decimal? GstValue { get; set; }

    public decimal? StartRange { get; set; }

    public decimal? EndRange { get; set; }
}

