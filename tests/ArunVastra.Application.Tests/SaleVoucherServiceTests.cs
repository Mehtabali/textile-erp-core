using ArunVastra.Application.DTOs.SaleVouchers;
using ArunVastra.Application.Interfaces;
using ArunVastra.Application.Models;
using ArunVastra.Application.Services;
using ArunVastra.Domain.Enums;

namespace ArunVastra.Application.Tests;

public sealed class SaleVoucherServiceTests
{
    private static readonly CurrentUserContext CurrentUser = new()
    {
        UserId = 11,
        Role = (int)UserRole.Supplier
    };

    [Fact]
    public async Task CreateAsync_WhenRequestIsValid_NormalizesAndCreatesSaleVoucher()
    {
        var repository = new FakeSaleVoucherRepository();
        var service = new SaleVoucherService(repository);

        var response = await service.CreateAsync(
            new CreateSaleVoucherRequest
            {
                CompanyId = 3,
                TransportId = 4,
                FloorId = 5,
                Date = new DateTime(2026, 6, 9),
                Challan = " INV-1 ",
                Status = (int)SaleVoucherStatus.Ready,
                Details =
                [
                    new SaleVoucherDetailRequest
                    {
                        SupplierProductId = 6,
                        Purchase = 100,
                        Mrp = 150,
                        Quantity = 2
                    }
                ]
            },
            CurrentUser);

        Assert.Equal(1, response.SaleVoucherId);
        Assert.Equal("INV-1", repository.CreatedRequest?.Challan);
        Assert.Equal((int)SaleVoucherStatus.Ready, repository.CreatedRequest?.Status);
        Assert.Single(repository.CreatedRequest?.Details ?? []);
    }

    [Fact]
    public async Task CreateAsync_WhenStatusIsLegacyButUnsupported_Throws()
    {
        var service = new SaleVoucherService(new FakeSaleVoucherRepository());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(
                ValidCreateRequest(status: 1),
                CurrentUser));

        Assert.Equal("Sale voucher status is invalid.", ex.Message);
    }

    [Fact]
    public async Task CreateAsync_WhenDetailsAreMissing_Throws()
    {
        var service = new SaleVoucherService(new FakeSaleVoucherRepository());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(
                new CreateSaleVoucherRequest
                {
                    CompanyId = 1,
                    TransportId = 2,
                    FloorId = 3,
                    Date = DateTime.UtcNow,
                    Challan = "INV",
                    Status = (int)SaleVoucherStatus.Ready,
                    Details = []
                },
                CurrentUser));

        Assert.Equal("Please add at least one product to generate sale voucher.", ex.Message);
    }

    [Fact]
    public async Task CreateAsync_WhenCompanyIsInvalid_Throws()
    {
        var repository = new FakeSaleVoucherRepository
        {
            CompanyCanBeUsed = false
        };
        var service = new SaleVoucherService(repository);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(ValidCreateRequest(), CurrentUser));

        Assert.Equal("Company selection is invalid.", ex.Message);
        Assert.Null(repository.CreatedRequest);
    }

    [Fact]
    public async Task UpdateAsync_WhenRequestIsValid_NormalizesAndUpdatesSaleVoucher()
    {
        var repository = new FakeSaleVoucherRepository();
        var service = new SaleVoucherService(repository);

        var response = await service.UpdateAsync(
            7,
            new UpdateSaleVoucherRequest
            {
                CompanyId = 3,
                TransportId = 4,
                FloorId = 5,
                Challan = " INV-2 ",
                Status = (int)SaleVoucherStatus.Enter,
                Details =
                [
                    new SaleVoucherDetailRequest
                    {
                        SaleVoucherDetailId = 9,
                        SupplierProductId = 6,
                        Purchase = 100,
                        Mrp = 150,
                        Quantity = 2
                    }
                ]
            },
            CurrentUser);

        Assert.NotNull(response);
        Assert.Equal(7, repository.UpdatedSaleVoucherId);
        Assert.Equal("INV-2", repository.UpdatedRequest?.Challan);
        Assert.Equal((int)SaleVoucherStatus.Enter, repository.UpdatedRequest?.Status);
    }

    [Fact]
    public async Task ChangeStatusAsync_WhenRequestIsValid_NormalizesRemarks()
    {
        var repository = new FakeSaleVoucherRepository();
        var service = new SaleVoucherService(repository);

        var response = await service.ChangeStatusAsync(
            7,
            new ChangeSaleVoucherStatusRequest
            {
                Status = (int)SaleVoucherStatus.Cancel,
                Remarks = " Cancelled by admin "
            },
            CurrentUser);

        Assert.NotNull(response);
        Assert.Equal(7, repository.StatusChangedSaleVoucherId);
        Assert.Equal((int)SaleVoucherStatus.Cancel, repository.ChangeStatusRequest?.Status);
        Assert.Equal("Cancelled by admin", repository.ChangeStatusRequest?.Remarks);
    }

    [Fact]
    public async Task CancelAsync_WhenRequestHasRemarks_SetsCancelStatusAndNormalizesRemarks()
    {
        var repository = new FakeSaleVoucherRepository();
        var service = new SaleVoucherService(repository);

        var response = await service.CancelAsync(
            7,
            new CancelSaleVoucherRequest
            {
                Remarks = " Cancelled from list "
            },
            CurrentUser);

        Assert.NotNull(response);
        Assert.Equal(7, repository.CancelledSaleVoucherId);
        Assert.Equal("Cancelled from list", repository.CancelRequest?.Remarks);
    }

    [Fact]
    public async Task CancelAsync_WhenRequestBodyIsMissing_StillCancelsSaleVoucher()
    {
        var repository = new FakeSaleVoucherRepository();
        var service = new SaleVoucherService(repository);

        var response = await service.CancelAsync(7, null, CurrentUser);

        Assert.NotNull(response);
        Assert.Equal(7, repository.CancelledSaleVoucherId);
        Assert.Null(repository.CancelRequest?.Remarks);
    }

    [Fact]
    public async Task DeleteAsync_WhenUserIsInternal_DelegatesToRepository()
    {
        var repository = new FakeSaleVoucherRepository();
        var service = new SaleVoucherService(repository);

        var deleted = await service.DeleteAsync(
            12,
            new CurrentUserContext
            {
                UserId = 11,
                Role = (int)UserRole.Admin
            });

        Assert.True(deleted);
        Assert.Equal(12, repository.DeletedSaleVoucherId);
    }

    [Fact]
    public async Task DeleteAsync_WhenUserIsSupplier_Throws()
    {
        var repository = new FakeSaleVoucherRepository();
        var service = new SaleVoucherService(repository);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.DeleteAsync(12, CurrentUser));

        Assert.Equal("You do not have permission to delete sale vouchers.", ex.Message);
        Assert.Equal(0, repository.DeletedSaleVoucherId);
    }

    [Fact]
    public async Task ListAsync_WhenPagingIsInvalid_NormalizesPagingAndDelegates()
    {
        var repository = new FakeSaleVoucherRepository();
        var service = new SaleVoucherService(repository);

        await service.ListAsync(
            new SaleVoucherListRequest
            {
                PageNumber = 0,
                PageSize = 0
            },
            CurrentUser);

        Assert.Equal(1, repository.LastListRequest?.PageNumber);
        Assert.Equal(10, repository.LastListRequest?.PageSize);
    }

    [Fact]
    public async Task ListSupplierFilterOptionsAsync_WhenCurrentUserIsValid_DelegatesToRepository()
    {
        var repository = new FakeSaleVoucherRepository();
        var service = new SaleVoucherService(repository);

        var suppliers = await service.ListSupplierFilterOptionsAsync(CurrentUser);

        Assert.Single(suppliers);
        Assert.Equal(CurrentUser.UserId, repository.SupplierFilterCurrentUser?.UserId);
    }

    [Fact]
    public async Task ListCompanyFilterOptionsAsync_WhenCurrentUserIsValid_DelegatesToRepository()
    {
        var repository = new FakeSaleVoucherRepository();
        var service = new SaleVoucherService(repository);

        var companies = await service.ListCompanyFilterOptionsAsync(CurrentUser);

        Assert.Single(companies);
        Assert.Equal(CurrentUser.UserId, repository.CompanyFilterCurrentUser?.UserId);
    }

    [Fact]
    public async Task ListFloorFilterOptionsAsync_WhenCurrentUserIsValid_DelegatesToRepository()
    {
        var repository = new FakeSaleVoucherRepository();
        var service = new SaleVoucherService(repository);

        var floors = await service.ListFloorFilterOptionsAsync(CurrentUser);

        Assert.Single(floors);
        Assert.Equal(CurrentUser.UserId, repository.FloorFilterCurrentUser?.UserId);
    }

    private static CreateSaleVoucherRequest ValidCreateRequest(int status = (int)SaleVoucherStatus.Ready)
    {
        return new CreateSaleVoucherRequest
        {
            CompanyId = 1,
            TransportId = 2,
            FloorId = 3,
            Date = DateTime.UtcNow,
            Challan = "INV",
            Status = status,
            Details =
            [
                new SaleVoucherDetailRequest
                {
                    SupplierProductId = 4,
                    Purchase = 100,
                    Mrp = 150,
                    Quantity = 1
                }
            ]
        };
    }

    private sealed class FakeSaleVoucherRepository : ISaleVoucherRepository
    {
        public bool CompanyCanBeUsed { get; set; } = true;

        public bool TransportExists { get; set; } = true;

        public bool FloorExists { get; set; } = true;

        public bool ProductsCanBeUsed { get; set; } = true;

        public SaleVoucherListRequest? LastListRequest { get; private set; }

        public CreateSaleVoucherRequest? CreatedRequest { get; private set; }

        public int UpdatedSaleVoucherId { get; private set; }

        public UpdateSaleVoucherRequest? UpdatedRequest { get; private set; }

        public int StatusChangedSaleVoucherId { get; private set; }

        public ChangeSaleVoucherStatusRequest? ChangeStatusRequest { get; private set; }

        public CurrentUserContext? SupplierFilterCurrentUser { get; private set; }

        public CurrentUserContext? CompanyFilterCurrentUser { get; private set; }

        public CurrentUserContext? FloorFilterCurrentUser { get; private set; }

        public int CancelledSaleVoucherId { get; private set; }

        public CancelSaleVoucherRequest? CancelRequest { get; private set; }

        public int DeletedSaleVoucherId { get; private set; }

        public Task<SaleVoucherListResponse> ListAsync(
            SaleVoucherListRequest request,
            CurrentUserContext currentUser,
            CancellationToken cancellationToken = default)
        {
            LastListRequest = request;
            return Task.FromResult(new SaleVoucherListResponse
            {
                Items = [],
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            });
        }

        public Task<SaleVoucherResponse?> GetByIdAsync(
            int saleVoucherId,
            CurrentUserContext currentUser,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<SaleVoucherResponse?>(Response(saleVoucherId));
        }

        public Task<IReadOnlyList<VoucherStatusHistoryResponse>> GetStatusHistoryAsync(
            int saleVoucherId,
            CurrentUserContext currentUser,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<VoucherStatusHistoryResponse> history = [];
            return Task.FromResult(history);
        }

        public Task<IReadOnlyList<FloorResponse>> ListFloorsAsync(CancellationToken cancellationToken = default)
        {
            IReadOnlyList<FloorResponse> floors = [];
            return Task.FromResult(floors);
        }

        public Task<IReadOnlyList<SaleVoucherSupplierFilterOptionResponse>> ListSupplierFilterOptionsAsync(
            CurrentUserContext currentUser,
            CancellationToken cancellationToken = default)
        {
            SupplierFilterCurrentUser = currentUser;
            IReadOnlyList<SaleVoucherSupplierFilterOptionResponse> suppliers =
            [
                new()
                {
                    SupplierUserId = currentUser.UserId,
                    SupplierName = "Supplier"
                }
            ];

            return Task.FromResult(suppliers);
        }

        public Task<IReadOnlyList<SaleVoucherCompanyFilterOptionResponse>> ListCompanyFilterOptionsAsync(
            CurrentUserContext currentUser,
            CancellationToken cancellationToken = default)
        {
            CompanyFilterCurrentUser = currentUser;
            IReadOnlyList<SaleVoucherCompanyFilterOptionResponse> companies =
            [
                new()
                {
                    CompanyId = 3,
                    CompanyName = "Company"
                }
            ];

            return Task.FromResult(companies);
        }

        public Task<IReadOnlyList<SaleVoucherFloorFilterOptionResponse>> ListFloorFilterOptionsAsync(
            CurrentUserContext currentUser,
            CancellationToken cancellationToken = default)
        {
            FloorFilterCurrentUser = currentUser;
            IReadOnlyList<SaleVoucherFloorFilterOptionResponse> floors =
            [
                new()
                {
                    FloorId = 5,
                    FloorName = "Floor"
                }
            ];

            return Task.FromResult(floors);
        }

        public Task<bool> CompanyCanBeUsedAsync(
            int companyId,
            CurrentUserContext currentUser,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CompanyCanBeUsed);
        }

        public Task<bool> TransportExistsAsync(int transportId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(TransportExists);
        }

        public Task<bool> FloorExistsAsync(int floorId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(FloorExists);
        }

        public Task<bool> SupplierProductsCanBeUsedAsync(
            IReadOnlyCollection<int> supplierProductIds,
            int companyId,
            CurrentUserContext currentUser,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ProductsCanBeUsed);
        }

        public Task<SaleVoucherResponse> CreateAsync(
            CreateSaleVoucherRequest request,
            CurrentUserContext currentUser,
            CancellationToken cancellationToken = default)
        {
            CreatedRequest = request;
            return Task.FromResult(Response(1));
        }

        public Task<SaleVoucherResponse?> UpdateAsync(
            int saleVoucherId,
            UpdateSaleVoucherRequest request,
            CurrentUserContext currentUser,
            CancellationToken cancellationToken = default)
        {
            UpdatedSaleVoucherId = saleVoucherId;
            UpdatedRequest = request;
            return Task.FromResult<SaleVoucherResponse?>(Response(saleVoucherId));
        }

        public Task<SaleVoucherResponse?> ChangeStatusAsync(
            int saleVoucherId,
            ChangeSaleVoucherStatusRequest request,
            CurrentUserContext currentUser,
            CancellationToken cancellationToken = default)
        {
            StatusChangedSaleVoucherId = saleVoucherId;
            ChangeStatusRequest = request;
            return Task.FromResult<SaleVoucherResponse?>(Response(saleVoucherId));
        }

        public Task<SaleVoucherResponse?> CancelAsync(
            int saleVoucherId,
            CancelSaleVoucherRequest? request,
            CurrentUserContext currentUser,
            CancellationToken cancellationToken = default)
        {
            CancelledSaleVoucherId = saleVoucherId;
            CancelRequest = request;
            return Task.FromResult<SaleVoucherResponse?>(Response(saleVoucherId));
        }

        public Task<bool> DeleteAsync(
            int saleVoucherId,
            CurrentUserContext currentUser,
            CancellationToken cancellationToken = default)
        {
            DeletedSaleVoucherId = saleVoucherId;
            return Task.FromResult(true);
        }

        private static SaleVoucherResponse Response(int saleVoucherId)
        {
            return new SaleVoucherResponse
            {
                SaleVoucherId = saleVoucherId,
                Status = (int)SaleVoucherStatus.Ready,
                StatusName = nameof(SaleVoucherStatus.Ready)
            };
        }
    }
}
