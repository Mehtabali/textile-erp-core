using ArunVastra.Application.DTOs.Locations;
using ArunVastra.Application.Interfaces;
using ArunVastra.Infrastructure.Data;
using ArunVastra.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArunVastra.Infrastructure.Repositories;

public sealed class CityRepository(ArunVastraDbContext dbContext) : ICityRepository
{
    private readonly ArunVastraDbContext _dbContext = dbContext;

    public async Task<IReadOnlyList<CityResponse>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Cities
            .AsNoTracking()
            .OrderBy(city => city.Cityname)
            .ThenBy(city => city.Cityid)
            .Select(city => new CityResponse
            {
                CityId = city.Cityid,
                StateId = city.Stateid,
                CityName = city.Cityname
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CityResponse>> ListByStateAsync(
        int stateId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Cities
            .AsNoTracking()
            .Where(city => city.Stateid == stateId)
            .OrderBy(city => city.Cityname)
            .ThenBy(city => city.Cityid)
            .Select(city => new CityResponse
            {
                CityId = city.Cityid,
                StateId = city.Stateid,
                CityName = city.Cityname
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<CityResponse?> GetByIdAsync(
        int cityId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Cities
            .AsNoTracking()
            .Where(city => city.Cityid == cityId)
            .Select(city => ToCityResponse(city))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> StateExistsAsync(
        int stateId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.States.AnyAsync(
            state => state.Stateid == stateId,
            cancellationToken);
    }

    public async Task<bool> CityBelongsToStateAsync(
        int cityId,
        int stateId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Cities.AnyAsync(
            city => city.Cityid == cityId && city.Stateid == stateId,
            cancellationToken);
    }

    public async Task<bool> NameExistsInStateAsync(
        int stateId,
        string cityName,
        int? excludingCityId = null,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Cities.AnyAsync(
            city =>
                city.Stateid == stateId &&
                city.Cityname == cityName &&
                (!excludingCityId.HasValue || city.Cityid != excludingCityId.Value),
            cancellationToken);
    }

    public async Task<CityResponse> CreateAsync(
        CreateCityRequest request,
        CancellationToken cancellationToken = default)
    {
        var city = new City
        {
            Stateid = request.StateId,
            Cityname = request.CityName
        };

        _dbContext.Cities.Add(city);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToCityResponse(city);
    }

    public async Task<CityResponse?> UpdateAsync(
        int cityId,
        UpdateCityRequest request,
        CancellationToken cancellationToken = default)
    {
        var city = await _dbContext.Cities
            .SingleOrDefaultAsync(item => item.Cityid == cityId, cancellationToken);

        if (city is null)
        {
            return null;
        }

        city.Stateid = request.StateId;
        city.Cityname = request.CityName;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToCityResponse(city);
    }

    private static CityResponse ToCityResponse(City city)
    {
        return new CityResponse
        {
            CityId = city.Cityid,
            StateId = city.Stateid,
            CityName = city.Cityname
        };
    }
}
