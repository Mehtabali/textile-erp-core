using ArunVastra.Application.DTOs.SupplierDashboard;
using ArunVastra.Application.Interfaces;
using ArunVastra.Domain.Enums;
using ArunVastra.Infrastructure.Data;
using ArunVastra.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArunVastra.Infrastructure.Repositories;

public sealed class SupplierDashboardRepository(ArunVastraDbContext dbContext) : ISupplierDashboardRepository
{
    private readonly ArunVastraDbContext _dbContext = dbContext;

    public async Task<SupplierDashboardResponse> GetAsync(
        int supplierUserId,
        CancellationToken cancellationToken = default)
    {
        var supplierVouchers = _dbContext.SaleVouchers
            .AsNoTracking()
            .Where(voucher => voucher.Company.Userid == supplierUserId);

        var productQuery = _dbContext.SupItems
            .AsNoTracking()
            .Where(product => product.Userid == supplierUserId);

        return new SupplierDashboardResponse
        {
            Ready = await CountVouchersByStatusAsync(supplierVouchers, SaleVoucherStatus.Ready, cancellationToken),
            Deliver = await CountVouchersByStatusAsync(supplierVouchers, SaleVoucherStatus.Deliver, cancellationToken),
            Enter = await CountVouchersByStatusAsync(supplierVouchers, SaleVoucherStatus.Enter, cancellationToken),
            Open = await CountVouchersByStatusAsync(supplierVouchers, SaleVoucherStatus.Open, cancellationToken),
            ReturnCancel = await CountVouchersByStatusAsync(supplierVouchers, SaleVoucherStatus.Cancel, cancellationToken),
            ActiveProducts = await productQuery.CountAsync(product => product.Isactive, cancellationToken),
            DeactiveProducts = await productQuery.CountAsync(product => !product.Isactive, cancellationToken)
        };
    }

    private static Task<int> CountVouchersByStatusAsync(
        IQueryable<SaleVoucher> query,
        SaleVoucherStatus status,
        CancellationToken cancellationToken)
    {
        return query.CountAsync(voucher => voucher.Status == (byte)status, cancellationToken);
    }
}
