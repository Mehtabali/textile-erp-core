using System.Linq.Expressions;
using ArunVastra.Application.DTOs.SupplierCategoryMappings;
using ArunVastra.Application.Interfaces;
using ArunVastra.Domain.Enums;
using ArunVastra.Infrastructure.Data;
using ArunVastra.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArunVastra.Infrastructure.Repositories;

public sealed class SupplierCategoryMappingRepository(ArunVastraDbContext dbContext)
    : ISupplierCategoryMappingRepository
{
    private const int SupplierRole = (int)UserRole.Supplier;

    private readonly ArunVastraDbContext _dbContext = dbContext;

    public async Task<SupplierCategoryMappingListResponse> ListAsync(
        SupplierCategoryMappingListRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.SupplierCategoryMappings
            .AsNoTracking()
            .Include(mapping => mapping.Supplier)
            .Include(mapping => mapping.ProductCategory)
            .AsQueryable();

        query = ApplySearch(query, request.SearchKeyword);
        query = ApplyColumnFilters(query, request.Filters);

        var supplierQuery = query
            .GroupBy(mapping => new
            {
                mapping.Userid,
                mapping.Supplier.Usercode,
                mapping.Supplier.Firstname
            })
            .Select(group => new SupplierCategoryMappingSupplierRow
            {
                SupplierUserId = group.Key.Userid,
                SupplierCode = group.Key.Usercode ?? string.Empty,
                SupplierName = group.Key.Firstname,
                FirstProductCategoryName = group.Min(mapping => mapping.ProductCategory.Prodname)
            });

        supplierQuery = ApplySort(supplierQuery, request.Sort);

        var totalRecords = await supplierQuery.CountAsync(cancellationToken);
        var skip = (request.PageNumber - 1) * request.PageSize;
        var suppliers = await supplierQuery
            .Skip(skip)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var supplierUserIds = suppliers.Select(supplier => supplier.SupplierUserId).ToArray();
        var mappingsQuery = _dbContext.SupplierCategoryMappings.AsNoTracking();

        mappingsQuery = supplierUserIds.Length == 0
            ? mappingsQuery.Where(mapping => false)
            : mappingsQuery.Where(BuildSupplierUserIdPredicate(supplierUserIds));

        var mappings = await mappingsQuery
            .OrderBy(mapping => mapping.ProductCategory.Prodname)
            .ThenBy(mapping => mapping.Prodid)
            .Select(mapping => new
            {
                mapping.Userid,
                mapping.Prodid,
                mapping.ProductCategory.Prodname
            })
            .ToListAsync(cancellationToken);

        var mappingLookup = mappings
            .GroupBy(mapping => mapping.Userid)
            .ToDictionary(group => group.Key, group => group.ToList());

        return new SupplierCategoryMappingListResponse
        {
            Items = suppliers.Select(supplier =>
            {
                var supplierMappings = mappingLookup.TryGetValue(supplier.SupplierUserId, out var items)
                    ? items
                    : [];

                return new SupplierCategoryMappingResponse
                {
                    SupplierUserId = supplier.SupplierUserId,
                    SupplierCode = supplier.SupplierCode,
                    SupplierName = supplier.SupplierName,
                    MappedProductCategoryIds = supplierMappings.Select(mapping => mapping.Prodid).ToList(),
                    MappedProductCategoryNames = supplierMappings.Select(mapping => mapping.Prodname).ToList()
                };
            }).ToList(),
            TotalRecords = totalRecords,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<SupplierCategoryMappingResponse?> GetBySupplierUserIdAsync(
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

        var productCategories = await _dbContext.SupplierCategoryMappings
            .AsNoTracking()
            .Where(mapping => mapping.Userid == supplierUserId)
            .OrderBy(mapping => mapping.ProductCategory.Prodname)
            .ThenBy(mapping => mapping.Prodid)
            .Select(mapping => new
            {
                mapping.Prodid,
                mapping.ProductCategory.Prodname
            })
            .ToListAsync(cancellationToken);

        return new SupplierCategoryMappingResponse
        {
            SupplierUserId = supplier.SupplierUserId,
            SupplierCode = supplier.SupplierCode,
            SupplierName = supplier.SupplierName,
            MappedProductCategoryIds = productCategories.Select(category => category.Prodid).ToList(),
            MappedProductCategoryNames = productCategories.Select(category => category.Prodname).ToList()
        };
    }

    public async Task<IReadOnlyList<SupplierCategoryMappingOptionResponse>> SearchSuppliersAsync(
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
            .Select(user => new SupplierCategoryMappingOptionResponse
            {
                Id = user.Userid,
                Code = user.Usercode,
                Name = user.Firstname
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SupplierCategoryMappingOptionResponse>> SearchProductCategoriesAsync(
        string? searchKeyword,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Products.AsNoTracking();

        if (NormalizeFilter(searchKeyword) is { } search)
        {
            query = query.Where(category =>
                category.Prodname.Contains(search) ||
                (category.Hsncode != null && category.Hsncode.Contains(search)));
        }

        return await query
            .OrderBy(category => category.Prodname)
            .ThenBy(category => category.Prodid)
            .Take(take)
            .Select(category => new SupplierCategoryMappingOptionResponse
            {
                Id = category.Prodid,
                Name = category.Prodname
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

    public async Task<IReadOnlySet<int>> GetValidProductCategoryIdsAsync(
        IReadOnlyCollection<int> productCategoryIds,
        CancellationToken cancellationToken = default)
    {
        if (productCategoryIds.Count == 0)
        {
            return new HashSet<int>();
        }

        var validProductCategoryIds = await _dbContext.Products
            .AsNoTracking()
            .Where(BuildProductCategoryIdPredicate(productCategoryIds))
            .Select(category => category.Prodid)
            .ToListAsync(cancellationToken);

        return validProductCategoryIds.ToHashSet();
    }

    public async Task<SupplierCategoryMappingResponse> SaveAsync(
        SaveSupplierCategoryMappingRequest request,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var requestedProductCategoryIds = request.ProductCategoryIds.ToHashSet();
        var existingMappings = await _dbContext.SupplierCategoryMappings
            .Where(mapping => mapping.Userid == request.SupplierUserId)
            .ToListAsync(cancellationToken);

        var mappingsToRemove = existingMappings
            .Where(mapping => !requestedProductCategoryIds.Contains(mapping.Prodid))
            .ToList();

        if (mappingsToRemove.Count > 0)
        {
            _dbContext.SupplierCategoryMappings.RemoveRange(mappingsToRemove);
        }

        var existingProductCategoryIds = existingMappings
            .Select(mapping => mapping.Prodid)
            .ToHashSet();

        foreach (var productCategoryId in requestedProductCategoryIds.Where(id => !existingProductCategoryIds.Contains(id)))
        {
            _dbContext.SupplierCategoryMappings.Add(new SupplierCategoryMapping
            {
                Userid = request.SupplierUserId,
                Prodid = productCategoryId
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return (await GetBySupplierUserIdAsync(request.SupplierUserId, cancellationToken))!;
    }

    public async Task<bool> RemoveAsync(
        int supplierUserId,
        int productCategoryId,
        CancellationToken cancellationToken = default)
    {
        var mapping = await _dbContext.SupplierCategoryMappings
            .SingleOrDefaultAsync(
                item => item.Userid == supplierUserId && item.Prodid == productCategoryId,
                cancellationToken);

        if (mapping is null)
        {
            return false;
        }

        _dbContext.SupplierCategoryMappings.Remove(mapping);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static IQueryable<SupplierCategoryMapping> ApplySearch(
        IQueryable<SupplierCategoryMapping> query,
        string? searchKeyword)
    {
        if (NormalizeFilter(searchKeyword) is not { } search)
        {
            return query;
        }

        return query.Where(mapping =>
            (mapping.Supplier.Usercode != null && mapping.Supplier.Usercode.Contains(search)) ||
            mapping.Supplier.Firstname.Contains(search) ||
            mapping.ProductCategory.Prodname.Contains(search) ||
            (mapping.ProductCategory.Hsncode != null && mapping.ProductCategory.Hsncode.Contains(search)));
    }

    private static IQueryable<SupplierCategoryMapping> ApplyColumnFilters(
        IQueryable<SupplierCategoryMapping> query,
        SupplierCategoryMappingListFiltersRequest filters)
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

        if (NormalizeFilter(filters.ProductCategoryName) is { } productCategoryName)
        {
            query = query.Where(mapping => mapping.ProductCategory.Prodname.Contains(productCategoryName));
        }

        return query;
    }

    private static IQueryable<SupplierCategoryMappingSupplierRow> ApplySort(
        IQueryable<SupplierCategoryMappingSupplierRow> query,
        SupplierCategoryMappingListSortRequest? sort)
    {
        var descending = string.Equals(sort?.Direction, "desc", StringComparison.OrdinalIgnoreCase);

        return NormalizeFilter(sort?.Field)?.ToLowerInvariant() switch
        {
            "suppliercode" or "code" => descending
                ? query.OrderByDescending(row => row.SupplierCode).ThenByDescending(row => row.SupplierUserId)
                : query.OrderBy(row => row.SupplierCode).ThenBy(row => row.SupplierUserId),
            "productcategoryname" or "productcategory" or "category" => descending
                ? query.OrderByDescending(row => row.FirstProductCategoryName).ThenByDescending(row => row.SupplierUserId)
                : query.OrderBy(row => row.FirstProductCategoryName).ThenBy(row => row.SupplierUserId),
            _ => descending
                ? query.OrderByDescending(row => row.SupplierName).ThenByDescending(row => row.SupplierUserId)
                : query.OrderBy(row => row.SupplierName).ThenBy(row => row.SupplierUserId)
        };
    }

    private static string? NormalizeFilter(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static Expression<Func<SupplierCategoryMapping, bool>> BuildSupplierUserIdPredicate(
        IReadOnlyCollection<int> supplierUserIds)
    {
        var parameter = Expression.Parameter(typeof(SupplierCategoryMapping), "mapping");
        var property = Expression.Property(parameter, nameof(SupplierCategoryMapping.Userid));
        var body = supplierUserIds
            .Select(id => Expression.Equal(property, Expression.Constant(id)))
            .Aggregate<Expression>((left, right) => Expression.OrElse(left, right));

        return Expression.Lambda<Func<SupplierCategoryMapping, bool>>(body, parameter);
    }

    private static Expression<Func<Product, bool>> BuildProductCategoryIdPredicate(
        IReadOnlyCollection<int> productCategoryIds)
    {
        var parameter = Expression.Parameter(typeof(Product), "category");
        var property = Expression.Property(parameter, nameof(Product.Prodid));
        var body = productCategoryIds
            .Select(id => Expression.Equal(property, Expression.Constant(id)))
            .Aggregate<Expression>((left, right) => Expression.OrElse(left, right));

        return Expression.Lambda<Func<Product, bool>>(body, parameter);
    }

    private sealed class SupplierCategoryMappingSupplierRow
    {
        public int SupplierUserId { get; set; }

        public string SupplierCode { get; set; } = string.Empty;

        public string SupplierName { get; set; } = string.Empty;

        public string? FirstProductCategoryName { get; set; }
    }
}
