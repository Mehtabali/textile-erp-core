using ArunVastra.Application.DTOs.Products;
using ArunVastra.Application.Interfaces;
using ArunVastra.Infrastructure.Data;
using ArunVastra.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArunVastra.Infrastructure.Repositories;

public sealed class ProductListingRepository(ArunVastraDbContext dbContext) : IProductListingRepository
{
    private readonly ArunVastraDbContext _dbContext = dbContext;

    public async Task<ProductListResponse> ListAsync(
        ProductListRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.SupItemViews
            .AsNoTracking()
            .Where(product => product.Isactive);

        query = ApplySearch(query, request.SearchKeyword);
        query = ApplyColumnFilters(query, request.Filters);
        query = ApplySort(query, request.Sort);

        var totalRecords = await query.CountAsync(cancellationToken);
        var skip = (request.PageNumber - 1) * request.PageSize;
        var products = await query
            .Skip(skip)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new ProductListResponse
        {
            Items = products.Select(ToListItem).ToList(),
            TotalRecords = totalRecords,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<ProductAutocompleteResponse> GetAutocompleteValuesAsync(
        ProductAutocompleteRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.SupItemViews
            .AsNoTracking()
            .Where(product => product.Isactive);

        var search = NormalizeFilter(request.SearchKeyword);
        var field = NormalizeSortField(request.Field);

        if (field == "salerate")
        {
            var rateQuery = query.Where(product => product.Mrpnew != null);

            if (TryNormalizeDecimal(search) is { } saleRate)
            {
                rateQuery = rateQuery.Where(product => product.Mrpnew == saleRate);
            }

            var rateValues = await rateQuery
                .Select(product => product.Mrpnew!.Value)
                .Distinct()
                .OrderBy(value => value)
                .Take(request.MaxResults)
                .ToListAsync(cancellationToken);

            return new ProductAutocompleteResponse
            {
                Values = rateValues.Select(value => value.ToString("0.##")).ToList()
            };
        }

        var valuesQuery = field switch
        {
            "barcode" => query
                .Where(product => product.Barcode != null && (search == null || product.Barcode.Contains(search)))
                .Select(product => product.Barcode!),
            "company" => query
                .Where(product => product.Compname != null && (search == null || product.Compname.Contains(search)))
                .Select(product => product.Compname!),
            _ => query
                .Where(product => product.Description != null && (search == null || product.Description.Contains(search)))
                .Select(product => product.Description!)
        };

        var values = await valuesQuery
            .Where(value => value != string.Empty)
            .Distinct()
            .OrderBy(value => value)
            .Take(request.MaxResults)
            .ToListAsync(cancellationToken);

        return new ProductAutocompleteResponse { Values = values };
    }

    private static IQueryable<SupItemView> ApplySearch(IQueryable<SupItemView> query, string? searchKeyword)
    {
        if (NormalizeFilter(searchKeyword) is not { } search)
        {
            return query;
        }

        return query.Where(product =>
            (product.Prodname != null && product.Prodname.Contains(search)) ||
            (product.Description != null && product.Description.Contains(search)) ||
            (product.Barcode != null && product.Barcode.Contains(search)) ||
            (product.Hsncode != null && product.Hsncode.Contains(search)) ||
            (product.Compname != null && product.Compname.Contains(search)));
    }

    private static IQueryable<SupItemView> ApplyColumnFilters(
        IQueryable<SupItemView> query,
        ProductListFiltersRequest filters)
    {
        if (NormalizeFilter(filters.ProductName) is { } productName)
        {
            query = query.Where(product => product.Prodname != null && product.Prodname.Contains(productName));
        }

        if (NormalizeFilter(filters.Description) is { } description)
        {
            query = query.Where(product => product.Description != null && product.Description.Contains(description));
        }

        if (NormalizeFilter(filters.Barcode) is { } barcode)
        {
            query = query.Where(product => product.Barcode != null && product.Barcode.Contains(barcode));
        }

        if (NormalizeFilter(filters.Hsn) is { } hsn)
        {
            query = query.Where(product => product.Hsncode != null && product.Hsncode.Contains(hsn));
        }

        if (TryNormalizeDecimal(filters.Purchase) is { } purchase)
        {
            query = query.Where(product => product.Purchase == purchase);
        }

        if (TryNormalizeDecimal(filters.Mrp) is { } mrp)
        {
            query = query.Where(product => product.Mrpnew == mrp);
        }

        if (TryNormalizeDecimal(filters.SaleRate) is { } saleRate)
        {
            query = query.Where(product => product.Mrpnew == saleRate);
        }

        if (NormalizeFilter(filters.Company) is { } company)
        {
            query = query.Where(product => product.Compname != null && product.Compname.Contains(company));
        }

        if (NormalizeFilter(filters.Formula) is { } formula)
        {
            var normalizedFormula = formula.ToLowerInvariant();

            if ("mu".Contains(normalizedFormula, StringComparison.OrdinalIgnoreCase))
            {
                return query;
            }
            if ("% (percentage)".Contains(normalizedFormula, StringComparison.OrdinalIgnoreCase) ||
                "percentage".Contains(normalizedFormula, StringComparison.OrdinalIgnoreCase) ||
                normalizedFormula.Contains('%'))
            {
                return query.Where(product => false);
            }

            return query.Where(product => false);
        }

        return query;
    }

    private static IQueryable<SupItemView> ApplySort(
        IQueryable<SupItemView> query,
        ProductListSortRequest? sort)
    {
        var descending = string.Equals(sort?.Direction, "desc", StringComparison.OrdinalIgnoreCase);

        return NormalizeSortField(sort?.Field) switch
        {
            "productname" => descending ? query.OrderByDescending(product => product.Prodname).ThenBy(product => product.Supprodid) : query.OrderBy(product => product.Prodname).ThenBy(product => product.Supprodid),
            "barcode" => descending ? query.OrderByDescending(product => product.Barcode).ThenBy(product => product.Supprodid) : query.OrderBy(product => product.Barcode).ThenBy(product => product.Supprodid),
            "hsn" => descending ? query.OrderByDescending(product => product.Hsncode).ThenBy(product => product.Supprodid) : query.OrderBy(product => product.Hsncode).ThenBy(product => product.Supprodid),
            "purchase" => descending ? query.OrderByDescending(product => product.Purchase).ThenBy(product => product.Supprodid) : query.OrderBy(product => product.Purchase).ThenBy(product => product.Supprodid),
            "mrp" or "salerate" => descending ? query.OrderByDescending(product => product.Mrpnew).ThenBy(product => product.Supprodid) : query.OrderBy(product => product.Mrpnew).ThenBy(product => product.Supprodid),
            "company" => descending ? query.OrderByDescending(product => product.Compname).ThenBy(product => product.Supprodid) : query.OrderBy(product => product.Compname).ThenBy(product => product.Supprodid),
            "formula" => descending ? query.OrderByDescending(product => product.Formula).ThenBy(product => product.Supprodid) : query.OrderBy(product => product.Formula).ThenBy(product => product.Supprodid),
            _ => descending ? query.OrderByDescending(product => product.Description).ThenBy(product => product.Supprodid) : query.OrderBy(product => product.Description).ThenBy(product => product.Supprodid)
        };
    }

    private static ProductListItemResponse ToListItem(SupItemView product)
    {
        return new ProductListItemResponse
        {
            Id = product.Supprodid,
            ProductName = product.Prodname ?? string.Empty,
            Description = product.Description ?? string.Empty,
            Barcode = product.Barcode ?? string.Empty,
            Hsn = product.Hsncode ?? string.Empty,
            Purchase = product.Purchase,
            Mrp = product.Mrpnew ?? product.Mrp,
            Company = product.Compname ?? string.Empty,
            Formula = MapFormula(product.Formula),
            SaleRate = product.Mrpnew ?? product.Mrp
        };
    }

    private static string MapFormula(bool? formula)
    {
        // Legacy UI renders Formula as: Formula == 2 ? "% (PERCENTAGE)" : "MU".
        // SUPITEMVIEW exposes FORMULA as bit, so both 0 and 1 map to MU in the legacy screen.
        return "MU";
    }

    private static string? NormalizeFilter(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string NormalizeSortField(string? sortField)
    {
        return string.IsNullOrWhiteSpace(sortField)
            ? "description"
            : sortField.Trim().Replace("-", string.Empty, StringComparison.Ordinal).ToLowerInvariant();
    }

    private static decimal? TryNormalizeDecimal(string? value)
    {
        return decimal.TryParse(value?.Trim(), out var parsedValue) ? parsedValue : null;
    }
}
