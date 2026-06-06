using ArunVastra.Application.DTOs.Users.Transport;
using ArunVastra.Application.Interfaces;
using ArunVastra.Application.Models.Users;
using ArunVastra.Domain.Enums;
using ArunVastra.Infrastructure.Data;
using ArunVastra.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArunVastra.Infrastructure.Repositories;

public sealed class TransportUserRepository : ITransportUserRepository
{
    private const int TransportRole = (int)UserRole.Transport;

    private readonly ArunVastraDbContext _dbContext;

    public TransportUserRepository(ArunVastraDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TransportUserListResponse> ListAsync(
        TransportUserListRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Users
            .AsNoTracking()
            .Where(user => user.Role == TransportRole);

        if (!request.IncludeLocked)
        {
            query = query.Where(user => !user.Locked);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchKeyword))
        {
            var normalizedSearch = request.SearchKeyword.Trim();

            query = query.Where(user =>
                user.Firstname.Contains(normalizedSearch) ||
                user.Email.Contains(normalizedSearch) ||
                (user.Phone != null && user.Phone.Contains(normalizedSearch)) ||
                (user.Mobile != null && user.Mobile.Contains(normalizedSearch)) ||
                (user.Gstin != null && user.Gstin.Contains(normalizedSearch)) ||
                (user.Brandname != null && user.Brandname.Contains(normalizedSearch)) ||
                (user.Cityid != null && _dbContext.Cities.Any(city =>
                    city.Cityid == user.Cityid &&
                    ((city.Cityname != null && city.Cityname.Contains(normalizedSearch)) ||
                     (city.State != null && city.State.Statename != null && city.State.Statename.Contains(normalizedSearch))))));
        }

        query = ApplyColumnFilters(query, request);
        query = ApplySort(query, request);

        var totalRecords = await query.CountAsync(cancellationToken);
        var skip = (request.PageNumber - 1) * request.PageSize;

        var items = await query
            .Skip(skip)
            .Take(request.PageSize)
            .Select(user => new TransportUserListItemResponse
            {
                UserId = user.Userid,
                Name = user.Firstname,
                Email = user.Email,
                Role = user.Role,
                Phone = user.Phone,
                Mobile = user.Mobile,
                Gstin = user.Gstin,
                BrandName = user.Brandname,
                StateId = _dbContext.Cities
                    .Where(city => city.Cityid == user.Cityid)
                    .Select(city => city.Stateid)
                    .FirstOrDefault(),
                StateName = _dbContext.Cities
                    .Where(city => city.Cityid == user.Cityid)
                    .Select(city => city.State != null ? city.State.Statename : null)
                    .FirstOrDefault(),
                CityId = user.Cityid,
                CityName = _dbContext.Cities
                    .Where(city => city.Cityid == user.Cityid)
                    .Select(city => city.Cityname)
                    .FirstOrDefault(),
                Remarks = user.Description,
                Status = !user.Locked,
                LastLoginAt = user.Lastloginat
            })
            .ToListAsync(cancellationToken);

        return new TransportUserListResponse
        {
            Items = items,
            TotalRecords = totalRecords,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<TransportUserResponse?> GetByIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Where(user => user.Userid == userId && user.Role == TransportRole)
            .Select(user => new TransportUserResponse
            {
                UserId = user.Userid,
                Name = user.Firstname,
                Email = user.Email,
                Phone = user.Phone,
                Mobile = user.Mobile,
                Gstin = user.Gstin,
                BrandName = user.Brandname,
                Address = user.Useraddress,
                StateId = _dbContext.Cities
                    .Where(city => city.Cityid == user.Cityid)
                    .Select(city => city.Stateid)
                    .FirstOrDefault(),
                StateName = _dbContext.Cities
                    .Where(city => city.Cityid == user.Cityid)
                    .Select(city => city.State != null ? city.State.Statename : null)
                    .FirstOrDefault(),
                CityId = user.Cityid,
                CityName = _dbContext.Cities
                    .Where(city => city.Cityid == user.Cityid)
                    .Select(city => city.Cityname)
                    .FirstOrDefault(),
                Remarks = user.Description,
                Status = !user.Locked,
                Role = user.Role,
                CreatedAt = user.Created,
                UpdatedAt = user.Updatedat,
                LastLoginAt = user.Lastloginat
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(
        string email,
        int? excludingUserId = null,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users.AnyAsync(
            user =>
                user.Email == email &&
                (!excludingUserId.HasValue || user.Userid != excludingUserId.Value),
            cancellationToken);
    }

    public async Task<TransportUserResponse> CreateAsync(
        TransportUserCreateModel model,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var user = new User
        {
            Firstname = model.Name,
            Email = model.Email,
            Phone = model.Phone,
            Mobile = model.Mobile,
            Gstin = model.Gstin,
            Brandname = model.BrandName,
            Useraddress = model.Address,
            Cityid = model.CityId,
            Pwhash = string.Empty,
            Passwordhash = model.PasswordHash,
            Passwordmigrated = true,
            Passwordresetrequired = false,
            Role = TransportRole,
            Profit = 0,
            Locked = true,
            Description = model.Remarks,
            Created = now,
            Updatedat = now,
            Btuser = false,
            Isgstupdate = "N",
            Extracharges = 0
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(user.Userid, cancellationToken) ?? ToTransportUserResponse(user);
    }

    public async Task<TransportUserResponse?> UpdateAsync(
        int userId,
        TransportUserUpdateModel model,
        CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .SingleOrDefaultAsync(
                item => item.Userid == userId && item.Role == TransportRole,
                cancellationToken);

        if (user is null)
        {
            return null;
        }

        user.Firstname = model.Name;
        user.Email = model.Email;
        user.Phone = model.Phone;
        user.Mobile = model.Mobile;
        user.Gstin = model.Gstin;
        user.Brandname = model.BrandName;
        user.Useraddress = model.Address;
        user.Cityid = model.CityId;
        user.Description = model.Remarks;
        user.Role = TransportRole;
        user.Locked = !model.Status;
        user.Updatedat = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(user.Userid, cancellationToken) ?? ToTransportUserResponse(user);
    }

    private IQueryable<User> ApplyColumnFilters(
        IQueryable<User> query,
        TransportUserListRequest request)
    {
        var filters = request.Filters ?? new TransportUserListFiltersRequest();

        if (NormalizeFilter(filters.Name) is { } name)
        {
            query = query.Where(user => user.Firstname.Contains(name));
        }

        if (NormalizeFilter(filters.Email) is { } email)
        {
            query = query.Where(user => user.Email.Contains(email));
        }

        if (NormalizeFilter(filters.Phone) is { } phone)
        {
            query = query.Where(user => user.Phone != null && user.Phone.Contains(phone));
        }

        if (NormalizeFilter(filters.Mobile) is { } mobile)
        {
            query = query.Where(user => user.Mobile != null && user.Mobile.Contains(mobile));
        }

        if (NormalizeFilter(filters.Gstin) is { } gstin)
        {
            query = query.Where(user => user.Gstin != null && user.Gstin.Contains(gstin));
        }

        if (NormalizeFilter(filters.BrandName) is { } brandName)
        {
            query = query.Where(user => user.Brandname != null && user.Brandname.Contains(brandName));
        }

        if (NormalizeFilter(filters.StateName) is { } stateName)
        {
            query = query.Where(user => user.Cityid != null && _dbContext.Cities.Any(city =>
                city.Cityid == user.Cityid &&
                city.State != null &&
                city.State.Statename != null &&
                city.State.Statename.Contains(stateName)));
        }

        if (NormalizeFilter(filters.CityName) is { } cityName)
        {
            query = query.Where(user => user.Cityid != null && _dbContext.Cities.Any(city =>
                city.Cityid == user.Cityid &&
                city.Cityname != null &&
                city.Cityname.Contains(cityName)));
        }

        if (!string.IsNullOrWhiteSpace(filters.Status))
        {
            var normalizedStatus = filters.Status.Trim().ToLowerInvariant();
            if ("inactive".StartsWith(normalizedStatus, StringComparison.Ordinal))
            {
                query = query.Where(user => user.Locked);
            }
            else if ("active".StartsWith(normalizedStatus, StringComparison.Ordinal))
            {
                query = query.Where(user => !user.Locked);
            }
        }

        return query;
    }

    private IQueryable<User> ApplySort(
        IQueryable<User> query,
        TransportUserListRequest request)
    {
        var descending = string.Equals(request.Sort?.Direction, "desc", StringComparison.OrdinalIgnoreCase);

        return NormalizeSortField(request.Sort?.Field) switch
        {
            "email" => descending
                ? query.OrderByDescending(user => user.Email).ThenBy(user => user.Userid)
                : query.OrderBy(user => user.Email).ThenBy(user => user.Userid),
            "phone" => descending
                ? query.OrderByDescending(user => user.Phone).ThenBy(user => user.Userid)
                : query.OrderBy(user => user.Phone).ThenBy(user => user.Userid),
            "mobile" => descending
                ? query.OrderByDescending(user => user.Mobile).ThenBy(user => user.Userid)
                : query.OrderBy(user => user.Mobile).ThenBy(user => user.Userid),
            "gstin" => descending
                ? query.OrderByDescending(user => user.Gstin).ThenBy(user => user.Userid)
                : query.OrderBy(user => user.Gstin).ThenBy(user => user.Userid),
            "brandName" => descending
                ? query.OrderByDescending(user => user.Brandname).ThenBy(user => user.Userid)
                : query.OrderBy(user => user.Brandname).ThenBy(user => user.Userid),
            "stateName" => descending
                ? query.OrderByDescending(user => _dbContext.Cities
                    .Where(city => city.Cityid == user.Cityid)
                    .Select(city => city.State != null ? city.State.Statename : null)
                    .FirstOrDefault()).ThenBy(user => user.Userid)
                : query.OrderBy(user => _dbContext.Cities
                    .Where(city => city.Cityid == user.Cityid)
                    .Select(city => city.State != null ? city.State.Statename : null)
                    .FirstOrDefault()).ThenBy(user => user.Userid),
            "cityName" => descending
                ? query.OrderByDescending(user => _dbContext.Cities
                    .Where(city => city.Cityid == user.Cityid)
                    .Select(city => city.Cityname)
                    .FirstOrDefault()).ThenBy(user => user.Userid)
                : query.OrderBy(user => _dbContext.Cities
                    .Where(city => city.Cityid == user.Cityid)
                    .Select(city => city.Cityname)
                    .FirstOrDefault()).ThenBy(user => user.Userid),
            "status" => descending
                ? query.OrderByDescending(user => !user.Locked).ThenBy(user => user.Userid)
                : query.OrderBy(user => !user.Locked).ThenBy(user => user.Userid),
            _ => descending
                ? query.OrderByDescending(user => user.Firstname).ThenBy(user => user.Userid)
                : query.OrderBy(user => user.Firstname).ThenBy(user => user.Userid)
        };
    }

    private static TransportUserResponse ToTransportUserResponse(User user)
    {
        return new TransportUserResponse
        {
            UserId = user.Userid,
            Name = user.Firstname,
            Email = user.Email,
            Phone = user.Phone,
            Mobile = user.Mobile,
            Gstin = user.Gstin,
            BrandName = user.Brandname,
            Address = user.Useraddress,
            StateId = null,
            StateName = null,
            CityId = user.Cityid,
            CityName = null,
            Remarks = user.Description,
            Status = !user.Locked,
            Role = user.Role,
            CreatedAt = user.Created,
            UpdatedAt = user.Updatedat,
            LastLoginAt = user.Lastloginat
        };
    }

    private static string? NormalizeFilter(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string NormalizeSortField(string? sortField)
    {
        return string.IsNullOrWhiteSpace(sortField)
            ? "name"
            : sortField.Trim();
    }
}
