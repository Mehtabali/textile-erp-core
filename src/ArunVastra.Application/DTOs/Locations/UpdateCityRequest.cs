namespace ArunVastra.Application.DTOs.Locations;

public sealed class UpdateCityRequest
{
    public int StateId { get; set; }

    public string? CityName { get; set; }
}
