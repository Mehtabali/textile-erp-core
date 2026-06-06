namespace ArunVastra.Infrastructure.Data.Entities;

public partial class State
{
    public int Stateid { get; set; }

    public string? Statename { get; set; }

    public int? Gststatecode { get; set; }

    public virtual ICollection<City> Cities { get; set; } = new List<City>();
}
