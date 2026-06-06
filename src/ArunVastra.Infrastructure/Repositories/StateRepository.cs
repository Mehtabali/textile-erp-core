using ArunVastra.Application.DTOs.Locations;
using ArunVastra.Application.Interfaces;
using ArunVastra.Infrastructure.Data;
using ArunVastra.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArunVastra.Infrastructure.Repositories;

public sealed class StateRepository(ArunVastraDbContext dbContext) : IStateRepository
{
    private readonly ArunVastraDbContext _dbContext = dbContext;

    public async Task<IReadOnlyList<StateResponse>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.States
            .AsNoTracking()
            .OrderBy(state => state.Statename)
            .ThenBy(state => state.Stateid)
            .Select(state => new StateResponse
            {
                StateId = state.Stateid,
                StateName = state.Statename,
                GstStateCode = state.Gststatecode
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<StateResponse?> GetByIdAsync(
        int stateId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.States
            .AsNoTracking()
            .Where(state => state.Stateid == stateId)
            .Select(state => ToStateResponse(state))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> NameExistsAsync(
        string stateName,
        int? excludingStateId = null,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.States.AnyAsync(
            state =>
                state.Statename == stateName &&
                (!excludingStateId.HasValue || state.Stateid != excludingStateId.Value),
            cancellationToken);
    }

    public async Task<StateResponse> CreateAsync(
        CreateStateRequest request,
        CancellationToken cancellationToken = default)
    {
        var state = new State
        {
            Statename = request.StateName,
            Gststatecode = request.GstStateCode
        };

        _dbContext.States.Add(state);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToStateResponse(state);
    }

    public async Task<StateResponse?> UpdateAsync(
        int stateId,
        UpdateStateRequest request,
        CancellationToken cancellationToken = default)
    {
        var state = await _dbContext.States
            .SingleOrDefaultAsync(item => item.Stateid == stateId, cancellationToken);

        if (state is null)
        {
            return null;
        }

        state.Statename = request.StateName;
        state.Gststatecode = request.GstStateCode;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToStateResponse(state);
    }

    private static StateResponse ToStateResponse(State state)
    {
        return new StateResponse
        {
            StateId = state.Stateid,
            StateName = state.Statename,
            GstStateCode = state.Gststatecode
        };
    }
}
