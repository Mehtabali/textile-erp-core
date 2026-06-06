namespace ArunVastra.Infrastructure.Data.Entities;

public partial class City
{
    public int Cityid { get; set; }

    public int? Stateid { get; set; }

    public string? Cityname { get; set; }

    public virtual State? State { get; set; }
}
