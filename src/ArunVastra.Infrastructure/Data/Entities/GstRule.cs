namespace ArunVastra.Infrastructure.Data.Entities;

public partial class GstRule
{
    public int Id { get; set; }

    public int Productid { get; set; }

    public decimal? Gstvalue { get; set; }

    public decimal Startrange { get; set; }

    public decimal? Endrange { get; set; }

    public virtual Product Product { get; set; } = null!;
}
