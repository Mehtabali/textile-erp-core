using ArunVastra.Application.DTOs.AdditionalCharges;

namespace ArunVastra.Application.Interfaces;

public interface IAdditionalChargeService
{
    Task<AdditionalChargeListResponse> ListAsync(AdditionalChargeListRequest request, CancellationToken cancellationToken = default);

    Task<AdditionalChargeResponse?> GetByIdAsync(int additionalChargeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AdditionalChargeCategoryOptionResponse>> ListCategoryOptionsAsync(CancellationToken cancellationToken = default);

    Task<AdditionalChargeResponse> CreateAsync(CreateAdditionalChargeRequest request, CancellationToken cancellationToken = default);

    Task<AdditionalChargeResponse?> UpdateAsync(int additionalChargeId, UpdateAdditionalChargeRequest request, CancellationToken cancellationToken = default);
}

