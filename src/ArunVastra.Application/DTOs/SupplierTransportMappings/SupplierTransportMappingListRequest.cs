namespace ArunVastra.Application.DTOs.SupplierTransportMappings;

public sealed class SupplierTransportMappingListRequest
{
    public string? SearchKeyword { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public SupplierTransportMappingListFiltersRequest Filters { get; set; } = new();

    public SupplierTransportMappingListSortRequest? Sort { get; set; }
}

public sealed class SupplierTransportMappingListFiltersRequest
{
    public string? SupplierCode { get; set; }

    public string? SupplierName { get; set; }

    public string? TransportName { get; set; }
}

public sealed class SupplierTransportMappingListSortRequest
{
    public string? Field { get; set; }

    public string? Direction { get; set; }
}
