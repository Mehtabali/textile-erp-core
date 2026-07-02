namespace ArunVastra.Infrastructure.Data.Entities;

public partial class SupplierTransportMapping
{
    public int Id { get; set; }

    public int Supplieruserid { get; set; }

    public int Transportid { get; set; }

    public virtual User Supplier { get; set; } = null!;

    public virtual User Transport { get; set; } = null!;
}
