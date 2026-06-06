namespace ArunVastra.Application.DTOs.Locations;

public sealed class CreateCityRequest
{
    public int StateId { get; set; }

    public string? CityName { get; set; }
}
