using ArunVastra.Api.Controllers;
using ArunVastra.Application.DTOs.Locations;
using ArunVastra.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ArunVastra.Api.Controllers.Locations;

[Route("api/states")]
[SwaggerTag("State lookup endpoints for cascading state and city dropdowns.")]
public sealed class StatesController(
    IStateService stateService,
    ICityService cityService) : ApiControllerBase
{
    private readonly IStateService _stateService = stateService;
    private readonly ICityService _cityService = cityService;

    [HttpGet]
    [SwaggerOperation(
        Summary = "List states",
        Description = "Returns all states from dbo.STATES ordered by state name.")]
    [ProducesResponseType(typeof(IReadOnlyList<StateResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<StateResponse>>> List(
        CancellationToken cancellationToken)
    {
        var states = await _stateService.ListAsync(cancellationToken);

        return Ok(states);
    }

    [HttpGet("{stateId:int}")]
    [SwaggerOperation(
        Summary = "Get state",
        Description = "Returns one state from dbo.STATES by STATEID.")]
    [ProducesResponseType(typeof(StateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StateResponse>> GetById(
        int stateId,
        CancellationToken cancellationToken)
    {
        try
        {
            var state = await _stateService.GetByIdAsync(stateId, cancellationToken);

            if (state is null)
            {
                return NotFound();
            }

            return Ok(state);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "Create state",
        Description = "Creates a state in dbo.STATES.")]
    [ProducesResponseType(typeof(StateResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<StateResponse>> Create(
        [FromBody] CreateStateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var state = await _stateService.CreateAsync(request, cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { stateId = state.StateId },
                state);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{stateId:int}")]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "Update state",
        Description = "Updates an existing state in dbo.STATES.")]
    [ProducesResponseType(typeof(StateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StateResponse>> Update(
        int stateId,
        [FromBody] UpdateStateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var state = await _stateService.UpdateAsync(stateId, request, cancellationToken);

            if (state is null)
            {
                return NotFound();
            }

            return Ok(state);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{stateId:int}/cities")]
    [SwaggerOperation(
        Summary = "List cities by state",
        Description = "Returns cities from dbo.CITIES for the selected STATEID. Use this endpoint for cascading city dropdowns.")]
    [ProducesResponseType(typeof(IReadOnlyList<CityResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<CityResponse>>> ListCities(
        int stateId,
        CancellationToken cancellationToken)
    {
        try
        {
            var cities = await _cityService.ListByStateAsync(stateId, cancellationToken);

            return Ok(cities);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
