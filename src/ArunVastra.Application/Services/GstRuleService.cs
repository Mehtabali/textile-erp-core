using ArunVastra.Application.DTOs.GstRules;
using ArunVastra.Application.Interfaces;

namespace ArunVastra.Application.Services;

public sealed class GstRuleService(IGstRuleRepository gstRuleRepository) : IGstRuleService
{
    private readonly IGstRuleRepository _gstRuleRepository = gstRuleRepository;

    public Task<GstRuleListResponse> ListAsync(
        GstRuleListRequest request,
        CancellationToken cancellationToken = default)
    {
        request.PageNumber = Math.Max(request.PageNumber, 1);
        request.PageSize = Math.Clamp(request.PageSize, 1, 1000);
        request.Filters ??= new GstRuleListFiltersRequest();

        return _gstRuleRepository.ListAsync(request, cancellationToken);
    }

    public Task<GstRuleResponse?> GetByIdAsync(
        int gstRuleId,
        CancellationToken cancellationToken = default)
    {
        ValidateId(gstRuleId, "GST rule id");

        return _gstRuleRepository.GetByIdAsync(gstRuleId, cancellationToken);
    }

    public Task<IReadOnlyList<GstRuleCategoryOptionResponse>> ListCategoryOptionsAsync(
        CancellationToken cancellationToken = default)
    {
        return _gstRuleRepository.ListCategoryOptionsAsync(cancellationToken);
    }

    public async Task<GstRuleResponse> CreateAsync(
        CreateGstRuleRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedRequest = await NormalizeCreateRequestAsync(request, cancellationToken);

        return await _gstRuleRepository.CreateAsync(normalizedRequest, cancellationToken);
    }

    public async Task<GstRuleResponse?> UpdateAsync(
        int gstRuleId,
        UpdateGstRuleRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateId(gstRuleId, "GST rule id");

        var normalizedRequest = await NormalizeUpdateRequestAsync(request, cancellationToken);

        return await _gstRuleRepository.UpdateAsync(gstRuleId, normalizedRequest, cancellationToken);
    }

    private async Task<CreateGstRuleRequest> NormalizeCreateRequestAsync(
        CreateGstRuleRequest request,
        CancellationToken cancellationToken)
    {
        await ValidateCategoryAsync(request.StockGroupId, cancellationToken);
        var startRange = NormalizeRequiredAmount(request.StartRange, "Start");
        var endRange = NormalizeRequiredAmount(request.EndRange, "End");
        ValidateRange(startRange, endRange);

        return new CreateGstRuleRequest
        {
            StockGroupId = request.StockGroupId,
            GstValue = NormalizeRequiredAmount(request.GstValue, "Value"),
            StartRange = startRange,
            EndRange = endRange
        };
    }

    private async Task<UpdateGstRuleRequest> NormalizeUpdateRequestAsync(
        UpdateGstRuleRequest request,
        CancellationToken cancellationToken)
    {
        await ValidateCategoryAsync(request.StockGroupId, cancellationToken);
        var startRange = NormalizeRequiredAmount(request.StartRange, "Start");
        var endRange = NormalizeRequiredAmount(request.EndRange, "End");
        ValidateRange(startRange, endRange);

        return new UpdateGstRuleRequest
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

        if (!await _gstRuleRepository.CategoryExistsAsync(categoryId, cancellationToken))
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
