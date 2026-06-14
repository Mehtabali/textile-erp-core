namespace ArunVastra.Infrastructure.Data.Entities;

public partial class Product
{
    public int Prodid { get; set; }

    public string Prodname { get; set; } = string.Empty;

    public int? Serial { get; set; }

    public bool Isactive { get; set; }

    public decimal? Gstper { get; set; }

    public string? Hsncode { get; set; }

    public decimal? Stitch { get; set; }

    public decimal? Gstpernew { get; set; }
}
