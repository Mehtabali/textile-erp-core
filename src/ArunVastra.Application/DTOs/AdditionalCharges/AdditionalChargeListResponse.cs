namespace ArunVastra.Application.DTOs.AdditionalCharges;

public sealed class AdditionalChargeListResponse
{
    public IReadOnlyList<AdditionalChargeResponse> Items { get; set; } = [];

    public int TotalRecords { get; set; }

    public int PageNumber { get; set; }

    public int PageSize { get; set; }
}

