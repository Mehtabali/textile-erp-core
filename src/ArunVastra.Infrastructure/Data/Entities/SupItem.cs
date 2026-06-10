namespace ArunVastra.Infrastructure.Data.Entities;

public partial class SupItem
{
    public int Supprodid { get; set; }

    public int Prodid { get; set; }

    public int Userid { get; set; }

    public int? Compid { get; set; }

    public string Barcode { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal? Purchase { get; set; }

    public decimal? Mrpnew { get; set; }

    public bool Isactive { get; set; }

    public string? Hsncode { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
