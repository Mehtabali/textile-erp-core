using ArunVastra.Application.DTOs.Locations;
using ArunVastra.Application.Interfaces;

namespace ArunVastra.Application.Services;

public sealed class CityService(ICityRepository cityRepository) : ICityService
{
    private readonly ICityRepository _cityRepository = cityRepository;

    public Task<IReadOnlyList<CityResponse>> ListAsync(CancellationToken cancellationToken = default)
    {
        return _cityRepository.ListAsync(cancellationToken);
    }

    public Task<IReadOnlyList<CityResponse>> ListByStateAsync(
        int stateId,
        CancellationToken cancellationToken = default)
    {
        ValidateId(stateId, "State id");

        return _cityRepository.ListByStateAsync(stateId, cancellationToken);
    }

    public Task<CityResponse?> GetByIdAsync(
        int cityId,
        CancellationToken cancellationToken = default)
    {
        ValidateId(cityId, "City id");

        return _cityRepository.GetByIdAsync(cityId, cancellationToken);
    }

    public async Task<CityResponse> CreateAsync(
        CreateCityRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateId(request.StateId, "State id");
        var cityName = NormalizeName(request.CityName, "City name");

        if (!await _cityRepository.StateExistsAsync(request.StateId, cancellationToken))
        {
            throw new InvalidOperationException("State does not exist.");
        }

        if (await _cityRepository.NameExistsInStateAsync(
            request.StateId,
            cityName,
            cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException("City name already exists for this state.");
        }

        return await _cityRepository.CreateAsync(
            new CreateCityRequest
            {
                StateId = request.StateId,
                CityName = cityName
            },
            cancellationToken);
    }

    public async Task<CityResponse?> UpdateAsync(
        int cityId,
        UpdateCityRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateId(cityId, "City id");
        ValidateId(request.StateId, "State id");
        var cityName = NormalizeName(request.CityName, "City name");

        if (!await _cityRepository.StateExistsAsync(request.StateId, cancellationToken))
        {
            throw new InvalidOperationException("State does not exist.");
        }

        if (await _cityRepository.NameExistsInStateAsync(
            request.StateId,
            cityName,
            cityId,
            cancellationToken))
        {
            throw new InvalidOperationException("City name already exists for this state.");
        }

        return await _cityRepository.UpdateAsync(
            cityId,
            new UpdateCityRequest
            {
                StateId = request.StateId,
                CityName = cityName
            },
            cancellationToken);
    }

    private static void ValidateId(int id, string name)
    {
        if (id <= 0)
        {
            throw new InvalidOperationException($"{name} must be greater than zero.");
        }
    }

    private static string NormalizeName(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{name} is required.");
        }

        var normalized = value.Trim();
        if (normalized.Length > 50)
        {
            throw new InvalidOperationException($"{name} cannot exceed 50 characters.");
        }

        return normalized;
    }
}
