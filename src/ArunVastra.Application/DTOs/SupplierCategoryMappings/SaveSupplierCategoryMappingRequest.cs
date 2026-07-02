namespace ArunVastra.Application.DTOs.SupplierCategoryMappings;

public sealed class SaveSupplierCategoryMappingRequest
{
    public int SupplierUserId { get; set; }

    public IReadOnlyList<int> ProductCategoryIds { get; set; } = [];
}

