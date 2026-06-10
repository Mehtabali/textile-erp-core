namespace ArunVastra.Application.DTOs.SaleVouchers;

public sealed class SaleVoucherDetailResponse
{
    public int SaleVoucherDetailId { get; set; }

    public int SupplierProductId { get; set; }

    public string? ProductName { get; set; }

    public string? Description { get; set; }

    public string? BarCode { get; set; }

    public string? HsnCode { get; set; }

    public decimal Purchase { get; set; }

    public decimal Mrp { get; set; }

    public int Quantity { get; set; }
}
