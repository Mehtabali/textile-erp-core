namespace ArunVastra.Application.DTOs.SupplierCategoryMappings;

public sealed class SupplierCategoryMappingResponse
{
    public int SupplierUserId { get; set; }

    public string SupplierCode { get; set; } = string.Empty;

    public string SupplierName { get; set; } = string.Empty;

    public IReadOnlyList<int> MappedProductCategoryIds { get; set; } = [];

    public IReadOnlyList<string> MappedProductCategoryNames { get; set; } = [];
}

