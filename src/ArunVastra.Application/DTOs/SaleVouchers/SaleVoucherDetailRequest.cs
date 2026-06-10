namespace ArunVastra.Application.DTOs.SaleVouchers;

public sealed class SaleVoucherDetailRequest
{
    public int? SaleVoucherDetailId { get; set; }

    public int SupplierProductId { get; set; }

    public decimal Purchase { get; set; }

    public decimal Mrp { get; set; }

    public int Quantity { get; set; }
}
