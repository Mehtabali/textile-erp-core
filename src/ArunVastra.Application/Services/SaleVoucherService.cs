using ArunVastra.Application.DTOs.SaleVouchers;
using ArunVastra.Application.Interfaces;
using ArunVastra.Application.Models;
using ArunVastra.Domain.Enums;

namespace ArunVastra.Application.Services;

public sealed class SaleVoucherService(ISaleVoucherRepository saleVoucherRepository) : ISaleVoucherService
{
    private const int MaxChallanLength = 50;

    private readonly ISaleVoucherRepository _saleVoucherRepository = saleVoucherRepository;

    public Task<SaleVoucherListResponse> ListAsync(
        SaleVoucherListRequest request,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default)
    {
        ValidateCurrentUser(currentUser);
        NormalizePaging(request);

        return _saleVoucherRepository.ListAsync(request, currentUser, cancellationToken);
    }

    public Task<SaleVoucherResponse?> GetByIdAsync(
        int saleVoucherId,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default)
    {
        ValidateId(saleVoucherId, "Sale voucher id");
        ValidateCurrentUser(currentUser);

        return _saleVoucherRepository.GetByIdAsync(saleVoucherId, currentUser, cancellationToken);
    }

    public Task<IReadOnlyList<VoucherStatusHistoryResponse>> GetStatusHistoryAsync(
        int saleVoucherId,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default)
    {
        ValidateId(saleVoucherId, "Sale voucher id");
        ValidateCurrentUser(currentUser);

        return _saleVoucherRepository.GetStatusHistoryAsync(saleVoucherId, currentUser, cancellationToken);
    }

    public Task<IReadOnlyList<FloorResponse>> ListFloorsAsync(CancellationToken cancellationToken = default)
    {
        return _saleVoucherRepository.ListFloorsAsync(cancellationToken);
    }

    public Task<IReadOnlyList<SaleVoucherSupplierFilterOptionResponse>> ListSupplierFilterOptionsAsync(
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default)
    {
        ValidateCurrentUser(currentUser);

        return _saleVoucherRepository.ListSupplierFilterOptionsAsync(currentUser, cancellationToken);
    }

    public Task<IReadOnlyList<SaleVoucherCompanyFilterOptionResponse>> ListCompanyFilterOptionsAsync(
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default)
    {
        ValidateCurrentUser(currentUser);

        return _saleVoucherRepository.ListCompanyFilterOptionsAsync(currentUser, cancellationToken);
    }

    public Task<IReadOnlyList<SaleVoucherFloorFilterOptionResponse>> ListFloorFilterOptionsAsync(
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default)
    {
        ValidateCurrentUser(currentUser);

        return _saleVoucherRepository.ListFloorFilterOptionsAsync(currentUser, cancellationToken);
    }

    public async Task<SaleVoucherResponse> CreateAsync(
        CreateSaleVoucherRequest request,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default)
    {
        ValidateCurrentUser(currentUser);
        ValidateHeader(request.CompanyId, request.TransportId, request.FloorId, request.Challan, request.Status);
        ValidateDate(request.Date);
        ValidateDetails(request.Details);

        var normalizedRequest = new CreateSaleVoucherRequest
        {
            CompanyId = request.CompanyId,
            TransportId = request.TransportId,
            FloorId = request.FloorId,
            Date = request.Date,
            Challan = request.Challan.Trim(),
            Status = request.Status,
            Details = NormalizeDetails(request.Details)
        };

        await ValidateReferencesAsync(normalizedRequest, currentUser, cancellationToken);

        return await _saleVoucherRepository.CreateAsync(normalizedRequest, currentUser, cancellationToken);
    }

    public async Task<SaleVoucherResponse?> UpdateAsync(
        int saleVoucherId,
        UpdateSaleVoucherRequest request,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default)
    {
        ValidateId(saleVoucherId, "Sale voucher id");
        ValidateCurrentUser(currentUser);
        ValidateHeader(request.CompanyId, request.TransportId, request.FloorId, request.Challan, request.Status);
        ValidateDetails(request.Details);

        var normalizedRequest = new UpdateSaleVoucherRequest
        {
            CompanyId = request.CompanyId,
            TransportId = request.TransportId,
            FloorId = request.FloorId,
            Challan = request.Challan.Trim(),
            Status = request.Status,
            Details = NormalizeDetails(request.Details)
        };

        await ValidateReferencesAsync(normalizedRequest, currentUser, cancellationToken);

        return await _saleVoucherRepository.UpdateAsync(saleVoucherId, normalizedRequest, currentUser, cancellationToken);
    }

    public async Task<SaleVoucherResponse?> ChangeStatusAsync(
        int saleVoucherId,
        ChangeSaleVoucherStatusRequest request,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default)
    {
        ValidateId(saleVoucherId, "Sale voucher id");
        ValidateCurrentUser(currentUser);
        ValidateStatus(request.Status);

        var normalizedRequest = new ChangeSaleVoucherStatusRequest
        {
            Status = request.Status,
            Remarks = NormalizeNullable(request.Remarks, 100)
        };

        return await _saleVoucherRepository.ChangeStatusAsync(
            saleVoucherId,
            normalizedRequest,
            currentUser,
            cancellationToken);
    }

    public Task<SaleVoucherResponse?> CancelAsync(
        int saleVoucherId,
        CancelSaleVoucherRequest? request,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default)
    {
        ValidateId(saleVoucherId, "Sale voucher id");
        ValidateCurrentUser(currentUser);

        var cancelRequest = new CancelSaleVoucherRequest
        {
            Remarks = NormalizeNullable(request?.Remarks, 100)
        };

        return _saleVoucherRepository.CancelAsync(
            saleVoucherId,
            cancelRequest,
            currentUser,
            cancellationToken);
    }

    public Task<bool> DeleteAsync(
        int saleVoucherId,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default)
    {
        ValidateId(saleVoucherId, "Sale voucher id");
        ValidateCurrentUser(currentUser);

        if (!CanDelete(currentUser))
        {
            throw new InvalidOperationException("You do not have permission to delete sale vouchers.");
        }

        return _saleVoucherRepository.DeleteAsync(saleVoucherId, currentUser, cancellationToken);
    }

    private async Task ValidateReferencesAsync(
        CreateSaleVoucherRequest request,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken)
    {
        if (!await _saleVoucherRepository.CompanyCanBeUsedAsync(request.CompanyId, currentUser, cancellationToken))
        {
            throw new InvalidOperationException("Company selection is invalid.");
        }

        if (!await _saleVoucherRepository.TransportExistsAsync(request.TransportId, cancellationToken))
        {
            throw new InvalidOperationException("Transport selection is invalid.");
        }

        if (!await _saleVoucherRepository.FloorExistsAsync(request.FloorId, cancellationToken))
        {
            throw new InvalidOperationException("Floor selection is invalid.");
        }

        var supplierProductIds = request.Details.Select(detail => detail.SupplierProductId).Distinct().ToArray();
        if (!await _saleVoucherRepository.SupplierProductsCanBeUsedAsync(
                supplierProductIds,
                request.CompanyId,
                currentUser,
                cancellationToken))
        {
            throw new InvalidOperationException("One or more products are invalid for this sale voucher.");
        }
    }

    private Task ValidateReferencesAsync(
        UpdateSaleVoucherRequest request,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken)
    {
        return ValidateReferencesAsync(
            new CreateSaleVoucherRequest
            {
                CompanyId = request.CompanyId,
                TransportId = request.TransportId,
                FloorId = request.FloorId,
                Challan = request.Challan,
                Status = request.Status,
                Details = request.Details
            },
            currentUser,
            cancellationToken);
    }

    private static void ValidateHeader(
        int companyId,
        int transportId,
        int floorId,
        string? challan,
        int status)
    {
        ValidateId(companyId, "Company id");
        ValidateId(transportId, "Transport id");
        ValidateId(floorId, "Floor id");
        ValidateStatus(status);

        if (string.IsNullOrWhiteSpace(challan))
        {
            throw new InvalidOperationException("Challan is required.");
        }

        if (challan.Trim().Length > MaxChallanLength)
        {
            throw new InvalidOperationException($"Challan cannot exceed {MaxChallanLength} characters.");
        }
    }

    private static void ValidateDate(DateTime date)
    {
        if (date == default)
        {
            throw new InvalidOperationException("Date is required.");
        }
    }

    private static void ValidateDetails(IReadOnlyList<SaleVoucherDetailRequest>? details)
    {
        if (details is null || details.Count == 0)
        {
            throw new InvalidOperationException("Please add at least one product to generate sale voucher.");
        }

        foreach (var detail in details)
        {
            ValidateId(detail.SupplierProductId, "Supplier product id");

            if (detail.Purchase <= 0)
            {
                throw new InvalidOperationException("Purchase must be greater than zero.");
            }

            if (detail.Mrp <= 0)
            {
                throw new InvalidOperationException("MRP must be greater than zero.");
            }

            if (detail.Quantity <= 0)
            {
                throw new InvalidOperationException("Quantity must be greater than zero.");
            }
        }
    }

    private static IReadOnlyList<SaleVoucherDetailRequest> NormalizeDetails(
        IReadOnlyList<SaleVoucherDetailRequest> details)
    {
        return details
            .Select(detail => new SaleVoucherDetailRequest
            {
                SaleVoucherDetailId = detail.SaleVoucherDetailId,
                SupplierProductId = detail.SupplierProductId,
                Purchase = detail.Purchase,
                Mrp = detail.Mrp,
                Quantity = detail.Quantity
            })
            .ToList();
    }

    private static void ValidateStatus(int status)
    {
        if (!Enum.IsDefined(typeof(SaleVoucherStatus), status))
        {
            throw new InvalidOperationException("Sale voucher status is invalid.");
        }
    }

    private static void ValidateId(int id, string name)
    {
        if (id <= 0)
        {
            throw new InvalidOperationException($"{name} must be greater than zero.");
        }
    }

    private static void ValidateCurrentUser(CurrentUserContext currentUser)
    {
        ValidateId(currentUser.UserId, "Current user id");
    }

    private static bool CanDelete(CurrentUserContext currentUser)
    {
        return currentUser.Role == (int)UserRole.Admin;
    }

    private static void NormalizePaging(SaleVoucherListRequest request)
    {
        if (request.PageNumber <= 0)
        {
            request.PageNumber = 1;
        }

        if (request.PageSize <= 0)
        {
            request.PageSize = 10;
        }

        if (request.PageSize > 1000)
        {
            request.PageSize = 1000;
        }
    }

    private static string? NormalizeNullable(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new InvalidOperationException($"Remarks cannot exceed {maxLength} characters.");
        }

        return normalized;
    }
}
