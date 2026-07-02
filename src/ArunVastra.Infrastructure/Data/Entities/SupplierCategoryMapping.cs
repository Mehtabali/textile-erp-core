namespace ArunVastra.Infrastructure.Data.Entities;

public partial class SupplierCategoryMapping
{
    public int Userid { get; set; }

    public int Prodid { get; set; }

    public virtual User Supplier { get; set; } = null!;

    public virtual Product ProductCategory { get; set; } = null!;
}
