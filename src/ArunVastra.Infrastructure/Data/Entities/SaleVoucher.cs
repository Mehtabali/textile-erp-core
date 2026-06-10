namespace ArunVastra.Infrastructure.Data.Entities;

public partial class SaleVoucher
{
    public int Svid { get; set; }

    public int Autobillno { get; set; }

    public int Compid { get; set; }

    public int Transid { get; set; }

    public DateTime Date { get; set; }

    public string Challan { get; set; } = string.Empty;

    public int Profit { get; set; }

    public byte Status { get; set; }

    public int? Floorid { get; set; }

    public string? Istsynched { get; set; }

    public virtual Company Company { get; set; } = null!;

    public virtual User Transport { get; set; } = null!;

    public virtual ICollection<SaleVoucherDetail> Details { get; set; } = new List<SaleVoucherDetail>();
}
