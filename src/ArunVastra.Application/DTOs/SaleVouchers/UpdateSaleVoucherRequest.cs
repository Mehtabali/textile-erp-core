namespace ArunVastra.Application.DTOs.SaleVouchers;

public sealed class UpdateSaleVoucherRequest
{
    public int CompanyId { get; set; }

    public int TransportId { get; set; }

    public int FloorId { get; set; }

    public string Challan { get; set; } = string.Empty;

    public int Status { get; set; }

    public IReadOnlyList<SaleVoucherDetailRequest> Details { get; set; } = [];
}
