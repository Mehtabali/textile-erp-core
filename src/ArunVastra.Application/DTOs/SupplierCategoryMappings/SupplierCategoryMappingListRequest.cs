namespace ArunVastra.Application.DTOs.SupplierCategoryMappings;

public sealed class SupplierCategoryMappingListRequest
{
    public string? SearchKeyword { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public SupplierCategoryMappingListFiltersRequest Filters { get; set; } = new();

    public SupplierCategoryMappingListSortRequest? Sort { get; set; }
}

public sealed class SupplierCategoryMappingListFiltersRequest
{
    public string? SupplierCode { get; set; }

    public string? SupplierName { get; set; }

    public string? ProductCategoryName { get; set; }
}

public sealed class SupplierCategoryMappingListSortRequest
{
    public string? Field { get; set; }

    public string? Direction { get; set; }
}

