using ArunVastra.Application.DTOs.Categories;

namespace ArunVastra.Application.Interfaces;

public interface ICategoryRepository
{
    Task<IReadOnlyList<CategoryResponse>> ListAsync(CancellationToken cancellationToken = default);

    Task<CategoryResponse?> GetByIdAsync(int categoryId, CancellationToken cancellationToken = default);

    Task<bool> NameExistsAsync(
        string productName,
        int? excludingCategoryId = null,
        CancellationToken cancellationToken = default);

    Task<CategoryResponse> CreateAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default);

    Task<CategoryResponse?> UpdateAsync(
        int categoryId,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken = default);
}
