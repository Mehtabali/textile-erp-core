using ArunVastra.Application.DTOs.SupplierTransportMappings;
using ArunVastra.Application.Interfaces;
using ArunVastra.Application.Services;

namespace ArunVastra.Application.Tests;

public sealed class SupplierTransportMappingServiceTests
{
    [Fact]
    public async Task ListAsync_WhenPagingIsInvalid_NormalizesAndDelegates()
    {
        var repository = new FakeSupplierTransportMappingRepository();
        var service = new SupplierTransportMappingService(repository);

        await service.ListAsync(new SupplierTransportMappingListRequest
        {
            PageNumber = 0,
            PageSize = 0
        });

        Assert.Equal(1, repository.LastListRequest?.PageNumber);
        Assert.Equal(1, repository.LastListRequest?.PageSize);
    }

    [Fact]
    public async Task SaveAsync_WhenSupplierIsMissing_Throws()
    {
        var service = new SupplierTransportMappingService(new FakeSupplierTransportMappingRepository());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SaveAsync(new SaveSupplierTransportMappingRequest
            {
                SupplierUserId = 0,
                TransportIds = [2]
            }));

        Assert.Equal("Supplier user id must be greater than zero.", ex.Message);
    }

    [Fact]
    public async Task SaveAsync_WhenTransportIdsAreMissing_Throws()
    {
        var service = new SupplierTransportMappingService(new FakeSupplierTransportMappingRepository());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SaveAsync(new SaveSupplierTransportMappingRequest
            {
                SupplierUserId = 1,
                TransportIds = []
            }));

        Assert.Equal("At least one transport is required.", ex.Message);
    }

    [Fact]
    public async Task SaveAsync_WhenSupplierDoesNotExist_Throws()
    {
        var repository = new FakeSupplierTransportMappingRepository
        {
            SupplierExists = false
        };
        var service = new SupplierTransportMappingService(repository);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SaveAsync(new SaveSupplierTransportMappingRequest
            {
                SupplierUserId = 1,
                TransportIds = [2]
            }));

        Assert.Equal("Supplier selection is invalid.", ex.Message);
        Assert.Null(repository.SavedRequest);
    }

    [Fact]
    public async Task SaveAsync_WhenTransportDoesNotExist_Throws()
    {
        var repository = new FakeSupplierTransportMappingRepository
        {
            ValidTransportIds = new HashSet<int> { 2 }
        };
        var service = new SupplierTransportMappingService(repository);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SaveAsync(new SaveSupplierTransportMappingRequest
            {
                SupplierUserId = 1,
                TransportIds = [2, 3]
            }));

        Assert.Equal("One or more transport selections are invalid.", ex.Message);
        Assert.Null(repository.SavedRequest);
    }

    [Fact]
    public async Task SaveAsync_WhenRequestIsValid_DeduplicatesAndDelegates()
    {
        var repository = new FakeSupplierTransportMappingRepository
        {
            ValidTransportIds = new HashSet<int> { 2, 3 }
        };
        var service = new SupplierTransportMappingService(repository);

        var response = await service.SaveAsync(new SaveSupplierTransportMappingRequest
        {
            SupplierUserId = 1,
            TransportIds = [2, 2, 3]
        });

        Assert.Equal(1, response.SupplierUserId);
        Assert.Equal([2, 3], repository.SavedRequest?.TransportIds);
    }

    [Fact]
    public async Task SearchSuppliersAsync_WhenTakeIsTooLarge_ClampsTake()
    {
        var repository = new FakeSupplierTransportMappingRepository();
        var service = new SupplierTransportMappingService(repository);

        await service.SearchSuppliersAsync(" supplier ", 100);

        Assert.Equal("supplier", repository.LastSupplierSearchKeyword);
        Assert.Equal(50, repository.LastSupplierTake);
    }

    [Fact]
    public async Task RemoveAsync_WhenMappingExists_DelegatesToRepository()
    {
        var repository = new FakeSupplierTransportMappingRepository();
        var service = new SupplierTransportMappingService(repository);

        await service.RemoveAsync(1, 2);

        Assert.Equal(1, repository.RemovedSupplierUserId);
        Assert.Equal(2, repository.RemovedTransportUserId);
    }

    [Fact]
    public async Task RemoveAsync_WhenMappingDoesNotExist_Throws()
    {
        var repository = new FakeSupplierTransportMappingRepository
        {
            RemoveResult = false
        };
        var service = new SupplierTransportMappingService(repository);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.RemoveAsync(1, 2));

        Assert.Equal("Supplier transport mapping was not found.", ex.Message);
    }

    private sealed class FakeSupplierTransportMappingRepository : ISupplierTransportMappingRepository
    {
        public bool SupplierExists { get; set; } = true;

        public bool RemoveResult { get; set; } = true;

        public IReadOnlySet<int>? ValidTransportIds { get; set; }

        public SupplierTransportMappingListRequest? LastListRequest { get; private set; }

        public SaveSupplierTransportMappingRequest? SavedRequest { get; private set; }

        public int? RemovedSupplierUserId { get; private set; }

        public int? RemovedTransportUserId { get; private set; }

        public string? LastSupplierSearchKeyword { get; private set; }

        public int LastSupplierTake { get; private set; }

        public Task<SupplierTransportMappingListResponse> ListAsync(
            SupplierTransportMappingListRequest request,
            CancellationToken cancellationToken = default)
        {
            LastListRequest = request;
            return Task.FromResult(new SupplierTransportMappingListResponse
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            });
        }

        public Task<SupplierTransportMappingResponse?> GetBySupplierUserIdAsync(
            int supplierUserId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<SupplierTransportMappingResponse?>(Response(supplierUserId, [2]));
        }

        public Task<IReadOnlyList<SupplierTransportMappingOptionResponse>> SearchSuppliersAsync(
            string? searchKeyword,
            int take,
            CancellationToken cancellationToken = default)
        {
            LastSupplierSearchKeyword = searchKeyword;
            LastSupplierTake = take;
            IReadOnlyList<SupplierTransportMappingOptionResponse> options = [];
            return Task.FromResult(options);
        }

        public Task<IReadOnlyList<SupplierTransportMappingOptionResponse>> SearchTransportsAsync(
            string? searchKeyword,
            int take,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<SupplierTransportMappingOptionResponse> options = [];
            return Task.FromResult(options);
        }

        public Task<bool> SupplierExistsAsync(
            int supplierUserId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(SupplierExists);
        }

        public Task<IReadOnlySet<int>> GetValidTransportIdsAsync(
            IReadOnlyCollection<int> transportIds,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ValidTransportIds ?? transportIds.ToHashSet());
        }

        public Task<SupplierTransportMappingResponse> SaveAsync(
            SaveSupplierTransportMappingRequest request,
            CancellationToken cancellationToken = default)
        {
            SavedRequest = request;
            return Task.FromResult(Response(request.SupplierUserId, request.TransportIds));
        }

        public Task<bool> RemoveAsync(
            int supplierUserId,
            int transportUserId,
            CancellationToken cancellationToken = default)
        {
            RemovedSupplierUserId = supplierUserId;
            RemovedTransportUserId = transportUserId;
            return Task.FromResult(RemoveResult);
        }

        private static SupplierTransportMappingResponse Response(
            int supplierUserId,
            IReadOnlyList<int> transportIds)
        {
            return new SupplierTransportMappingResponse
            {
                SupplierUserId = supplierUserId,
                SupplierCode = "S001",
                SupplierName = "Supplier",
                MappedTransportIds = transportIds,
                MappedTransportNames = transportIds.Select(id => $"Transport {id}").ToList()
            };
        }
    }
}
