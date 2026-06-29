using ArunVastra.Application.DTOs.Users.Supplier;
using ArunVastra.Application.Interfaces;
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
                .Where(user => user.Agentname != null && (search == null || user.Agentname.StartsWith(search)))
                .Select(user => user.Agentname!),
            "city" => query
                .Where(user => user.Cityname != null && (search == null || user.Cityname.StartsWith(search)))
                .Select(user => user.Cityname!),
            _ => query
                .Where(user => search == null || user.Firstname.StartsWith(search))
                .Select(user => user.Firstname)
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
