namespace ArunVastra.Application.DTOs.Locations;

public sealed class CreateStateRequest
{
    public string? StateName { get; set; }

    public int? GstStateCode { get; set; }
}
