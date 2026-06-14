using ArunVastra.Application.DTOs.Categories;
using ArunVastra.Application.Interfaces;
using ArunVastra.Infrastructure.Data;
using ArunVastra.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArunVastra.Infrastructure.Repositories;

public sealed class CategoryRepository(ArunVastraDbContext dbContext) : ICategoryRepository
{
    private readonly ArunVastraDbContext _dbContext = dbContext;

    public async Task<IReadOnlyList<CategoryResponse>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .OrderBy(category => category.Prodname)
            .ThenBy(category => category.Prodid)
            .Select(category => ToCategoryResponse(category))
            .ToListAsync(cancellationToken);
    }

    public async Task<CategoryResponse?> GetByIdAsync(
        int categoryId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Where(category => category.Prodid == categoryId)
            .Select(category => ToCategoryResponse(category))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> NameExistsAsync(
        string productName,
        int? excludingCategoryId = null,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products.AnyAsync(
            category =>
                category.Prodname == productName &&
                (!excludingCategoryId.HasValue || category.Prodid != excludingCategoryId.Value),
            cancellationToken);
    }

    public async Task<CategoryResponse> CreateAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = new Product
        {
            Prodname = request.ProductName!,
            Gstper = request.Gst,
            Gstpernew = request.Gst,
            Hsncode = request.HsnCode,
            Stitch = request.Stitch,
            Isactive = request.GstRule,
            Serial = request.AdditionalCharges ? 1 : 0
        };

        _dbContext.Products.Add(category);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToCategoryResponse(category);
    }

    public async Task<CategoryResponse?> UpdateAsync(
        int categoryId,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.Products
            .SingleOrDefaultAsync(item => item.Prodid == categoryId, cancellationToken);

        if (category is null)
        {
            return null;
        }

        category.Prodname = request.ProductName!;
        category.Gstper = request.Gst;
        category.Gstpernew = request.Gst;
        category.Hsncode = request.HsnCode;
        category.Stitch = request.Stitch;
        category.Isactive = request.GstRule;
        category.Serial = request.AdditionalCharges ? 1 : 0;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToCategoryResponse(category);
    }

    private static CategoryResponse ToCategoryResponse(Product category)
    {
        return new CategoryResponse
        {
            CategoryId = category.Prodid,
            ProductName = category.Prodname,
            Gst = category.Gstper ?? category.Gstpernew ?? 0,
            HsnCode = category.Hsncode,
            Stitch = category.Stitch ?? 0,
            GstRule = category.Isactive,
            AdditionalCharges = category.Serial == 1
        };
    }
}
