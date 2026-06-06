namespace ArunVastra.Application.DTOs.Locations;

public sealed class UpdateStateRequest
{
    public string? StateName { get; set; }

    public int? GstStateCode { get; set; }
}
