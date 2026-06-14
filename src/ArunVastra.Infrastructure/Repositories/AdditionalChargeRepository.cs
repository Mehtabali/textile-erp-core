using ArunVastra.Application.DTOs.AdditionalCharges;
using ArunVastra.Application.Interfaces;
using ArunVastra.Infrastructure.Data;
using ArunVastra.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArunVastra.Infrastructure.Repositories;

public sealed class AdditionalChargeRepository(ArunVastraDbContext dbContext) : IAdditionalChargeRepository
{
    private readonly ArunVastraDbContext _dbContext = dbContext;

    public async Task<AdditionalChargeListResponse> ListAsync(
        AdditionalChargeListRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.AdditionalCharges
            .AsNoTracking()
            .Include(rule => rule.Product)
            .AsQueryable();

        query = ApplySearch(query, request.SearchKeyword);
        query = ApplyColumnFilters(query, request.Filters);
        query = ApplySort(query, request.Sort);

        var totalRecords = await query.CountAsync(cancellationToken);
        var skip = (request.PageNumber - 1) * request.PageSize;
        var rows = await query
            .Skip(skip)
            .Take(request.PageSize)
            .Select(rule => ToAdditionalChargeResponse(rule))
            .ToListAsync(cancellationToken);

        for (var index = 0; index < rows.Count; index++)
        {
            rows[index].ApplyOrder = skip + index + 1;
        }

        return new AdditionalChargeListResponse
        {
            Items = rows,
            TotalRecords = totalRecords,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<AdditionalChargeResponse?> GetByIdAsync(
        int AdditionalChargeId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.AdditionalCharges
            .AsNoTracking()
            .Include(rule => rule.Product)
            .Where(rule => rule.Id == AdditionalChargeId)
            .Select(rule => ToAdditionalChargeResponse(rule))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public Task<bool> CategoryExistsAsync(
        int categoryId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Products.AnyAsync(category => category.Prodid == categoryId, cancellationToken);
    }

    public async Task<IReadOnlyList<AdditionalChargeCategoryOptionResponse>> ListCategoryOptionsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .OrderBy(category => category.Prodname)
            .ThenBy(category => category.Prodid)
            .Select(category => new AdditionalChargeCategoryOptionResponse
            {
                CategoryId = category.Prodid,
                ProductName = category.Prodname
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<AdditionalChargeResponse> CreateAsync(
        CreateAdditionalChargeRequest request,
        CancellationToken cancellationToken = default)
    {
        var AdditionalCharge = new AdditionalCharge
        {
            Productid = request.StockGroupId,
            Gstvalue = request.GstValue,
            Startrange = request.StartRange!.Value,
            Endrange = request.EndRange
        };

        _dbContext.AdditionalCharges.Add(AdditionalCharge);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(AdditionalCharge.Id, cancellationToken) ?? ToAdditionalChargeResponse(AdditionalCharge);
    }

    public async Task<AdditionalChargeResponse?> UpdateAsync(
        int AdditionalChargeId,
        UpdateAdditionalChargeRequest request,
        CancellationToken cancellationToken = default)
    {
        var AdditionalCharge = await _dbContext.AdditionalCharges
            .SingleOrDefaultAsync(item => item.Id == AdditionalChargeId, cancellationToken);

        if (AdditionalCharge is null)
        {
            return null;
        }

        AdditionalCharge.Productid = request.StockGroupId;
        AdditionalCharge.Gstvalue = request.GstValue;
        AdditionalCharge.Startrange = request.StartRange!.Value;
        AdditionalCharge.Endrange = request.EndRange;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(AdditionalCharge.Id, cancellationToken);
    }

    private static IQueryable<AdditionalCharge> ApplySearch(IQueryable<AdditionalCharge> query, string? searchKeyword)
    {
        var search = NormalizeFilter(searchKeyword);
        if (search is null)
        {
            return query;
        }

        if (!decimal.TryParse(search, out var numericSearch))
        {
            return query.Where(rule => rule.Product.Prodname.Contains(search));
        }

        return query.Where(rule =>
            rule.Product.Prodname.Contains(search) ||
            rule.Gstvalue == numericSearch ||
            rule.Startrange == numericSearch ||
            rule.Endrange == numericSearch);
    }

    private static IQueryable<AdditionalCharge> ApplyColumnFilters(
        IQueryable<AdditionalCharge> query,
        AdditionalChargeListFiltersRequest filters)
    {
        if (NormalizeFilter(filters.StockGroupName) is { } stockGroupName)
        {
            query = query.Where(rule => rule.Product.Prodname.Contains(stockGroupName));
        }

        if (NormalizeFilter(filters.GstValue) is { } gstValue)
        {
            query = decimal.TryParse(gstValue, out var value)
                ? query.Where(rule => rule.Gstvalue == value)
                : query.Where(rule => false);
        }

        if (NormalizeFilter(filters.StartRange) is { } startRange)
        {
            query = decimal.TryParse(startRange, out var value)
                ? query.Where(rule => rule.Startrange == value)
                : query.Where(rule => false);
        }

        if (NormalizeFilter(filters.EndRange) is { } endRange)
        {
            query = decimal.TryParse(endRange, out var value)
                ? query.Where(rule => rule.Endrange == value)
                : query.Where(rule => false);
        }

        if (NormalizeFilter(filters.ApplyOrder) is { } applyOrder)
        {
            query = int.TryParse(applyOrder, out var value)
                ? query.Where(rule => rule.Id == value)
                : query.Where(rule => false);
        }

        return query;
    }

    private static IQueryable<AdditionalCharge> ApplySort(
        IQueryable<AdditionalCharge> query,
        AdditionalChargeListSortRequest? sort)
    {
        var field = sort?.Field?.Trim().ToLowerInvariant();
        var descending = string.Equals(sort?.Direction, "desc", StringComparison.OrdinalIgnoreCase);

        return field switch
        {
            "stockgroupname" or "stockgroup" or "name" => descending
                ? query.OrderByDescending(rule => rule.Product.Prodname).ThenByDescending(rule => rule.Id)
                : query.OrderBy(rule => rule.Product.Prodname).ThenBy(rule => rule.Id),
            "gstvalue" or "value" => descending
                ? query.OrderByDescending(rule => rule.Gstvalue).ThenByDescending(rule => rule.Id)
                : query.OrderBy(rule => rule.Gstvalue).ThenBy(rule => rule.Id),
            "startrange" or "start" => descending
                ? query.OrderByDescending(rule => rule.Startrange).ThenByDescending(rule => rule.Id)
                : query.OrderBy(rule => rule.Startrange).ThenBy(rule => rule.Id),
            "endrange" or "end" => descending
                ? query.OrderByDescending(rule => rule.Endrange).ThenByDescending(rule => rule.Id)
                : query.OrderBy(rule => rule.Endrange).ThenBy(rule => rule.Id),
            "applyorder" or "order" => descending
                ? query.OrderByDescending(rule => rule.Id)
                : query.OrderBy(rule => rule.Id),
            _ => query.OrderBy(rule => rule.Id)
        };
    }

    private static AdditionalChargeResponse ToAdditionalChargeResponse(AdditionalCharge rule)
    {
        return new AdditionalChargeResponse
        {
            Id = rule.Id,
            StockGroupId = rule.Productid,
            StockGroupName = rule.Product?.Prodname ?? string.Empty,
            GstValue = rule.Gstvalue ?? 0,
            StartRange = rule.Startrange,
            EndRange = rule.Endrange,
            ApplyOrder = rule.Id
        };
    }

    private static string? NormalizeFilter(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}

