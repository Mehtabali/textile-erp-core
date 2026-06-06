using ArunVastra.Application.DTOs.Locations;
using ArunVastra.Application.Interfaces;

namespace ArunVastra.Application.Services;

public sealed class StateService(IStateRepository stateRepository) : IStateService
{
    private readonly IStateRepository _stateRepository = stateRepository;

    public Task<IReadOnlyList<StateResponse>> ListAsync(CancellationToken cancellationToken = default)
    {
        return _stateRepository.ListAsync(cancellationToken);
    }

    public Task<StateResponse?> GetByIdAsync(
        int stateId,
        CancellationToken cancellationToken = default)
    {
        ValidateId(stateId, "State id");

        return _stateRepository.GetByIdAsync(stateId, cancellationToken);
    }

    public async Task<StateResponse> CreateAsync(
        CreateStateRequest request,
        CancellationToken cancellationToken = default)
    {
        var stateName = NormalizeName(request.StateName, "State name");

        if (await _stateRepository.NameExistsAsync(stateName, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException("State name already exists.");
        }

        return await _stateRepository.CreateAsync(
            new CreateStateRequest
            {
                StateName = stateName,
                GstStateCode = request.GstStateCode
            },
            cancellationToken);
    }

    public async Task<StateResponse?> UpdateAsync(
        int stateId,
        UpdateStateRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateId(stateId, "State id");

        var stateName = NormalizeName(request.StateName, "State name");

        if (await _stateRepository.NameExistsAsync(stateName, stateId, cancellationToken))
        {
            throw new InvalidOperationException("State name already exists.");
        }

        return await _stateRepository.UpdateAsync(
            stateId,
            new UpdateStateRequest
            {
                StateName = stateName,
                GstStateCode = request.GstStateCode
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
