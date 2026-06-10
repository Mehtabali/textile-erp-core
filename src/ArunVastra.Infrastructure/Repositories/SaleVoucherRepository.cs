using System.Data;
using ArunVastra.Application.DTOs.SaleVouchers;
using ArunVastra.Application.Interfaces;
using ArunVastra.Application.Models;
using ArunVastra.Domain.Enums;
using ArunVastra.Infrastructure.Data;
using ArunVastra.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArunVastra.Infrastructure.Repositories;

public sealed class SaleVoucherRepository(ArunVastraDbContext dbContext) : ISaleVoucherRepository
{
    private readonly ArunVastraDbContext _dbContext = dbContext;

    public async Task<SaleVoucherListResponse> ListAsync(
        SaleVoucherListRequest request,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default)
    {
        var query = BuildAccessibleVoucherQuery(currentUser).AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchKeyword))
        {
            var search = request.SearchKeyword.Trim();
            query = query.Where(voucher =>
                voucher.Autobillno.ToString().Contains(search) ||
                voucher.Challan.Contains(search) ||
                voucher.Company.Compname.Contains(search) ||
                voucher.Company.User.Firstname.Contains(search) ||
                voucher.Transport.Firstname.Contains(search));
        }

        query = ApplyFilters(query, request);
        query = ApplySort(query, request);

        var totalRecords = await query.CountAsync(cancellationToken);
        var skip = (request.PageNumber - 1) * request.PageSize;

        var rows = await query
            .Skip(skip)
            .Take(request.PageSize)
            .Select(voucher => new
            {
                voucher.Svid,
                voucher.Autobillno,
                voucher.Date,
                voucher.Challan,
                voucher.Compid,
                voucher.Company.Compname,
                SupplierUserId = voucher.Company.Userid,
                SupplierName = voucher.Company.User.Firstname,
                voucher.Transid,
                TransportName = voucher.Transport.Firstname,
                voucher.Floorid,
                FloorName = _dbContext.Floors
                    .Where(floor => floor.Floorid == voucher.Floorid)
                    .Select(floor => floor.Floorname)
                    .FirstOrDefault(),
                Status = (int)voucher.Status
            })
            .ToListAsync(cancellationToken);

        var items = rows.Select(row => new SaleVoucherListItemResponse
        {
            SaleVoucherId = row.Svid,
            AutoBillNo = row.Autobillno,
            Date = row.Date,
            Challan = row.Challan,
            CompanyId = row.Compid,
            CompanyName = row.Compname,
            SupplierUserId = row.SupplierUserId,
            SupplierName = row.SupplierName,
            TransportId = row.Transid,
            TransportName = row.TransportName,
            FloorId = row.Floorid,
            FloorName = row.FloorName,
            Status = row.Status,
            StatusName = GetStatusName(row.Status),
            CanCancel = CanCancel(row.Status),
            AvailableActions = GetAvailableActions(row.Status)
        }).ToList();

        return new SaleVoucherListResponse
        {
            Items = items,
            TotalRecords = totalRecords,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<SaleVoucherResponse?> GetByIdAsync(
        int saleVoucherId,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default)
    {
        var voucher = await BuildAccessibleVoucherQuery(currentUser)
            .AsNoTracking()
            .Where(item => item.Svid == saleVoucherId)
            .Select(item => new
            {
                item.Svid,
                item.Autobillno,
                item.Compid,
                item.Company.Compname,
                item.Company.Gstin,
                item.Company.Pan,
                item.Company.Tin,
                SupplierUserId = item.Company.Userid,
                SupplierName = item.Company.User.Firstname,
                item.Transid,
                TransportName = item.Transport.Firstname,
                item.Floorid,
                FloorName = _dbContext.Floors
                    .Where(floor => floor.Floorid == item.Floorid)
                    .Select(floor => floor.Floorname)
                    .FirstOrDefault(),
                item.Date,
                item.Challan,
                item.Profit,
                Status = (int)item.Status
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (voucher is null)
        {
            return null;
        }

        var details = await GetDetailsAsync(saleVoucherId, cancellationToken);

        return BuildResponse(
            voucher.Svid,
            voucher.Autobillno,
            voucher.Compid,
            voucher.Compname,
            voucher.Gstin,
            voucher.Pan,
            voucher.Tin,
            voucher.SupplierUserId,
            voucher.SupplierName,
            voucher.Transid,
            voucher.TransportName,
            voucher.Floorid,
            voucher.FloorName,
            voucher.Date,
            voucher.Challan,
            voucher.Profit,
            voucher.Status,
            details);
    }

    public async Task<bool> DeleteAsync(
        int saleVoucherId,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default)
    {
        var canAccess = await BuildAccessibleVoucherQuery(currentUser)
            .AnyAsync(voucher => voucher.Svid == saleVoucherId, cancellationToken);

        if (!canAccess)
        {
            return false;
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"DELETE FROM VOUCHERSTATUS WHERE SVID = {saleVoucherId}",
            cancellationToken);

        await _dbContext.SaleVoucherDetails
            .Where(detail => detail.Svid == saleVoucherId)
            .ExecuteDeleteAsync(cancellationToken);

        var deleted = await _dbContext.SaleVouchers
            .Where(voucher => voucher.Svid == saleVoucherId)
            .ExecuteDeleteAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return deleted > 0;
    }

    private static SaleVoucherResponse BuildResponse(
        int saleVoucherId,
        int autoBillNo,
        int companyId,
        string companyName,
        string? companyGstin,
        string? companyPan,
        string? companyTin,
        int supplierUserId,
        string supplierName,
        int transportId,
        string transportName,
        int? floorId,
        string? floorName,
        DateTime date,
        string challan,
        int profit,
        int status,
        IReadOnlyList<SaleVoucherDetailResponse> details)
    {
        return new SaleVoucherResponse
        {
            SaleVoucherId = saleVoucherId,
            AutoBillNo = autoBillNo,
            CompanyId = companyId,
            CompanyName = companyName,
            CompanyGstin = companyGstin,
            CompanyPan = companyPan,
            CompanyTin = companyTin,
            SupplierUserId = supplierUserId,
            SupplierName = supplierName,
            TransportId = transportId,
            TransportName = transportName,
            FloorId = floorId,
            FloorName = floorName,
            Date = date,
            Challan = challan,
            Profit = profit,
            Status = status,
            StatusName = GetStatusName(status),
            CanCancel = CanCancel(status),
            AvailableActions = GetAvailableActions(status),
            TotalRows = details.Count,
            TotalPieces = details.Sum(detail => detail.Quantity),
            GrandTotal = details.Sum(detail => detail.Mrp),
            Details = details
        };
    }

    public async Task<IReadOnlyList<VoucherStatusHistoryResponse>> GetStatusHistoryAsync(
        int saleVoucherId,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default)
    {
        var canAccess = await BuildAccessibleVoucherQuery(currentUser)
            .AsNoTracking()
            .AnyAsync(voucher => voucher.Svid == saleVoucherId, cancellationToken);

        if (!canAccess)
        {
            return [];
        }

        var rows = await _dbContext.Database
            .SqlQuery<VoucherStatusHistoryRow>(
                $"""
                SELECT
                    VOUCHERSTATUS.SVID AS SaleVoucherId,
                    VOUCHERSTATUS.DATE AS Date,
                    CAST(VOUCHERSTATUS.STATUS AS int) AS Status,
                    VOUCHERSTATUS.USERID AS UserId,
                    USERS.FIRSTNAME AS UserName,
                    VOUCHERSTATUS.REMARKS AS Remarks
                FROM VOUCHERSTATUS
                JOIN USERS ON USERS.USERID = VOUCHERSTATUS.USERID
                WHERE VOUCHERSTATUS.SVID = {saleVoucherId}
                ORDER BY VOUCHERSTATUS.DATE DESC
                """)
            .ToListAsync(cancellationToken);

        return rows.Select(row => new VoucherStatusHistoryResponse
        {
            SaleVoucherId = row.SaleVoucherId,
            Date = row.Date,
            Status = row.Status,
            StatusName = GetStatusName(row.Status),
            UserId = row.UserId,
            UserName = row.UserName,
            Remarks = row.Remarks
        }).ToList();
    }

    public async Task<IReadOnlyList<FloorResponse>> ListFloorsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Floors
            .AsNoTracking()
            .Where(floor => floor.Status == "Y")
            .OrderBy(floor => floor.Floorname)
            .ThenBy(floor => floor.Floorid)
            .Select(floor => new FloorResponse
            {
                FloorId = floor.Floorid,
                FloorName = floor.Floorname
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SaleVoucherSupplierFilterOptionResponse>> ListSupplierFilterOptionsAsync(
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default)
    {
        var rows = await BuildAccessibleVoucherQuery(currentUser)
            .AsNoTracking()
            .Select(voucher => new
            {
                SupplierUserId = voucher.Company.Userid,
                SupplierName = voucher.Company.User.Firstname
            })
            .Distinct()
            .OrderBy(item => item.SupplierName)
            .ThenBy(item => item.SupplierUserId)
            .ToListAsync(cancellationToken);

        return rows.Select(row => new SaleVoucherSupplierFilterOptionResponse
        {
            SupplierUserId = row.SupplierUserId,
            SupplierName = row.SupplierName
        }).ToList();
    }

    public async Task<IReadOnlyList<SaleVoucherCompanyFilterOptionResponse>> ListCompanyFilterOptionsAsync(
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default)
    {
        var rows = await BuildAccessibleVoucherQuery(currentUser)
            .AsNoTracking()
            .Select(voucher => new
            {
                CompanyId = voucher.Compid,
                CompanyName = voucher.Company.Compname
            })
            .Distinct()
            .OrderBy(item => item.CompanyName)
            .ThenBy(item => item.CompanyId)
            .ToListAsync(cancellationToken);

        return rows.Select(row => new SaleVoucherCompanyFilterOptionResponse
        {
            CompanyId = row.CompanyId,
            CompanyName = row.CompanyName
        }).ToList();
    }

    public async Task<IReadOnlyList<SaleVoucherFloorFilterOptionResponse>> ListFloorFilterOptionsAsync(
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default)
    {
        var rows = await (
            from voucher in BuildAccessibleVoucherQuery(currentUser).AsNoTracking()
            join floor in _dbContext.Floors.AsNoTracking() on voucher.Floorid equals floor.Floorid
            where floor.Floorname != null
            select new
            {
                FloorId = floor.Floorid,
                FloorName = floor.Floorname
            })
            .Distinct()
            .OrderBy(item => item.FloorName)
            .ThenBy(item => item.FloorId)
            .ToListAsync(cancellationToken);

        return rows.Select(row => new SaleVoucherFloorFilterOptionResponse
        {
            FloorId = row.FloorId,
            FloorName = row.FloorName!
        }).ToList();
    }

    public async Task<bool> CompanyCanBeUsedAsync(
        int companyId,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Companies.AsNoTracking().Where(company => company.Compid == companyId);

        if (currentUser.Role == (int)UserRole.Supplier)
        {
            query = query.Where(company => company.Userid == currentUser.UserId);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public Task<bool> TransportExistsAsync(int transportId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users
            .AsNoTracking()
            .AnyAsync(user => user.Userid == transportId && user.Role == (int)UserRole.Transport, cancellationToken);
    }

    public Task<bool> FloorExistsAsync(int floorId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Floors
            .AsNoTracking()
            .AnyAsync(floor => floor.Floorid == floorId && floor.Status == "Y", cancellationToken);
    }

    public async Task<bool> SupplierProductsCanBeUsedAsync(
        IReadOnlyCollection<int> supplierProductIds,
        int companyId,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default)
    {
        var supplierUserId = await _dbContext.Companies
            .AsNoTracking()
            .Where(company => company.Compid == companyId)
            .Select(company => company.Userid)
            .SingleOrDefaultAsync(cancellationToken);

        if (supplierUserId == 0)
        {
            return false;
        }

        var matchedCount = await _dbContext.SupItems
            .AsNoTracking()
            .Where(item =>
                supplierProductIds.Contains(item.Supprodid) &&
                item.Userid == supplierUserId &&
                item.Isactive &&
                (!item.Compid.HasValue || item.Compid == companyId))
            .Select(item => item.Supprodid)
            .Distinct()
            .CountAsync(cancellationToken);

        return matchedCount == supplierProductIds.Count;
    }

    public async Task<SaleVoucherResponse> CreateAsync(
        CreateSaleVoucherRequest request,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var autoBillNo = await _dbContext.SaleVouchers
            .Select(voucher => (int?)voucher.Autobillno)
            .MaxAsync(cancellationToken) ?? 1;
        autoBillNo++;

        var profit = await _dbContext.Users
            .Where(user => user.Userid == currentUser.UserId)
            .Select(user => user.Profit ?? 0)
            .SingleAsync(cancellationToken);

        var voucher = new SaleVoucher
        {
            Autobillno = autoBillNo,
            Compid = request.CompanyId,
            Transid = request.TransportId,
            Date = request.Date,
            Challan = request.Challan,
            Profit = profit,
            Status = (byte)request.Status,
            Floorid = request.FloorId,
            Istsynched = "N",
            Details = request.Details.Select(ToDetailEntity).ToList()
        };

        _dbContext.SaleVouchers.Add(voucher);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await InsertVoucherStatusAsync(voucher.Svid, request.Status, null, currentUser.UserId, cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return (await GetByIdAsync(voucher.Svid, currentUser, cancellationToken))!;
    }

    public async Task<SaleVoucherResponse?> UpdateAsync(
        int saleVoucherId,
        UpdateSaleVoucherRequest request,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default)
    {
        var voucher = await BuildAccessibleVoucherQuery(currentUser)
            .Include(item => item.Details)
            .SingleOrDefaultAsync(item => item.Svid == saleVoucherId, cancellationToken);

        if (voucher is null)
        {
            return null;
        }

        voucher.Compid = request.CompanyId;
        voucher.Transid = request.TransportId;
        voucher.Challan = request.Challan;
        voucher.Status = (byte)request.Status;
        voucher.Floorid = request.FloorId;

        foreach (var detail in request.Details)
        {
            var existing = voucher.Details.FirstOrDefault(item =>
                (detail.SaleVoucherDetailId.HasValue && item.Svdetailid == detail.SaleVoucherDetailId.Value) ||
                item.Supprodid == detail.SupplierProductId);

            if (existing is null)
            {
                voucher.Details.Add(ToDetailEntity(detail));
            }
            else
            {
                existing.Supprodid = detail.SupplierProductId;
                existing.Purchase = detail.Purchase;
                existing.Mrp = detail.Mrp;
                existing.Qty = detail.Quantity;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(saleVoucherId, currentUser, cancellationToken);
    }

    public async Task<SaleVoucherResponse?> ChangeStatusAsync(
        int saleVoucherId,
        ChangeSaleVoucherStatusRequest request,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default)
    {
        var voucher = await BuildAccessibleVoucherQuery(currentUser)
            .SingleOrDefaultAsync(item => item.Svid == saleVoucherId, cancellationToken);

        if (voucher is null)
        {
            return null;
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        voucher.Status = (byte)request.Status;
        await _dbContext.SaveChangesAsync(cancellationToken);
        await InsertVoucherStatusAsync(
            saleVoucherId,
            request.Status,
            request.Remarks,
            currentUser.UserId,
            cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return await GetByIdAsync(saleVoucherId, currentUser, cancellationToken);
    }

    public async Task<SaleVoucherResponse?> CancelAsync(
        int saleVoucherId,
        CancelSaleVoucherRequest? request,
        CurrentUserContext currentUser,
        CancellationToken cancellationToken = default)
    {
        var voucher = await BuildAccessibleVoucherQuery(currentUser)
            .SingleOrDefaultAsync(item => item.Svid == saleVoucherId, cancellationToken);

        if (voucher is null)
        {
            return null;
        }

        if (voucher.Status == (byte)SaleVoucherStatus.Cancel)
        {
            throw new InvalidOperationException("Sale voucher is already cancelled.");
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        voucher.Status = (byte)SaleVoucherStatus.Cancel;
        await _dbContext.SaveChangesAsync(cancellationToken);
        await InsertVoucherStatusAsync(
            saleVoucherId,
            (int)SaleVoucherStatus.Cancel,
            request?.Remarks,
            currentUser.UserId,
            cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return await GetByIdAsync(saleVoucherId, currentUser, cancellationToken);
    }

    private IQueryable<SaleVoucher> BuildAccessibleVoucherQuery(CurrentUserContext currentUser)
    {
        var query = _dbContext.SaleVouchers
            .Include(voucher => voucher.Company)
            .ThenInclude(company => company.User)
            .Include(voucher => voucher.Transport)
            .AsQueryable();

        if (currentUser.Role == (int)UserRole.Supplier)
        {
            query = query.Where(voucher => voucher.Company.Userid == currentUser.UserId);
        }

        return query;
    }

    private IQueryable<SaleVoucher> ApplyFilters(
        IQueryable<SaleVoucher> query,
        SaleVoucherListRequest request)
    {
        var filters = request.Filters ?? new SaleVoucherListFiltersRequest();

        if (NormalizeFilter(filters.AutoBillNo) is { } autoBillNo)
        {
            query = query.Where(voucher => voucher.Autobillno.ToString().Contains(autoBillNo));
        }

        if (NormalizeFilter(filters.SupplierName) is { } supplierName)
        {
            query = query.Where(voucher => voucher.Company.User.Firstname.Contains(supplierName));
        }

        if (NormalizeFilter(filters.Challan) is { } challan)
        {
            query = query.Where(voucher => voucher.Challan.Contains(challan));
        }

        if (NormalizeFilter(filters.CompanyName) is { } companyName)
        {
            query = query.Where(voucher => voucher.Company.Compname.Contains(companyName));
        }

        if (NormalizeFilter(filters.Status) is { } status && TryParseStatus(status, out var statusValue))
        {
            query = query.Where(voucher => voucher.Status == (byte)statusValue);
        }

        if (filters.Date.HasValue)
        {
            var date = filters.Date.Value.Date;
            query = query.Where(voucher => voucher.Date.Date == date);
        }

        if (NormalizeFilter(filters.FloorName) is { } floorName)
        {
            query = query.Where(voucher => _dbContext.Floors.Any(floor =>
                floor.Floorid == voucher.Floorid &&
                floor.Floorname != null &&
                floor.Floorname.Contains(floorName)));
        }

        return query;
    }

    private IQueryable<SaleVoucher> ApplySort(
        IQueryable<SaleVoucher> query,
        SaleVoucherListRequest request)
    {
        var descending = string.Equals(request.Sort?.Direction, "desc", StringComparison.OrdinalIgnoreCase);

        return NormalizeFilter(request.Sort?.Field)?.ToLowerInvariant() switch
        {
            "autobillno" => descending
                ? query.OrderByDescending(voucher => voucher.Autobillno)
                : query.OrderBy(voucher => voucher.Autobillno),
            "date" => descending
                ? query.OrderByDescending(voucher => voucher.Date).ThenByDescending(voucher => voucher.Autobillno)
                : query.OrderBy(voucher => voucher.Date).ThenBy(voucher => voucher.Autobillno),
            "supplier" => descending
                ? query.OrderByDescending(voucher => voucher.Company.User.Firstname).ThenBy(voucher => voucher.Svid)
                : query.OrderBy(voucher => voucher.Company.User.Firstname).ThenBy(voucher => voucher.Svid),
            "challan" => descending
                ? query.OrderByDescending(voucher => voucher.Challan).ThenBy(voucher => voucher.Svid)
                : query.OrderBy(voucher => voucher.Challan).ThenBy(voucher => voucher.Svid),
            "company" => descending
                ? query.OrderByDescending(voucher => voucher.Company.Compname).ThenBy(voucher => voucher.Svid)
                : query.OrderBy(voucher => voucher.Company.Compname).ThenBy(voucher => voucher.Svid),
            "floor" => descending
                ? query.OrderByDescending(voucher => _dbContext.Floors
                    .Where(floor => floor.Floorid == voucher.Floorid)
                    .Select(floor => floor.Floorname)
                    .FirstOrDefault()).ThenBy(voucher => voucher.Svid)
                : query.OrderBy(voucher => _dbContext.Floors
                    .Where(floor => floor.Floorid == voucher.Floorid)
                    .Select(floor => floor.Floorname)
                    .FirstOrDefault()).ThenBy(voucher => voucher.Svid),
            "status" => descending
                ? query.OrderByDescending(voucher => voucher.Status).ThenBy(voucher => voucher.Svid)
                : query.OrderBy(voucher => voucher.Status).ThenBy(voucher => voucher.Svid),
            _ => descending
                ? query.OrderByDescending(voucher => voucher.Autobillno)
                : query.OrderBy(voucher => voucher.Autobillno)
        };
    }

    private async Task<IReadOnlyList<SaleVoucherDetailResponse>> GetDetailsAsync(
        int saleVoucherId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.SaleVoucherDetails
            .AsNoTracking()
            .Where(detail => detail.Svid == saleVoucherId)
            .OrderBy(detail => detail.Svdetailid)
            .Select(detail => new SaleVoucherDetailResponse
            {
                SaleVoucherDetailId = detail.Svdetailid,
                SupplierProductId = detail.Supprodid ?? 0,
                ProductName = detail.SupplierProduct != null ? detail.SupplierProduct.Product.Prodname : null,
                Description = detail.SupplierProduct != null ? detail.SupplierProduct.Description : null,
                BarCode = detail.SupplierProduct != null ? detail.SupplierProduct.Barcode : null,
                HsnCode = detail.SupplierProduct != null ? detail.SupplierProduct.Hsncode : null,
                Purchase = detail.Purchase ?? 0,
                Mrp = detail.Mrp ?? 0,
                Quantity = detail.Qty ?? 0
            })
            .ToListAsync(cancellationToken);
    }

    private Task InsertVoucherStatusAsync(
        int saleVoucherId,
        int status,
        string? remarks,
        int userId,
        CancellationToken cancellationToken)
    {
        return _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO VOUCHERSTATUS(SVID, DATE, STATUS, USERID, REMARKS)
            VALUES({saleVoucherId}, {DateTime.UtcNow}, {status}, {userId}, {remarks})
            """,
            cancellationToken);
    }

    private static SaleVoucherDetail ToDetailEntity(SaleVoucherDetailRequest detail)
    {
        return new SaleVoucherDetail
        {
            Supprodid = detail.SupplierProductId,
            Purchase = detail.Purchase,
            Mrp = detail.Mrp,
            Qty = detail.Quantity
        };
    }

    private static string? NormalizeFilter(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static bool TryParseStatus(string value, out int status)
    {
        if (int.TryParse(value, out status) && Enum.IsDefined(typeof(SaleVoucherStatus), status))
        {
            return true;
        }

        var normalized = value.Trim().ToLowerInvariant();
        foreach (SaleVoucherStatus item in Enum.GetValues<SaleVoucherStatus>())
        {
            if (item.ToString().ToLowerInvariant().StartsWith(normalized, StringComparison.Ordinal))
            {
                status = (int)item;
                return true;
            }
        }

        status = 0;
        return false;
    }

    private static string GetStatusName(int status)
    {
        return Enum.IsDefined(typeof(SaleVoucherStatus), status)
            ? ((SaleVoucherStatus)status).ToString()
            : "NA";
    }

    private static bool CanCancel(int status)
    {
        return status != (int)SaleVoucherStatus.Cancel;
    }

    private static IReadOnlyList<SaleVoucherActionResponse> GetAvailableActions(int status)
    {
        var actions = new List<SaleVoucherActionResponse>
        {
            new()
            {
                Code = "view-detail",
                Label = "View Detail"
            }
        };

        if (CanCancel(status))
        {
            actions.Add(new SaleVoucherActionResponse
            {
                Code = "cancel",
                Label = "Cancel"
            });
        }

        return actions;
    }

    private sealed class VoucherStatusHistoryRow
    {
        public int SaleVoucherId { get; set; }

        public DateTime Date { get; set; }

        public int Status { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string? Remarks { get; set; }
    }
}
