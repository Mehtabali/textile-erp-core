namespace ArunVastra.Application.DTOs.SaleVouchers;

public sealed class SaleVoucherResponse
{
    public int SaleVoucherId { get; set; }

    public int AutoBillNo { get; set; }

    public int CompanyId { get; set; }

    public string CompanyName { get; set; } = string.Empty;

    public string? CompanyGstin { get; set; }

    public string? CompanyPan { get; set; }

    public string? CompanyTin { get; set; }

    public int SupplierUserId { get; set; }

    public string SupplierName { get; set; } = string.Empty;

    public int TransportId { get; set; }

    public string TransportName { get; set; } = string.Empty;

    public int? FloorId { get; set; }

    public string? FloorName { get; set; }

    public DateTime Date { get; set; }

    public string Challan { get; set; } = string.Empty;

    public int Profit { get; set; }

    public int Status { get; set; }

    public string StatusName { get; set; } = string.Empty;

    public bool CanCancel { get; set; }

    public IReadOnlyList<SaleVoucherActionResponse> AvailableActions { get; set; } = [];

    public int TotalRows { get; set; }

    public int TotalPieces { get; set; }

    public decimal GrandTotal { get; set; }

    public IReadOnlyList<SaleVoucherDetailResponse> Details { get; set; } = [];
}
