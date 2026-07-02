namespace ArunVastra.Application.DTOs.SupplierCategoryMappings;

public sealed class SupplierCategoryMappingListResponse
{
    public IReadOnlyList<SupplierCategoryMappingResponse> Items { get; set; } = [];

    public int TotalRecords { get; set; }

    public int PageNumber { get; set; }

    public int PageSize { get; set; }
}

