using ArunVastra.Application.DTOs.AdditionalCharges;
using ArunVastra.Application.Interfaces;

namespace ArunVastra.Application.Services;

public sealed class AdditionalChargeService(IAdditionalChargeRepository additionalChargeRepository) : IAdditionalChargeService
{
    private readonly IAdditionalChargeRepository _additionalChargeRepository = additionalChargeRepository;

    public Task<AdditionalChargeListResponse> ListAsync(
        AdditionalChargeListRequest request,
        CancellationToken cancellationToken = default)
    {
        request.PageNumber = Math.Max(request.PageNumber, 1);
        request.PageSize = Math.Clamp(request.PageSize, 1, 1000);
        request.Filters ??= new AdditionalChargeListFiltersRequest();

        return _additionalChargeRepository.ListAsync(request, cancellationToken);
    }

    public Task<AdditionalChargeResponse?> GetByIdAsync(
        int additionalChargeId,
        CancellationToken cancellationToken = default)
    {
        ValidateId(additionalChargeId, "additional charge id");

        return _additionalChargeRepository.GetByIdAsync(additionalChargeId, cancellationToken);
    }

    public Task<IReadOnlyList<AdditionalChargeCategoryOptionResponse>> ListCategoryOptionsAsync(
        CancellationToken cancellationToken = default)
    {
        return _additionalChargeRepository.ListCategoryOptionsAsync(cancellationToken);
    }

    public async Task<AdditionalChargeResponse> CreateAsync(
        CreateAdditionalChargeRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedRequest = await NormalizeCreateRequestAsync(request, cancellationToken);

        return await _additionalChargeRepository.CreateAsync(normalizedRequest, cancellationToken);
    }

    public async Task<AdditionalChargeResponse?> UpdateAsync(
        int additionalChargeId,
        UpdateAdditionalChargeRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateId(additionalChargeId, "additional charge id");

        var normalizedRequest = await NormalizeUpdateRequestAsync(request, cancellationToken);

        return await _additionalChargeRepository.UpdateAsync(additionalChargeId, normalizedRequest, cancellationToken);
    }

    private async Task<CreateAdditionalChargeRequest> NormalizeCreateRequestAsync(
        CreateAdditionalChargeRequest request,
        CancellationToken cancellationToken)
    {
        await ValidateCategoryAsync(request.StockGroupId, cancellationToken);
        var startRange = NormalizeRequiredAmount(request.StartRange, "Start");
        var endRange = NormalizeRequiredAmount(request.EndRange, "End");
        ValidateRange(startRange, endRange);

        return new CreateAdditionalChargeRequest
        {
            StockGroupId = request.StockGroupId,
            GstValue = NormalizeRequiredAmount(request.GstValue, "Value"),
            StartRange = startRange,
            EndRange = endRange
        };
    }

    private async Task<UpdateAdditionalChargeRequest> NormalizeUpdateRequestAsync(
        UpdateAdditionalChargeRequest request,
        CancellationToken cancellationToken)
    {
        await ValidateCategoryAsync(request.StockGroupId, cancellationToken);
        var startRange = NormalizeRequiredAmount(request.StartRange, "Start");
        var endRange = NormalizeRequiredAmount(request.EndRange, "End");
        ValidateRange(startRange, endRange);

        return new UpdateAdditionalChargeRequest
        {
            StockGroupId = request.StockGroupId,
            GstValue = NormalizeRequiredAmount(request.GstValue, "Value"),
            StartRange = startRange,
            EndRange = endRange
        };
    }

    private async Task ValidateCategoryAsync(int categoryId, CancellationToken cancellationToken)
    {
        ValidateId(categoryId, "Product category");

        if (!await _additionalChargeRepository.CategoryExistsAsync(categoryId, cancellationToken))
        {
            throw new InvalidOperationException("Product category selection is invalid.");
        }
    }

    private static void ValidateId(int id, string name)
    {
        if (id <= 0)
        {
            throw new InvalidOperationException($"{name} must be greater than zero.");
        }
    }

    private static decimal NormalizeRequiredAmount(decimal? value, string name)
    {
        if (!value.HasValue)
        {
            throw new InvalidOperationException($"{name} is required.");
        }

        if (value.Value < 0)
        {
            throw new InvalidOperationException($"{name} cannot be negative.");
        }

        return value.Value;
    }

    private static void ValidateRange(decimal startRange, decimal endRange)
    {
        if (endRange < startRange)
        {
            throw new InvalidOperationException("End should not be less than Start.");
        }
    }
}

