using ArunVastra.Application.DTOs.Users.Internal;
using ArunVastra.Application.Interfaces;
using ArunVastra.Application.Models.Users;
using ArunVastra.Domain.Enums;
using ArunVastra.Infrastructure.Data;
using ArunVastra.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArunVastra.Infrastructure.Repositories;

public sealed class InternalUserRepository : IInternalUserRepository
{
    private readonly ArunVastraDbContext _dbContext;

    public InternalUserRepository(ArunVastraDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<InternalUserListResponse> ListAsync(
        InternalUserListRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Users
            .AsNoTracking()
            .Where(user =>
                user.Role == (int)UserRole.Admin ||
                user.Role == (int)UserRole.FloorManager);

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
                (user.Brandname != null && user.Brandname.Contains(normalizedSearch)));
        }

        query = ApplyColumnFilters(query, request);
        query = ApplySort(query, request);

        var totalRecords = await query.CountAsync(cancellationToken);
        var skip = (request.PageNumber - 1) * request.PageSize;

        var items = await query
            .Skip(skip)
            .Take(request.PageSize)
            .Select(user => new InternalUserListItemResponse
            {
                UserId = user.Userid,
                Name = user.Firstname,
                Email = user.Email,
                Role = user.Role,
                Phone = user.Phone,
                Mobile = user.Mobile,
                Gstin = user.Gstin,
                BrandName = user.Brandname,
                Remarks = user.Description,
                Status = !user.Locked,
                LastLoginAt = user.Lastloginat
            })
            .ToListAsync(cancellationToken);

        return new InternalUserListResponse
        {
            Items = items,
            TotalRecords = totalRecords,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    private static IQueryable<User> ApplyColumnFilters(
        IQueryable<User> query,
        InternalUserListRequest request)
    {
        var filters = request.Filters ?? new InternalUserListFiltersRequest();

        if (NormalizeFilter(filters.Name) is { } name)
        {
            query = query.Where(user => user.Firstname.Contains(name));
        }

        if (NormalizeFilter(filters.Email) is { } email)
        {
            query = query.Where(user => user.Email.Contains(email));
        }

        if (NormalizeFilter(filters.Type) is { } type)
        {
            query = ApplyTypeFilter(query, type);
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

    private static string? NormalizeFilter(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static IQueryable<User> ApplySort(
        IQueryable<User> query,
        InternalUserListRequest request)
    {
        var descending = string.Equals(request.Sort?.Direction, "desc", StringComparison.OrdinalIgnoreCase);

        return NormalizeSortField(request.Sort?.Field) switch
        {
            "email" => descending
                ? query.OrderByDescending(user => user.Email).ThenBy(user => user.Userid)
                : query.OrderBy(user => user.Email).ThenBy(user => user.Userid),
            "type" => descending
                ? query.OrderByDescending(user => user.Role).ThenBy(user => user.Userid)
                : query.OrderBy(user => user.Role).ThenBy(user => user.Userid),
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
            "status" => descending
                ? query.OrderByDescending(user => !user.Locked).ThenBy(user => user.Userid)
                : query.OrderBy(user => !user.Locked).ThenBy(user => user.Userid),
            _ => descending
                ? query.OrderByDescending(user => user.Firstname).ThenBy(user => user.Userid)
                : query.OrderBy(user => user.Firstname).ThenBy(user => user.Userid)
        };
    }

    private static string NormalizeSortField(string? sortField)
    {
        return string.IsNullOrWhiteSpace(sortField)
            ? "name"
            : sortField.Trim();
    }

    public async Task<InternalUserResponse?> GetByIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Where(user =>
                user.Userid == userId &&
                (user.Role == (int)UserRole.Admin ||
                 user.Role == (int)UserRole.FloorManager))
            .Select(user => ToInternalUserResponse(user))
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

    public async Task<InternalUserResponse> CreateAsync(
        InternalUserCreateModel model,
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
            Pwhash = string.Empty,
            Passwordhash = model.PasswordHash,
            Passwordmigrated = true,
            Passwordresetrequired = false,
            Role = model.Role,
            Profit = 0,
            Locked = !model.Status,
            Description = model.Remarks,
            Created = now,
            Updatedat = now,
            Btuser = false,
            Isgstupdate = "N",
            Extracharges = 0
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToInternalUserResponse(user);
    }

    public async Task<InternalUserResponse?> UpdateAsync(
        int userId,
        InternalUserUpdateModel model,
        CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .SingleOrDefaultAsync(
                item =>
                    item.Userid == userId &&
                    (item.Role == (int)UserRole.Admin ||
                     item.Role == (int)UserRole.FloorManager),
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
        user.Description = model.Remarks;
        user.Role = model.Role;
        user.Locked = !model.Status;
        user.Updatedat = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToInternalUserResponse(user);
    }

    private static InternalUserResponse ToInternalUserResponse(User user)
    {
        return new InternalUserResponse
        {
            UserId = user.Userid,
            Name = user.Firstname,
            Email = user.Email,
            Phone = user.Phone,
            Mobile = user.Mobile,
            Gstin = user.Gstin,
            BrandName = user.Brandname,
            Remarks = user.Description,
            Status = !user.Locked,
            Role = user.Role,
            CreatedAt = user.Created,
            UpdatedAt = user.Updatedat,
            LastLoginAt = user.Lastloginat
        };
    }

    private static bool IsInternalUserRole(int role)
    {
        return role is (int)UserRole.Admin or (int)UserRole.FloorManager;
    }

    private static IQueryable<User> ApplyTypeFilter(IQueryable<User> query, string type)
    {
        if (int.TryParse(type, out var role) && IsInternalUserRole(role))
        {
            return query.Where(user => user.Role == role);
        }

        var normalizedType = type.Trim().ToLowerInvariant();
        if ("admin".Contains(normalizedType, StringComparison.Ordinal))
        {
            return query.Where(user => user.Role == (int)UserRole.Admin);
        }

        if ("floor manager".Contains(normalizedType, StringComparison.Ordinal) ||
            "floor".Contains(normalizedType, StringComparison.Ordinal))
        {
            return query.Where(user => user.Role == (int)UserRole.FloorManager);
        }

        return query.Where(user => false);
    }
}
