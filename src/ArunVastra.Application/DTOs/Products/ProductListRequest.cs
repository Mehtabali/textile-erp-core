namespace ArunVastra.Application.DTOs.Products;

public sealed class ProductListRequest
{
    public string? SearchKeyword { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public ProductListFiltersRequest Filters { get; set; } = new();

    public ProductListSortRequest? Sort { get; set; }
}

public sealed class ProductListFiltersRequest
{
    public string? ProductName { get; set; }

    public string? Description { get; set; }

    public string? Barcode { get; set; }

    public string? Hsn { get; set; }

    public string? Purchase { get; set; }

    public string? Mrp { get; set; }

    public string? Company { get; set; }

    public string? Formula { get; set; }

    public string? SaleRate { get; set; }
}

public sealed class ProductListSortRequest
{
    public string? Field { get; set; }

    public string? Direction { get; set; }
}

public sealed class ProductListResponse
{
    public IReadOnlyList<ProductListItemResponse> Items { get; set; } = [];

    public int TotalRecords { get; set; }

    public int PageNumber { get; set; }

    public int PageSize { get; set; }
}

public sealed class ProductAutocompleteRequest
{
    public string? Field { get; set; }

    public string? SearchKeyword { get; set; }

    public int MaxResults { get; set; } = 10;
}

public sealed class ProductAutocompleteResponse
{
    public IReadOnlyList<string> Values { get; set; } = [];
}
