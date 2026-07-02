namespace ArunVastra.Infrastructure.Data.Entities;

public sealed class SupItemView
{
    public int Supprodid { get; set; }

    public int Prodid { get; set; }

    public string? Prodname { get; set; }

    public int Userid { get; set; }

    public string? Firstname { get; set; }

    public int? Compid { get; set; }

    public string? Compname { get; set; }

    public string? Barcode { get; set; }

    public string? Description { get; set; }

    public bool? Formula { get; set; }

    public decimal? Purchase { get; set; }

    public int? Mrp { get; set; }

    public decimal? Mrpnew { get; set; }

    public bool Isactive { get; set; }

    public string? Hsncode { get; set; }
}
