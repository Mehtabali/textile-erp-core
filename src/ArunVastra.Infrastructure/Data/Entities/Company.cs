namespace ArunVastra.Infrastructure.Data.Entities;

public partial class Company
{
    public int Compid { get; set; }

    public int Userid { get; set; }

    public string Compname { get; set; } = string.Empty;

    public int Cityid { get; set; }

    public string Address { get; set; } = string.Empty;

    public string Pan { get; set; } = string.Empty;

    public string Tin { get; set; } = string.Empty;

    public string? Gstin { get; set; }

    public bool? Isactive { get; set; }

    public virtual User User { get; set; } = null!;
}
