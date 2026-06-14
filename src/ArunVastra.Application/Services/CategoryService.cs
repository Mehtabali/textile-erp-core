using ArunVastra.Application.DTOs.Categories;
using ArunVastra.Application.Interfaces;

namespace ArunVastra.Application.Services;

public sealed class CategoryService(ICategoryRepository categoryRepository) : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository = categoryRepository;

    public Task<IReadOnlyList<CategoryResponse>> ListAsync(CancellationToken cancellationToken = default)
    {
        return _categoryRepository.ListAsync(cancellationToken);
    }

    public Task<CategoryResponse?> GetByIdAsync(
        int categoryId,
        CancellationToken cancellationToken = default)
    {
        ValidateId(categoryId, "Category id");

        return _categoryRepository.GetByIdAsync(categoryId, cancellationToken);
    }

    public async Task<CategoryResponse> CreateAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedRequest = await NormalizeCreateRequestAsync(request, cancellationToken);

        return await _categoryRepository.CreateAsync(normalizedRequest, cancellationToken);
    }

    public async Task<CategoryResponse?> UpdateAsync(
        int categoryId,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateId(categoryId, "Category id");

        var normalizedRequest = await NormalizeUpdateRequestAsync(categoryId, request, cancellationToken);

        return await _categoryRepository.UpdateAsync(categoryId, normalizedRequest, cancellationToken);
    }

    private async Task<CreateCategoryRequest> NormalizeCreateRequestAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var productName = NormalizeProductName(request.ProductName);

        if (await _categoryRepository.NameExistsAsync(productName, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException("Product category already exists.");
        }

        return new CreateCategoryRequest
        {
            ProductName = productName,
            Gst = NormalizeAmount(request.Gst, "GST"),
            HsnCode = NormalizeHsnCode(request.HsnCode),
            Stitch = NormalizeAmount(request.Stitch, "Stitch"),
            GstRule = request.GstRule,
            AdditionalCharges = request.AdditionalCharges
        };
    }

    private async Task<UpdateCategoryRequest> NormalizeUpdateRequestAsync(
        int categoryId,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var productName = NormalizeProductName(request.ProductName);

        if (await _categoryRepository.NameExistsAsync(productName, categoryId, cancellationToken))
        {
            throw new InvalidOperationException("Product category already exists.");
        }

        return new UpdateCategoryRequest
        {
            ProductName = productName,
            Gst = NormalizeAmount(request.Gst, "GST"),
            HsnCode = NormalizeHsnCode(request.HsnCode),
            Stitch = NormalizeAmount(request.Stitch, "Stitch"),
            GstRule = request.GstRule,
            AdditionalCharges = request.AdditionalCharges
        };
    }

    private static void ValidateId(int id, string name)
    {
        if (id <= 0)
        {
            throw new InvalidOperationException($"{name} must be greater than zero.");
        }
    }

    private static string NormalizeProductName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException("Product name is required.");
        }

        var normalized = value.Trim();
        if (normalized.Length > 100)
        {
            throw new InvalidOperationException("Product name cannot exceed 100 characters.");
        }

        return normalized;
    }

    private static string? NormalizeHsnCode(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > 25)
        {
            throw new InvalidOperationException("HSN code cannot exceed 25 characters.");
        }

        return normalized;
    }

    private static decimal NormalizeAmount(decimal? value, string name)
    {
        var normalized = value ?? 0;
        if (normalized < 0)
        {
            throw new InvalidOperationException($"{name} cannot be negative.");
        }

        return normalized;
    }
}
