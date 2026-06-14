using ArunVastra.Application.DTOs.GstRules;

namespace ArunVastra.Application.Interfaces;

public interface IGstRuleService
{
    Task<GstRuleListResponse> ListAsync(GstRuleListRequest request, CancellationToken cancellationToken = default);

    Task<GstRuleResponse?> GetByIdAsync(int gstRuleId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GstRuleCategoryOptionResponse>> ListCategoryOptionsAsync(CancellationToken cancellationToken = default);

    Task<GstRuleResponse> CreateAsync(CreateGstRuleRequest request, CancellationToken cancellationToken = default);

    Task<GstRuleResponse?> UpdateAsync(int gstRuleId, UpdateGstRuleRequest request, CancellationToken cancellationToken = default);
}
