using System.Linq.Expressions;
using ArunVastra.Application.DTOs.SupplierTransportMappings;
using ArunVastra.Application.Interfaces;
using ArunVastra.Domain.Enums;
using ArunVastra.Infrastructure.Data;
using ArunVastra.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArunVastra.Infrastructure.Repositories;

public sealed class SupplierTransportMappingRepository(ArunVastraDbContext dbContext)
    : ISupplierTransportMappingRepository
{
    private const int SupplierRole = (int)UserRole.Supplier;
    private const int TransportRole = (int)UserRole.Transport;

    private readonly ArunVastraDbContext _dbContext = dbContext;

    public async Task<SupplierTransportMappingListResponse> ListAsync(
        SupplierTransportMappingListRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.SupplierTransportMappings
            .AsNoTracking()
            .Include(mapping => mapping.Supplier)
            .Include(mapping => mapping.Transport)
            .AsQueryable();

        query = ApplySearch(query, request.SearchKeyword);
        query = ApplyColumnFilters(query, request.Filters);

        var supplierQuery = query
            .GroupBy(mapping => new
            {
                mapping.Supplieruserid,
                mapping.Supplier.Usercode,
                mapping.Supplier.Firstname
            })
            .Select(group => new SupplierTransportMappingSupplierRow
            {
                SupplierUserId = group.Key.Supplieruserid,
                SupplierCode = group.Key.Usercode ?? string.Empty,
                SupplierName = group.Key.Firstname,
                FirstTransportName = group.Min(mapping => mapping.Transport.Firstname)
            });

        supplierQuery = ApplySort(supplierQuery, request.Sort);

        var totalRecords = await supplierQuery.CountAsync(cancellationToken);
        var skip = (request.PageNumber - 1) * request.PageSize;
        var suppliers = await supplierQuery
            .Skip(skip)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var supplierUserIds = suppliers.Select(supplier => supplier.SupplierUserId).ToArray();
        var mappingsQuery = _dbContext.SupplierTransportMappings.AsNoTracking();

        mappingsQuery = supplierUserIds.Length == 0
            ? mappingsQuery.Where(mapping => false)
            : mappingsQuery.Where(BuildSupplierUserIdPredicate(supplierUserIds));

        var mappings = await mappingsQuery
            .OrderBy(mapping => mapping.Transport.Firstname)
            .ThenBy(mapping => mapping.Transportid)
            .Select(mapping => new
            {
                mapping.Supplieruserid,
                mapping.Transportid,
                mapping.Transport.Firstname
            })
            .ToListAsync(cancellationToken);

        var mappingLookup = mappings
            .GroupBy(mapping => mapping.Supplieruserid)
            .ToDictionary(group => group.Key, group => group.ToList());

        return new SupplierTransportMappingListResponse
        {
            Items = suppliers.Select(supplier =>
            {
                var supplierMappings = mappingLookup.TryGetValue(supplier.SupplierUserId, out var items)
                    ? items
                    : [];

                return new SupplierTransportMappingResponse
                {
                    SupplierUserId = supplier.SupplierUserId,
                    SupplierCode = supplier.SupplierCode,
                    SupplierName = supplier.SupplierName,
                    MappedTransportIds = supplierMappings.Select(mapping => mapping.Transportid).ToList(),
                    MappedTransportNames = supplierMappings.Select(mapping => mapping.Firstname).ToList()
                };
            }).ToList(),
            TotalRecords = totalRecords,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<SupplierTransportMappingResponse?> GetBySupplierUserIdAsync(
        int supplierUserId,
        CancellationToken cancellationToken = default)
    {
        var supplier = await _dbContext.Users
            .AsNoTracking()
            .Where(user => user.Userid == supplierUserId && user.Role == SupplierRole)
            .Select(user => new
            {
                SupplierUserId = user.Userid,
                SupplierCode = user.Usercode ?? string.Empty,
                SupplierName = user.Firstname
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (supplier is null)
        {
            return null;
        }

        var transports = await _dbContext.SupplierTransportMappings
            .AsNoTracking()
            .Where(mapping => mapping.Supplieruserid == supplierUserId)
            .OrderBy(mapping => mapping.Transport.Firstname)
            .ThenBy(mapping => mapping.Transportid)
            .Select(mapping => new
            {
                mapping.Transportid,
                mapping.Transport.Firstname
            })
            .ToListAsync(cancellationToken);

        return new SupplierTransportMappingResponse
        {
            SupplierUserId = supplier.SupplierUserId,
            SupplierCode = supplier.SupplierCode,
            SupplierName = supplier.SupplierName,
            MappedTransportIds = transports.Select(transport => transport.Transportid).ToList(),
            MappedTransportNames = transports.Select(transport => transport.Firstname).ToList()
        };
    }

    public async Task<IReadOnlyList<SupplierTransportMappingOptionResponse>> SearchSuppliersAsync(
        string? searchKeyword,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.UserViews
            .AsNoTracking()
            .Where(user => user.Role == SupplierRole);

        if (NormalizeFilter(searchKeyword) is { } search)
        {
            query = query.Where(user =>
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

        return await query
            .OrderBy(user => user.Firstname)
            .ThenBy(user => user.Userid)
            .Take(take)
            .Select(user => new SupplierTransportMappingOptionResponse
            {
                Id = user.Userid,
                Code = user.Usercode,
                Name = user.Firstname
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SupplierTransportMappingOptionResponse>> SearchTransportsAsync(
        string? searchKeyword,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Users
            .AsNoTracking()
            .Where(user => user.Role == TransportRole);

        if (NormalizeFilter(searchKeyword) is { } search)
        {
            query = query.Where(user =>
                user.Firstname.Contains(search) ||
                user.Email.Contains(search) ||
                (user.Mobile != null && user.Mobile.Contains(search)) ||
                (user.Phone != null && user.Phone.Contains(search)));
        }

        return await query
            .OrderBy(user => user.Firstname)
            .ThenBy(user => user.Userid)
            .Take(take)
            .Select(user => new SupplierTransportMappingOptionResponse
            {
                Id = user.Userid,
                Name = user.Firstname
            })
            .ToListAsync(cancellationToken);
    }

    public Task<bool> SupplierExistsAsync(
        int supplierUserId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Users.AnyAsync(
            user => user.Userid == supplierUserId && user.Role == SupplierRole,
            cancellationToken);
    }

    public async Task<IReadOnlySet<int>> GetValidTransportIdsAsync(
        IReadOnlyCollection<int> transportIds,
        CancellationToken cancellationToken = default)
    {
        if (transportIds.Count == 0)
        {
            return new HashSet<int>();
        }

        var validTransportIds = await _dbContext.Users
            .AsNoTracking()
            .Where(BuildUserIdPredicate(transportIds))
            .Where(user => user.Role == TransportRole)
            .Select(user => user.Userid)
            .ToListAsync(cancellationToken);

        return validTransportIds.ToHashSet();
    }

    public async Task<SupplierTransportMappingResponse> SaveAsync(
        SaveSupplierTransportMappingRequest request,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var requestedTransportIds = request.TransportIds.ToHashSet();
        var existingMappings = await _dbContext.SupplierTransportMappings
            .Where(mapping => mapping.Supplieruserid == request.SupplierUserId)
            .ToListAsync(cancellationToken);

        var mappingsToRemove = existingMappings
            .Where(mapping => !requestedTransportIds.Contains(mapping.Transportid))
            .ToList();

        if (mappingsToRemove.Count > 0)
        {
            _dbContext.SupplierTransportMappings.RemoveRange(mappingsToRemove);
        }

        var existingTransportIds = existingMappings
            .Select(mapping => mapping.Transportid)
            .ToHashSet();

        foreach (var transportId in requestedTransportIds.Where(id => !existingTransportIds.Contains(id)))
        {
            _dbContext.SupplierTransportMappings.Add(new SupplierTransportMapping
            {
                Supplieruserid = request.SupplierUserId,
                Transportid = transportId
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return (await GetBySupplierUserIdAsync(request.SupplierUserId, cancellationToken))!;
    }

    public async Task<bool> RemoveAsync(
        int supplierUserId,
        int transportUserId,
        CancellationToken cancellationToken = default)
    {
        var mapping = await _dbContext.SupplierTransportMappings
            .SingleOrDefaultAsync(
                item => item.Supplieruserid == supplierUserId && item.Transportid == transportUserId,
                cancellationToken);

        if (mapping is null)
        {
            return false;
        }

        _dbContext.SupplierTransportMappings.Remove(mapping);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static IQueryable<SupplierTransportMapping> ApplySearch(
        IQueryable<SupplierTransportMapping> query,
        string? searchKeyword)
    {
        if (NormalizeFilter(searchKeyword) is not { } search)
        {
            return query;
        }

        return query.Where(mapping =>
            (mapping.Supplier.Usercode != null && mapping.Supplier.Usercode.Contains(search)) ||
            mapping.Supplier.Firstname.Contains(search) ||
            mapping.Transport.Firstname.Contains(search) ||
            mapping.Transport.Email.Contains(search));
    }

    private static IQueryable<SupplierTransportMapping> ApplyColumnFilters(
        IQueryable<SupplierTransportMapping> query,
        SupplierTransportMappingListFiltersRequest filters)
    {
        if (NormalizeFilter(filters.SupplierCode) is { } supplierCode)
        {
            query = query.Where(mapping =>
                mapping.Supplier.Usercode != null &&
                mapping.Supplier.Usercode.Contains(supplierCode));
        }

        if (NormalizeFilter(filters.SupplierName) is { } supplierName)
        {
            query = query.Where(mapping => mapping.Supplier.Firstname.Contains(supplierName));
        }

        if (NormalizeFilter(filters.TransportName) is { } transportName)
        {
            query = query.Where(mapping => mapping.Transport.Firstname.Contains(transportName));
        }

        return query;
    }

    private static IQueryable<SupplierTransportMappingSupplierRow> ApplySort(
        IQueryable<SupplierTransportMappingSupplierRow> query,
        SupplierTransportMappingListSortRequest? sort)
    {
        var descending = string.Equals(sort?.Direction, "desc", StringComparison.OrdinalIgnoreCase);

        return NormalizeFilter(sort?.Field)?.ToLowerInvariant() switch
        {
            "suppliercode" or "code" => descending
                ? query.OrderByDescending(row => row.SupplierCode).ThenByDescending(row => row.SupplierUserId)
                : query.OrderBy(row => row.SupplierCode).ThenBy(row => row.SupplierUserId),
            "transportname" or "transport" => descending
                ? query.OrderByDescending(row => row.FirstTransportName).ThenByDescending(row => row.SupplierUserId)
                : query.OrderBy(row => row.FirstTransportName).ThenBy(row => row.SupplierUserId),
            _ => descending
                ? query.OrderByDescending(row => row.SupplierName).ThenByDescending(row => row.SupplierUserId)
                : query.OrderBy(row => row.SupplierName).ThenBy(row => row.SupplierUserId)
        };
    }

    private static string? NormalizeFilter(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static Expression<Func<SupplierTransportMapping, bool>> BuildSupplierUserIdPredicate(
        IReadOnlyCollection<int> supplierUserIds)
    {
        var parameter = Expression.Parameter(typeof(SupplierTransportMapping), "mapping");
        var property = Expression.Property(parameter, nameof(SupplierTransportMapping.Supplieruserid));
        var body = supplierUserIds
            .Select(id => Expression.Equal(property, Expression.Constant(id)))
            .Aggregate<Expression>((left, right) => Expression.OrElse(left, right));

        return Expression.Lambda<Func<SupplierTransportMapping, bool>>(body, parameter);
    }

    private static Expression<Func<User, bool>> BuildUserIdPredicate(IReadOnlyCollection<int> userIds)
    {
        var parameter = Expression.Parameter(typeof(User), "user");
        var property = Expression.Property(parameter, nameof(User.Userid));
        var body = userIds
            .Select(id => Expression.Equal(property, Expression.Constant(id)))
            .Aggregate<Expression>((left, right) => Expression.OrElse(left, right));

        return Expression.Lambda<Func<User, bool>>(body, parameter);
    }

    private sealed class SupplierTransportMappingSupplierRow
    {
        public int SupplierUserId { get; set; }

        public string SupplierCode { get; set; } = string.Empty;

        public string SupplierName { get; set; } = string.Empty;

        public string? FirstTransportName { get; set; }
    }
}
