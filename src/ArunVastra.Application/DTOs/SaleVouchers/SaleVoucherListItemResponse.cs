namespace ArunVastra.Application.DTOs.SaleVouchers;

public sealed class SaleVoucherListItemResponse
{
    public int SaleVoucherId { get; set; }

    public int AutoBillNo { get; set; }

    public DateTime Date { get; set; }

    public string Challan { get; set; } = string.Empty;

    public int CompanyId { get; set; }

    public string CompanyName { get; set; } = string.Empty;

    public int SupplierUserId { get; set; }

    public string SupplierName { get; set; } = string.Empty;

    public int TransportId { get; set; }

    public string TransportName { get; set; } = string.Empty;

    public int? FloorId { get; set; }

    public string? FloorName { get; set; }

    public int Status { get; set; }

    public string StatusName { get; set; } = string.Empty;

    public bool CanCancel { get; set; }

    public IReadOnlyList<SaleVoucherActionResponse> AvailableActions { get; set; } = [];
}
