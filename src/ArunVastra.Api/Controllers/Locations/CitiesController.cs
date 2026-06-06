using ArunVastra.Application.DTOs.Locations;
using ArunVastra.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ArunVastra.Api.Controllers.Locations;

[ApiController]
[Route("api/cities")]
[Produces("application/json")]
[SwaggerTag("City lookup endpoints.")]
public sealed class CitiesController(ICityService cityService) : ControllerBase
{
    private readonly ICityService _cityService = cityService;

    [HttpGet]
    [SwaggerOperation(
        Summary = "List cities",
        Description = "Returns all cities from dbo.CITIES ordered by city name.")]
    [ProducesResponseType(typeof(IReadOnlyList<CityResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CityResponse>>> List(
        CancellationToken cancellationToken)
    {
        var cities = await _cityService.ListAsync(cancellationToken);

        return Ok(cities);
    }

    [HttpGet("{cityId:int}")]
    [SwaggerOperation(
        Summary = "Get city",
        Description = "Returns one city from dbo.CITIES by CITYID.")]
    [ProducesResponseType(typeof(CityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CityResponse>> GetById(
        int cityId,
        CancellationToken cancellationToken)
    {
        try
        {
            var city = await _cityService.GetByIdAsync(cityId, cancellationToken);

            if (city is null)
            {
                return NotFound();
            }

            return Ok(city);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "Create city",
        Description = "Creates a city in dbo.CITIES for the selected STATEID.")]
    [ProducesResponseType(typeof(CityResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CityResponse>> Create(
        [FromBody] CreateCityRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var city = await _cityService.CreateAsync(request, cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { cityId = city.CityId },
                city);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{cityId:int}")]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "Update city",
        Description = "Updates an existing city in dbo.CITIES.")]
    [ProducesResponseType(typeof(CityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CityResponse>> Update(
        int cityId,
        [FromBody] UpdateCityRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var city = await _cityService.UpdateAsync(cityId, request, cancellationToken);

            if (city is null)
            {
                return NotFound();
            }

            return Ok(city);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
