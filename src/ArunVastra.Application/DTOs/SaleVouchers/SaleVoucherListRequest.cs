namespace ArunVastra.Application.DTOs.SaleVouchers;

public sealed class SaleVoucherListRequest
{
    public string? SearchKeyword { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public SaleVoucherListFiltersRequest Filters { get; set; } = new();

    public SaleVoucherListSortRequest? Sort { get; set; }
}

public sealed class SaleVoucherListFiltersRequest
{
    public string? AutoBillNo { get; set; }

    public string? SupplierName { get; set; }

    public string? Challan { get; set; }

    public string? CompanyName { get; set; }

    public string? Status { get; set; }

    public DateTime? Date { get; set; }

    public string? FloorName { get; set; }
}

public sealed class SaleVoucherListSortRequest
{
    public string? Field { get; set; }

    public string? Direction { get; set; }
}

public sealed class SaleVoucherListResponse
{
    public IReadOnlyList<SaleVoucherListItemResponse> Items { get; set; } = [];

    public int TotalRecords { get; set; }

    public int PageNumber { get; set; }

    public int PageSize { get; set; }
}
