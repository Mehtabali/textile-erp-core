namespace ArunVastra.Application.DTOs.SupplierTransportMappings;

public sealed class SupplierTransportMappingListResponse
{
    public IReadOnlyList<SupplierTransportMappingResponse> Items { get; set; } = [];

    public int TotalRecords { get; set; }

    public int PageNumber { get; set; }

    public int PageSize { get; set; }
}
