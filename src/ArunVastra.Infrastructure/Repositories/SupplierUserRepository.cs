using ArunVastra.Application.DTOs.Users.Supplier;
using ArunVastra.Application.Interfaces;
using ArunVastra.Application.Models.Users;
using ArunVastra.Domain.Enums;
using ArunVastra.Infrastructure.Data;
using ArunVastra.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArunVastra.Infrastructure.Repositories;

public sealed class SupplierUserRepository : ISupplierUserRepository
{
    private readonly ArunVastraDbContext _dbContext;

    public SupplierUserRepository(ArunVastraDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SupplierUserListResponse> ListAsync(
        SupplierUserListRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.UserViews
            .AsNoTracking()
            .Where(user => user.Role == (int)UserRole.Supplier);

        if (!request.IncludeLocked)
        {
            query = query.Where(user => !user.Locked);
        }

        query = ApplySearch(query, request.SearchKeyword);
        query = ApplyColumnFilters(query, request);
        query = ApplySort(query, request);

        var totalRecords = await query.CountAsync(cancellationToken);
        var skip = (request.PageNumber - 1) * request.PageSize;

        var users = await query
            .Skip(skip)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new SupplierUserListResponse
        {
            Items = users.Select(ToListItem).ToList(),
            TotalRecords = totalRecords,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<SupplierUserAutocompleteResponse> GetAutocompleteValuesAsync(
        SupplierUserAutocompleteRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.UserViews
            .AsNoTracking()
            .Where(user => user.Role == (int)UserRole.Supplier);

        if (!request.IncludeLocked)
        {
            query = query.Where(user => !user.Locked);
        }

        var search = NormalizeFilter(request.SearchKeyword);
        var valuesQuery = NormalizeSortField(request.Field) switch
        {
            "agent" => query
                .Where(user => user.Agentname != null && (search == null || user.Agentname.Contains(search)))
                .Select(user => user.Agentname!),
            "city" => query
                .Where(user => user.Cityname != null && (search == null || user.Cityname.Contains(search)))
                .Select(user => user.Cityname!),
            _ => query
                .Where(user =>
                    search == null ||
                    user.Firstname.Contains(search) ||
                    (user.Usercode != null && user.Usercode.Contains(search)))
                .Select(user => user.Usercode == null || user.Usercode == string.Empty
                    ? user.Firstname
                    : user.Firstname + " (" + user.Usercode + ")")
        };

        var values = await valuesQuery
            .Where(value => value != string.Empty)
            .Distinct()
            .OrderBy(value => value)
            .Take(request.MaxResults)
            .ToListAsync(cancellationToken);

        return new SupplierUserAutocompleteResponse
        {
            Values = values
        };
    }

    public async Task<SupplierUserResponse?> GetByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var supplier = await _dbContext.Users
            .AsNoTracking()
            .Where(user => user.Userid == userId && user.Role == (int)UserRole.Supplier)
            .Select(user => new SupplierUserResponse
            {
                UserId = user.Userid,
                Code = user.Usercode,
                Name = user.Firstname,
                Email = user.Email,
                Password = user.Pwhash,
                Phone = user.Phone,
                Mobile = user.Mobile,
                Gstin = user.Gstin,
                BrandName = user.Brandname,
                AgencyId = user.Agentid,
                AgencyName = user.Agentname,
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
                Address = user.Useraddress,
                DharaProfit = user.Profit,
                ExtraCharges = user.Extracharges,
                Discount = user.Discount,
                TransportId = _dbContext.SupplierTransportMappings
                    .Where(mapping => mapping.Supplieruserid == user.Userid)
                    .OrderBy(mapping => mapping.Id)
                    .Select(mapping => (int?)mapping.Transportid)
                    .FirstOrDefault(),
                TransportName = _dbContext.SupplierTransportMappings
                    .Where(mapping => mapping.Supplieruserid == user.Userid)
                    .OrderBy(mapping => mapping.Id)
                    .Select(mapping => mapping.Transport.Firstname)
                    .FirstOrDefault(),
                ProductCategoryId = _dbContext.SupplierCategoryMappings
                    .Where(mapping => mapping.Userid == user.Userid)
                    .OrderBy(mapping => mapping.Prodid)
                    .Select(mapping => (int?)mapping.Prodid)
                    .FirstOrDefault(),
                ProductCategoryName = _dbContext.SupplierCategoryMappings
                    .Where(mapping => mapping.Userid == user.Userid)
                    .OrderBy(mapping => mapping.Prodid)
                    .Select(mapping => mapping.ProductCategory.Prodname)
                    .FirstOrDefault(),
                Remarks = user.Description,
                Locked = user.Locked
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (supplier is null)
        {
            return null;
        }

        supplier.Transports = await _dbContext.SupplierTransportMappings
            .AsNoTracking()
            .Where(mapping => mapping.Supplieruserid == userId)
            .OrderBy(mapping => mapping.Transport.Firstname)
            .Select(mapping => new SupplierOptionResponse
            {
                Id = mapping.Transportid,
                Name = mapping.Transport.Firstname
            })
            .ToListAsync(cancellationToken);

        supplier.TransportId = supplier.Transports.FirstOrDefault()?.Id;
        supplier.TransportName = supplier.Transports.FirstOrDefault()?.Name;

        return supplier;
    }

    public async Task<string> GetNextUserCodeAsync(CancellationToken cancellationToken = default)
    {
        var userCodes = await _dbContext.UserViews
            .AsNoTracking()
            .Where(user => user.Usercode != null && user.Usercode != string.Empty)
            .Select(user => user.Usercode!)
            .ToListAsync(cancellationToken);

        var maxCode = userCodes
            .Select(code => int.TryParse(code, out var numericCode) ? numericCode : 0)
            .DefaultIfEmpty(0)
            .Max();

        return (maxCode + 1).ToString("0000");
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludingUserId = null, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users.AnyAsync(
            user => user.Email == email && (!excludingUserId.HasValue || user.Userid != excludingUserId.Value),
            cancellationToken);
    }

    public async Task<SupplierUserResponse> CreateAsync(SupplierUserCreateModel model, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var user = new User
        {
            Usercode = model.UserCode,
            Firstname = model.Name,
            Email = model.Email,
            Phone = model.Phone,
            Mobile = model.Mobile,
            Gstin = model.Gstin,
            Brandname = model.BrandName,
            Agentid = model.AgencyId,
            Agentname = model.AgencyName,
            Cityid = model.CityId,
            Useraddress = model.Address,
            Profit = model.DharaProfit,
            Extracharges = model.ExtraCharges ?? 0,
            Discount = model.Discount,
            Description = model.Remarks,
            Pwhash = model.LegacyPassword,
            Passwordhash = model.PasswordHash,
            Passwordmigrated = true,
            Passwordresetrequired = false,
            Role = (int)UserRole.Supplier,
            Locked = false,
            Btuser = false,
            Isgstupdate = "N",
            Created = now,
            Updatedat = now
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(user.Userid, cancellationToken) ?? ToResponse(user);
    }

    public async Task<SupplierUserResponse?> UpdateAsync(int userId, SupplierUserUpdateModel model, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .SingleOrDefaultAsync(item => item.Userid == userId && item.Role == (int)UserRole.Supplier, cancellationToken);

        if (user is null)
        {
            return null;
        }

        user.Usercode = model.UserCode;
        user.Firstname = model.Name;
        user.Email = model.Email;
        user.Phone = model.Phone;
        user.Mobile = model.Mobile;
        user.Gstin = model.Gstin;
        user.Brandname = model.BrandName;
        user.Agentid = model.AgencyId;
        user.Agentname = model.AgencyName;
        user.Cityid = model.CityId;
        user.Useraddress = model.Address;
        user.Profit = model.DharaProfit;
        user.Extracharges = model.ExtraCharges ?? 0;
        user.Discount = model.Discount;
        user.Description = model.Remarks;
        user.Updatedat = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(user.Userid, cancellationToken) ?? ToResponse(user);
    }

    public async Task<bool> DeleteAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .SingleOrDefaultAsync(item => item.Userid == userId && item.Role == (int)UserRole.Supplier, cancellationToken);

        if (user is null)
        {
            return false;
        }

        var transportMappings = await _dbContext.SupplierTransportMappings
            .Where(mapping => mapping.Supplieruserid == userId)
            .ToListAsync(cancellationToken);
        var categoryMappings = await _dbContext.SupplierCategoryMappings
            .Where(mapping => mapping.Userid == userId)
            .ToListAsync(cancellationToken);

        _dbContext.SupplierTransportMappings.RemoveRange(transportMappings);
        _dbContext.SupplierCategoryMappings.RemoveRange(categoryMappings);
        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> LockAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .SingleOrDefaultAsync(item => item.Userid == userId && item.Role == (int)UserRole.Supplier, cancellationToken);

        if (user is null)
        {
            return false;
        }

        user.Locked = !user.Locked;
        user.Updatedat = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> ResetPasswordAsync(int userId, string legacyPassword, string passwordHash, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .SingleOrDefaultAsync(item => item.Userid == userId && item.Role == (int)UserRole.Supplier, cancellationToken);

        if (user is null)
        {
            return false;
        }

        user.Pwhash = legacyPassword;
        user.Passwordhash = passwordHash;
        user.Passwordmigrated = true;
        user.Passwordresetrequired = false;
        user.Updatedat = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<IReadOnlyList<SupplierOptionResponse>> GetAgencyOptionsAsync(string? searchKeyword, int take, CancellationToken cancellationToken = default)
    {
        var search = NormalizeFilter(searchKeyword);
        var query = _dbContext.Users
            .AsNoTracking()
            .Where(user => user.Role == (int)UserRole.Agency);

        if (search is not null)
        {
            query = query.Where(user => user.Firstname.Contains(search) || (user.Usercode != null && user.Usercode.Contains(search)));
        }

        return await query
            .OrderBy(user => user.Firstname)
            .Take(take)
            .Select(user => new SupplierOptionResponse
            {
                Id = user.Userid,
                Name = user.Firstname
            })
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<UserView> ApplySearch(IQueryable<UserView> query, string? searchKeyword)
    {
        if (NormalizeFilter(searchKeyword) is not { } search)
        {
            return query;
        }

        return query.Where(user =>
            (user.Usercode != null && user.Usercode.Contains(search)) ||
            user.Firstname.Contains(search) ||
            user.Email.Contains(search) ||
            (user.Brandname != null && user.Brandname.Contains(search)) ||
            (user.Gstin != null && user.Gstin.Contains(search)) ||
            (user.Phone != null && user.Phone.Contains(search)) ||
            (user.Mobile != null && user.Mobile.Contains(search)) ||
            (user.Cityname != null && user.Cityname.Contains(search)) ||
            (user.Agentname != null && user.Agentname.Contains(search)));
    }

    private static IQueryable<UserView> ApplyColumnFilters(
        IQueryable<UserView> query,
        SupplierUserListRequest request)
    {
        var filters = request.Filters ?? new SupplierUserListFiltersRequest();

        if (NormalizeFilter(filters.Code) is { } code)
        {
            query = query.Where(user => user.Usercode != null && user.Usercode.Contains(code));
        }

        if (NormalizeFilter(filters.Name) is { } name)
        {
            query = query.Where(user => user.Firstname.Contains(name));
        }

        if (NormalizeFilter(filters.Brand) is { } brand)
        {
            query = query.Where(user => user.Brandname != null && user.Brandname.Contains(brand));
        }

        if (NormalizeFilter(filters.Dhara) is { } dhara)
        {
            query = query.Where(user => user.Profit != null && user.Profit.ToString()!.Contains(dhara));
        }

        if (NormalizeFilter(filters.Gstin) is { } gstin)
        {
            query = query.Where(user => user.Gstin != null && user.Gstin.Contains(gstin));
        }

        if (NormalizeFilter(filters.Phone) is { } phone)
        {
            query = query.Where(user => user.Phone != null && user.Phone.Contains(phone));
        }

        if (NormalizeFilter(filters.Mobile) is { } mobile)
        {
            query = query.Where(user => user.Mobile != null && user.Mobile.Contains(mobile));
        }

        if (NormalizeFilter(filters.City) is { } city)
        {
            query = query.Where(user => user.Cityname != null && user.Cityname.Contains(city));
        }

        if (NormalizeFilter(filters.Email) is { } email)
        {
            query = query.Where(user => user.Email.Contains(email));
        }

        if (NormalizeFilter(filters.Password) is { } password)
        {
            query = query.Where(user => user.Pwhash != null && user.Pwhash.Contains(password));
        }

        if (NormalizeFilter(filters.Agent) is { } agent)
        {
            query = query.Where(user => user.Agentname != null && user.Agentname.Contains(agent));
        }

        if (NormalizeFilter(filters.Status) is { } status)
        {
            query = ApplyStatusFilter(query, status);
        }

        return query;
    }

    private static IQueryable<UserView> ApplyStatusFilter(IQueryable<UserView> query, string status)
    {
        var normalizedStatus = status.Trim().ToLowerInvariant();

        if ("locked".StartsWith(normalizedStatus, StringComparison.Ordinal))
        {
            return query.Where(user => user.Locked);
        }

        if ("active".StartsWith(normalizedStatus, StringComparison.Ordinal))
        {
            var activeAfter = DateTime.UtcNow.AddSeconds(-300);
            return query.Where(user => !user.Locked && user.Lastaccess != null && user.Lastaccess >= activeAfter);
        }

        if ("idle".StartsWith(normalizedStatus, StringComparison.Ordinal))
        {
            var idleBefore = DateTime.UtcNow.AddSeconds(-300);
            return query.Where(user => !user.Locked && user.Lastaccess != null && user.Lastaccess < idleBefore);
        }

        return query.Where(user => false);
    }

    private static IQueryable<UserView> ApplySort(
        IQueryable<UserView> query,
        SupplierUserListRequest request)
    {
        var descending = string.Equals(request.Sort?.Direction, "desc", StringComparison.OrdinalIgnoreCase);

        return NormalizeSortField(request.Sort?.Field) switch
        {
            "code" => descending ? query.OrderByDescending(user => user.Usercode).ThenBy(user => user.Userid) : query.OrderBy(user => user.Usercode).ThenBy(user => user.Userid),
            "brand" => descending ? query.OrderByDescending(user => user.Brandname).ThenBy(user => user.Userid) : query.OrderBy(user => user.Brandname).ThenBy(user => user.Userid),
            "dhara" => descending ? query.OrderByDescending(user => user.Profit).ThenBy(user => user.Userid) : query.OrderBy(user => user.Profit).ThenBy(user => user.Userid),
            "gstin" => descending ? query.OrderByDescending(user => user.Gstin).ThenBy(user => user.Userid) : query.OrderBy(user => user.Gstin).ThenBy(user => user.Userid),
            "phone" => descending ? query.OrderByDescending(user => user.Phone).ThenBy(user => user.Userid) : query.OrderBy(user => user.Phone).ThenBy(user => user.Userid),
            "mobile" => descending ? query.OrderByDescending(user => user.Mobile).ThenBy(user => user.Userid) : query.OrderBy(user => user.Mobile).ThenBy(user => user.Userid),
            "city" => descending ? query.OrderByDescending(user => user.Cityname).ThenBy(user => user.Userid) : query.OrderBy(user => user.Cityname).ThenBy(user => user.Userid),
            "email" => descending ? query.OrderByDescending(user => user.Email).ThenBy(user => user.Userid) : query.OrderBy(user => user.Email).ThenBy(user => user.Userid),
            "password" => descending ? query.OrderByDescending(user => user.Pwhash).ThenBy(user => user.Userid) : query.OrderBy(user => user.Pwhash).ThenBy(user => user.Userid),
            "agent" => descending ? query.OrderByDescending(user => user.Agentname).ThenBy(user => user.Userid) : query.OrderBy(user => user.Agentname).ThenBy(user => user.Userid),
            "status" => descending ? query.OrderByDescending(user => user.Locked).ThenByDescending(user => user.Lastaccess).ThenBy(user => user.Userid) : query.OrderBy(user => user.Locked).ThenByDescending(user => user.Lastaccess).ThenBy(user => user.Userid),
            _ => descending ? query.OrderByDescending(user => user.Firstname).ThenBy(user => user.Userid) : query.OrderBy(user => user.Firstname).ThenBy(user => user.Userid)
        };
    }

    private static string? NormalizeFilter(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string NormalizeSortField(string? sortField)
    {
        return string.IsNullOrWhiteSpace(sortField)
            ? "code"
            : sortField.Trim().ToLowerInvariant();
    }

    private static SupplierUserListItemResponse ToListItem(UserView user)
    {
        return new SupplierUserListItemResponse
        {
            UserId = user.Userid,
            Code = user.Usercode ?? string.Empty,
            Name = user.Firstname,
            Brand = user.Brandname,
            Dhara = user.Profit,
            Gstin = user.Gstin,
            Phone = user.Phone,
            Mobile = user.Mobile,
            City = user.Cityname,
            Email = user.Email,
            Password = user.Pwhash,
            Agent = user.Agentname,
            Status = GetStatus(user)
        };
    }

    private static SupplierUserResponse ToResponse(User user)
    {
        return new SupplierUserResponse
        {
            UserId = user.Userid,
            Code = user.Usercode,
            Name = user.Firstname,
            Email = user.Email,
            Password = user.Pwhash,
            Phone = user.Phone,
            Mobile = user.Mobile,
            Gstin = user.Gstin,
            BrandName = user.Brandname,
            AgencyId = user.Agentid,
            AgencyName = user.Agentname,
            CityId = user.Cityid,
            Address = user.Useraddress,
            DharaProfit = user.Profit,
            ExtraCharges = user.Extracharges,
            Discount = user.Discount,
            Remarks = user.Description,
            Locked = user.Locked
        };
    }

    private static string GetStatus(UserView user)
    {
        if (user.Locked)
        {
            return "Locked";
        }

        if (!user.Lastaccess.HasValue)
        {
            return string.Empty;
        }

        return DateTime.UtcNow.Subtract(user.Lastaccess.Value).TotalSeconds < 300
            ? "Active"
            : "Idle";
    }
}
