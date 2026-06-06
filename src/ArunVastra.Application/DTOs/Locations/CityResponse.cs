namespace ArunVastra.Application.DTOs.Locations;

public sealed class CityResponse
{
    public int CityId { get; set; }

    public int? StateId { get; set; }

    public string? CityName { get; set; }
}
