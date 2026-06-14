using ArunVastra.Application.DTOs.AdditionalCharges;

namespace ArunVastra.Application.Interfaces;

public interface IAdditionalChargeRepository
{
    Task<AdditionalChargeListResponse> ListAsync(AdditionalChargeListRequest request, CancellationToken cancellationToken = default);

    Task<AdditionalChargeResponse?> GetByIdAsync(int AdditionalChargeId, CancellationToken cancellationToken = default);

    Task<bool> CategoryExistsAsync(int categoryId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AdditionalChargeCategoryOptionResponse>> ListCategoryOptionsAsync(CancellationToken cancellationToken = default);

    Task<AdditionalChargeResponse> CreateAsync(CreateAdditionalChargeRequest request, CancellationToken cancellationToken = default);

    Task<AdditionalChargeResponse?> UpdateAsync(int AdditionalChargeId, UpdateAdditionalChargeRequest request, CancellationToken cancellationToken = default);
}

