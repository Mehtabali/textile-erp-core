namespace ArunVastra.Infrastructure.Data.Entities;

public partial class SaleVoucherDetail
{
    public int Svdetailid { get; set; }

    public int? Svid { get; set; }

    public int? Supprodid { get; set; }

    public decimal? Purchase { get; set; }

    public decimal? Mrp { get; set; }

    public int? Qty { get; set; }

    public virtual SaleVoucher? SaleVoucher { get; set; }

    public virtual SupItem? SupplierProduct { get; set; }
}
