using ArunVastra.Application.DTOs.SaleVouchers;
using ArunVastra.Application.Models;

namespace ArunVastra.Application.Interfaces;

public interface ISaleVoucherRepository
{
    Task<SaleVoucherListResponse> ListAsync(
        SaleVoucherListRequest request,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default);

    Task<SaleVoucherResponse?> GetByIdAsync(
        int saleVoucherId,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VoucherStatusHistoryResponse>> GetStatusHistoryAsync(
        int saleVoucherId,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FloorResponse>> ListFloorsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SaleVoucherSupplierFilterOptionResponse>> ListSupplierFilterOptionsAsync(
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SaleVoucherCompanyFilterOptionResponse>> ListCompanyFilterOptionsAsync(
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SaleVoucherFloorFilterOptionResponse>> ListFloorFilterOptionsAsync(
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default);

    Task<bool> CompanyCanBeUsedAsync(
        int companyId,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default);

    Task<bool> TransportExistsAsync(int transportId, CancellationToken cancellationToken = default);

    Task<bool> FloorExistsAsync(int floorId, CancellationToken cancellationToken = default);

    Task<bool> SupplierProductsCanBeUsedAsync(
        IReadOnlyCollection<int> supplierProductIds,
        int companyId,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default);

    Task<SaleVoucherResponse> CreateAsync(
        CreateSaleVoucherRequest request,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default);

    Task<SaleVoucherResponse?> UpdateAsync(
        int saleVoucherId,
        UpdateSaleVoucherRequest request,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default);

    Task<SaleVoucherResponse?> ChangeStatusAsync(
        int saleVoucherId,
        ChangeSaleVoucherStatusRequest request,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default);

    Task<SaleVoucherResponse?> CancelAsync(
        int saleVoucherId,
        CancelSaleVoucherRequest? request,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        int saleVoucherId,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default);
}
