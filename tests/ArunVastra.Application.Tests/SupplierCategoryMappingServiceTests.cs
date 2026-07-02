using ArunVastra.Application.DTOs.SupplierCategoryMappings;
using ArunVastra.Application.Interfaces;
using ArunVastra.Application.Services;

namespace ArunVastra.Application.Tests;

public sealed class SupplierCategoryMappingServiceTests
{
    [Fact]
    public async Task ListAsync_WhenPagingIsInvalid_NormalizesAndDelegates()
    {
        var repository = new FakeSupplierCategoryMappingRepository();
        var service = new SupplierCategoryMappingService(repository);

        await service.ListAsync(new SupplierCategoryMappingListRequest
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
        var service = new SupplierCategoryMappingService(new FakeSupplierCategoryMappingRepository());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SaveAsync(new SaveSupplierCategoryMappingRequest
            {
                SupplierUserId = 0,
                ProductCategoryIds = [2]
            }));

        Assert.Equal("Supplier user id must be greater than zero.", ex.Message);
    }

    [Fact]
    public async Task SaveAsync_WhenProductCategoryIdsAreMissing_Throws()
    {
        var service = new SupplierCategoryMappingService(new FakeSupplierCategoryMappingRepository());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SaveAsync(new SaveSupplierCategoryMappingRequest
            {
                SupplierUserId = 1,
                ProductCategoryIds = []
            }));

        Assert.Equal("At least one product category is required.", ex.Message);
    }

    [Fact]
    public async Task SaveAsync_WhenSupplierDoesNotExist_Throws()
    {
        var repository = new FakeSupplierCategoryMappingRepository
        {
            SupplierExists = false
        };
        var service = new SupplierCategoryMappingService(repository);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SaveAsync(new SaveSupplierCategoryMappingRequest
            {
                SupplierUserId = 1,
                ProductCategoryIds = [2]
            }));

        Assert.Equal("Supplier selection is invalid.", ex.Message);
        Assert.Null(repository.SavedRequest);
    }

    [Fact]
    public async Task SaveAsync_WhenProductCategoryDoesNotExist_Throws()
    {
        var repository = new FakeSupplierCategoryMappingRepository
        {
            ValidProductCategoryIds = new HashSet<int> { 2 }
        };
        var service = new SupplierCategoryMappingService(repository);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SaveAsync(new SaveSupplierCategoryMappingRequest
            {
                SupplierUserId = 1,
                ProductCategoryIds = [2, 3]
            }));

        Assert.Equal("One or more product category selections are invalid.", ex.Message);
        Assert.Null(repository.SavedRequest);
    }

    [Fact]
    public async Task SaveAsync_WhenRequestIsValid_DeduplicatesAndDelegates()
    {
        var repository = new FakeSupplierCategoryMappingRepository
        {
            ValidProductCategoryIds = new HashSet<int> { 2, 3 }
        };
        var service = new SupplierCategoryMappingService(repository);

        var response = await service.SaveAsync(new SaveSupplierCategoryMappingRequest
        {
            SupplierUserId = 1,
            ProductCategoryIds = [2, 2, 3]
        });

        Assert.Equal(1, response.SupplierUserId);
        Assert.Equal([2, 3], repository.SavedRequest?.ProductCategoryIds);
    }

    [Fact]
    public async Task SearchSuppliersAsync_WhenTakeIsTooLarge_ClampsTake()
    {
        var repository = new FakeSupplierCategoryMappingRepository();
        var service = new SupplierCategoryMappingService(repository);

        await service.SearchSuppliersAsync(" supplier ", 100);

        Assert.Equal("supplier", repository.LastSupplierSearchKeyword);
        Assert.Equal(50, repository.LastSupplierTake);
    }

    [Fact]
    public async Task RemoveAsync_WhenMappingExists_DelegatesToRepository()
    {
        var repository = new FakeSupplierCategoryMappingRepository();
        var service = new SupplierCategoryMappingService(repository);

        await service.RemoveAsync(1, 2);

        Assert.Equal(1, repository.RemovedSupplierUserId);
        Assert.Equal(2, repository.RemovedProductCategoryId);
    }

    [Fact]
    public async Task RemoveAsync_WhenMappingDoesNotExist_Throws()
    {
        var repository = new FakeSupplierCategoryMappingRepository
        {
            RemoveResult = false
        };
        var service = new SupplierCategoryMappingService(repository);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.RemoveAsync(1, 2));

        Assert.Equal("Supplier category mapping was not found.", ex.Message);
    }

    private sealed class FakeSupplierCategoryMappingRepository : ISupplierCategoryMappingRepository
    {
        public bool SupplierExists { get; set; } = true;

        public bool RemoveResult { get; set; } = true;

        public IReadOnlySet<int>? ValidProductCategoryIds { get; set; }

        public SupplierCategoryMappingListRequest? LastListRequest { get; private set; }

        public SaveSupplierCategoryMappingRequest? SavedRequest { get; private set; }

        public int? RemovedSupplierUserId { get; private set; }

        public int? RemovedProductCategoryId { get; private set; }

        public string? LastSupplierSearchKeyword { get; private set; }

        public int LastSupplierTake { get; private set; }

        public Task<SupplierCategoryMappingListResponse> ListAsync(
            SupplierCategoryMappingListRequest request,
            CancellationToken cancellationToken = default)
        {
            LastListRequest = request;
            return Task.FromResult(new SupplierCategoryMappingListResponse
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            });
        }

        public Task<SupplierCategoryMappingResponse?> GetBySupplierUserIdAsync(
            int supplierUserId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<SupplierCategoryMappingResponse?>(Response(supplierUserId, [2]));
        }

        public Task<IReadOnlyList<SupplierCategoryMappingOptionResponse>> SearchSuppliersAsync(
            string? searchKeyword,
            int take,
            CancellationToken cancellationToken = default)
        {
            LastSupplierSearchKeyword = searchKeyword;
            LastSupplierTake = take;
            IReadOnlyList<SupplierCategoryMappingOptionResponse> options = [];
            return Task.FromResult(options);
        }

        public Task<IReadOnlyList<SupplierCategoryMappingOptionResponse>> SearchProductCategoriesAsync(
            string? searchKeyword,
            int take,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<SupplierCategoryMappingOptionResponse> options = [];
            return Task.FromResult(options);
        }

        public Task<bool> SupplierExistsAsync(
            int supplierUserId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(SupplierExists);
        }

        public Task<IReadOnlySet<int>> GetValidProductCategoryIdsAsync(
            IReadOnlyCollection<int> productCategoryIds,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ValidProductCategoryIds ?? productCategoryIds.ToHashSet());
        }

        public Task<SupplierCategoryMappingResponse> SaveAsync(
            SaveSupplierCategoryMappingRequest request,
            CancellationToken cancellationToken = default)
        {
            SavedRequest = request;
            return Task.FromResult(Response(request.SupplierUserId, request.ProductCategoryIds));
        }

        public Task<bool> RemoveAsync(
            int supplierUserId,
            int productCategoryId,
            CancellationToken cancellationToken = default)
        {
            RemovedSupplierUserId = supplierUserId;
            RemovedProductCategoryId = productCategoryId;
            return Task.FromResult(RemoveResult);
        }

        private static SupplierCategoryMappingResponse Response(
            int supplierUserId,
            IReadOnlyList<int> productCategoryIds)
        {
            return new SupplierCategoryMappingResponse
            {
                SupplierUserId = supplierUserId,
                SupplierCode = "S001",
                SupplierName = "Supplier",
                MappedProductCategoryIds = productCategoryIds,
                MappedProductCategoryNames = productCategoryIds.Select(id => $"Category {id}").ToList()
            };
        }
    }
}
