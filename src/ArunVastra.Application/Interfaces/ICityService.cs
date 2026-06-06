using ArunVastra.Application.DTOs.Locations;

namespace ArunVastra.Application.Interfaces;

public interface ICityService
{
    Task<IReadOnlyList<CityResponse>> ListAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CityResponse>> ListByStateAsync(
        int stateId,
        CancellationToken cancellationToken = default);

    Task<CityResponse?> GetByIdAsync(int cityId, CancellationToken cancellationToken = default);

    Task<CityResponse> CreateAsync(
        CreateCityRequest request,
        CancellationToken cancellationToken = default);

    Task<CityResponse?> UpdateAsync(
        int cityId,
        UpdateCityRequest request,
        CancellationToken cancellationToken = default);
}
