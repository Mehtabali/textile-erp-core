namespace ArunVastra.Application.DTOs.SupplierTransportMappings;

public sealed class SaveSupplierTransportMappingRequest
{
    public int SupplierUserId { get; set; }

    public IReadOnlyList<int> TransportIds { get; set; } = [];
}
