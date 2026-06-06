namespace ArunVastra.Application.DTOs.Locations;

public sealed class StateResponse
{
    public int StateId { get; set; }

    public string? StateName { get; set; }

    public int? GstStateCode { get; set; }
}
