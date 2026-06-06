using ArunVastra.Application.DTOs.Locations;

namespace ArunVastra.Application.Interfaces;

public interface ICityRepository
{
    Task<IReadOnlyList<CityResponse>> ListAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CityResponse>> ListByStateAsync(
        int stateId,
        CancellationToken cancellationToken = default);

    Task<CityResponse?> GetByIdAsync(int cityId, CancellationToken cancellationToken = default);

    Task<bool> StateExistsAsync(int stateId, CancellationToken cancellationToken = default);

    Task<bool> CityBelongsToStateAsync(
        int cityId,
        int stateId,
        CancellationToken cancellationToken = default);

    Task<bool> NameExistsInStateAsync(
        int stateId,
        string cityName,
        int? excludingCityId = null,
        CancellationToken cancellationToken = default);

    Task<CityResponse> CreateAsync(
        CreateCityRequest request,
        CancellationToken cancellationToken = default);

    Task<CityResponse?> UpdateAsync(
        int cityId,
        UpdateCityRequest request,
        CancellationToken cancellationToken = default);
}
