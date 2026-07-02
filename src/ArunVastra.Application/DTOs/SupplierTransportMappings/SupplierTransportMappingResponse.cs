namespace ArunVastra.Application.DTOs.SupplierTransportMappings;

public sealed class SupplierTransportMappingResponse
{
    public int SupplierUserId { get; set; }

    public string SupplierCode { get; set; } = string.Empty;

    public string SupplierName { get; set; } = string.Empty;

    public IReadOnlyList<int> MappedTransportIds { get; set; } = [];

    public IReadOnlyList<string> MappedTransportNames { get; set; } = [];
}
