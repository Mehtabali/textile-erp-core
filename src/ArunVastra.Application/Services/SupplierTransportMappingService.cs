using ArunVastra.Application.DTOs.SupplierTransportMappings;
using ArunVastra.Application.Interfaces;

namespace ArunVastra.Application.Services;

public sealed class SupplierTransportMappingService(
    ISupplierTransportMappingRepository supplierTransportMappingRepository) : ISupplierTransportMappingService
{
    private readonly ISupplierTransportMappingRepository _supplierTransportMappingRepository =
        supplierTransportMappingRepository;

    public Task<SupplierTransportMappingListResponse> ListAsync(
        SupplierTransportMappingListRequest request,
        CancellationToken cancellationToken = default)
    {
        request.PageNumber = Math.Max(request.PageNumber, 1);
        request.PageSize = Math.Clamp(request.PageSize, 1, 1000);
        request.Filters ??= new SupplierTransportMappingListFiltersRequest();

        return _supplierTransportMappingRepository.ListAsync(request, cancellationToken);
    }

    public Task<SupplierTransportMappingResponse?> GetBySupplierUserIdAsync(
        int supplierUserId,
        CancellationToken cancellationToken = default)
    {
        ValidateId(supplierUserId, "Supplier user id");

        return _supplierTransportMappingRepository.GetBySupplierUserIdAsync(supplierUserId, cancellationToken);
    }

    public Task<IReadOnlyList<SupplierTransportMappingOptionResponse>> SearchSuppliersAsync(
        string? searchKeyword,
        int take,
        CancellationToken cancellationToken = default)
    {
        return _supplierTransportMappingRepository.SearchSuppliersAsync(
            NormalizeNullable(searchKeyword, 100),
            NormalizeTake(take),
            cancellationToken);
    }

    public Task<IReadOnlyList<SupplierTransportMappingOptionResponse>> SearchTransportsAsync(
        string? searchKeyword,
        int take,
        CancellationToken cancellationToken = default)
    {
        return _supplierTransportMappingRepository.SearchTransportsAsync(
            NormalizeNullable(searchKeyword, 100),
            NormalizeTake(take),
            cancellationToken);
    }

    public async Task<SupplierTransportMappingResponse> SaveAsync(
        SaveSupplierTransportMappingRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateId(request.SupplierUserId, "Supplier user id");

        var transportIds = request.TransportIds
            .Where(id => id > 0)
            .Distinct()
            .ToArray();

        if (transportIds.Length == 0)
        {
            throw new InvalidOperationException("At least one transport is required.");
        }

        if (!await _supplierTransportMappingRepository.SupplierExistsAsync(
                request.SupplierUserId,
                cancellationToken))
        {
            throw new InvalidOperationException("Supplier selection is invalid.");
        }

        var validTransportIds = await _supplierTransportMappingRepository.GetValidTransportIdsAsync(
            transportIds,
            cancellationToken);

        if (validTransportIds.Count != transportIds.Length)
        {
            throw new InvalidOperationException("One or more transport selections are invalid.");
        }

        return await _supplierTransportMappingRepository.SaveAsync(
            new SaveSupplierTransportMappingRequest
            {
                SupplierUserId = request.SupplierUserId,
                TransportIds = transportIds
            },
            cancellationToken);
    }

    public async Task RemoveAsync(
        int supplierUserId,
        int transportUserId,
        CancellationToken cancellationToken = default)
    {
        ValidateId(supplierUserId, "Supplier user id");
        ValidateId(transportUserId, "Transport user id");

        var removed = await _supplierTransportMappingRepository.RemoveAsync(
            supplierUserId,
            transportUserId,
            cancellationToken);

        if (!removed)
        {
            throw new InvalidOperationException("Supplier transport mapping was not found.");
        }
    }

    private static void ValidateId(int id, string name)
    {
        if (id <= 0)
        {
            throw new InvalidOperationException($"{name} must be greater than zero.");
        }
    }

    private static int NormalizeTake(int take)
    {
        return Math.Clamp(take <= 0 ? 20 : take, 1, 50);
    }

    private static string? NormalizeNullable(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        return normalized.Length > maxLength ? normalized[..maxLength] : normalized;
    }
}
