using ArunVastra.Application.DTOs.Locations;

namespace ArunVastra.Application.Interfaces;

public interface IStateService
{
    Task<IReadOnlyList<StateResponse>> ListAsync(CancellationToken cancellationToken = default);

    Task<StateResponse?> GetByIdAsync(int stateId, CancellationToken cancellationToken = default);

    Task<StateResponse> CreateAsync(
        CreateStateRequest request,
        CancellationToken cancellationToken = default);

    Task<StateResponse?> UpdateAsync(
        int stateId,
        UpdateStateRequest request,
        CancellationToken cancellationToken = default);
}
