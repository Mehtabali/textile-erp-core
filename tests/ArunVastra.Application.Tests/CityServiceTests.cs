using ArunVastra.Application.DTOs.Locations;
using ArunVastra.Application.Interfaces;
using ArunVastra.Application.Services;

namespace ArunVastra.Application.Tests;

public sealed class CityServiceTests
{
    [Fact]
    public async Task ListByStateAsync_WhenStateIdIsValid_DelegatesToRepository()
    {
        var repository = new FakeCityRepository();
        var service = new CityService(repository);

        var cities = await service.ListByStateAsync(9);

        Assert.Single(cities);
        Assert.Equal(9, repository.LastRequestedStateId);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCityIdIsValid_DelegatesToRepository()
    {
        var repository = new FakeCityRepository();
        var service = new CityService(repository);

        var city = await service.GetByIdAsync(11);

        Assert.NotNull(city);
        Assert.Equal(11, repository.LastRequestedCityId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ListByStateAsync_WhenStateIdIsInvalid_Throws(int stateId)
    {
        var service = new CityService(new FakeCityRepository());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ListByStateAsync(stateId));

        Assert.Equal("State id must be greater than zero.", ex.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetByIdAsync_WhenCityIdIsInvalid_Throws(int cityId)
    {
        var service = new CityService(new FakeCityRepository());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GetByIdAsync(cityId));

        Assert.Equal("City id must be greater than zero.", ex.Message);
    }

    [Fact]
    public async Task CreateAsync_WhenRequestIsValid_NormalizesAndCreatesCity()
    {
        var repository = new FakeCityRepository();
        var service = new CityService(repository);

        var city = await service.CreateAsync(new CreateCityRequest
        {
            StateId = 9,
            CityName = " Kanpur "
        });

        Assert.Equal(11, city.CityId);
        Assert.Equal(9, repository.CreatedRequest?.StateId);
        Assert.Equal("Kanpur", repository.CreatedRequest?.CityName);
    }

    [Fact]
    public async Task UpdateAsync_WhenRequestIsValid_NormalizesAndUpdatesCity()
    {
        var repository = new FakeCityRepository();
        var service = new CityService(repository);

        var city = await service.UpdateAsync(
            15,
            new UpdateCityRequest
            {
                StateId = 7,
                CityName = " Delhi "
            });

        Assert.NotNull(city);
        Assert.Equal(15, repository.LastUpdatedCityId);
        Assert.Equal(7, repository.UpdatedRequest?.StateId);
        Assert.Equal("Delhi", repository.UpdatedRequest?.CityName);
    }

    [Fact]
    public async Task CreateAsync_WhenStateDoesNotExist_Throws()
    {
        var repository = new FakeCityRepository
        {
            StateExists = false
        };
        var service = new CityService(repository);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateCityRequest
            {
                StateId = 9,
                CityName = "Kanpur"
            }));

        Assert.Equal("State does not exist.", ex.Message);
        Assert.Null(repository.CreatedRequest);
    }

    [Fact]
    public async Task CreateAsync_WhenNameAlreadyExistsInState_Throws()
    {
        var repository = new FakeCityRepository
        {
            CityNameExists = true
        };
        var service = new CityService(repository);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateCityRequest
            {
                StateId = 9,
                CityName = "Kanpur"
            }));

        Assert.Equal("City name already exists for this state.", ex.Message);
        Assert.Null(repository.CreatedRequest);
    }

    [Fact]
    public async Task CreateAsync_WhenNameIsMissing_Throws()
    {
        var service = new CityService(new FakeCityRepository());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateCityRequest
            {
                StateId = 9,
                CityName = " "
            }));

        Assert.Equal("City name is required.", ex.Message);
    }

    private sealed class FakeCityRepository : ICityRepository
    {
        public int LastRequestedStateId { get; private set; }

        public int LastRequestedCityId { get; private set; }

        public int LastUpdatedCityId { get; private set; }

        public bool StateExists { get; set; } = true;

        public bool CityNameExists { get; set; }

        public CreateCityRequest? CreatedRequest { get; private set; }

        public UpdateCityRequest? UpdatedRequest { get; private set; }

        public Task<IReadOnlyList<CityResponse>> ListAsync(CancellationToken cancellationToken = default)
        {
            IReadOnlyList<CityResponse> cities =
            [
                new CityResponse
                {
                    CityId = 11,
                    StateId = 9,
                    CityName = "Kanpur"
                }
            ];

            return Task.FromResult(cities);
        }

        public Task<IReadOnlyList<CityResponse>> ListByStateAsync(
            int stateId,
            CancellationToken cancellationToken = default)
        {
            LastRequestedStateId = stateId;

            IReadOnlyList<CityResponse> cities =
            [
                new CityResponse
                {
                    CityId = 11,
                    StateId = stateId,
                    CityName = "Kanpur"
                }
            ];

            return Task.FromResult(cities);
        }

        public Task<CityResponse?> GetByIdAsync(
            int cityId,
            CancellationToken cancellationToken = default)
        {
            LastRequestedCityId = cityId;

            return Task.FromResult<CityResponse?>(new CityResponse
            {
                CityId = cityId,
                StateId = 9,
                CityName = "Kanpur"
            });
        }

        public Task<bool> StateExistsAsync(
            int stateId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(StateExists);
        }

        public Task<bool> CityBelongsToStateAsync(
            int cityId,
            int stateId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        public Task<bool> NameExistsInStateAsync(
            int stateId,
            string cityName,
            int? excludingCityId = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CityNameExists);
        }

        public Task<CityResponse> CreateAsync(
            CreateCityRequest request,
            CancellationToken cancellationToken = default)
        {
            CreatedRequest = request;

            return Task.FromResult(new CityResponse
            {
                CityId = 11,
                StateId = request.StateId,
                CityName = request.CityName
            });
        }

        public Task<CityResponse?> UpdateAsync(
            int cityId,
            UpdateCityRequest request,
            CancellationToken cancellationToken = default)
        {
            LastUpdatedCityId = cityId;
            UpdatedRequest = request;

            return Task.FromResult<CityResponse?>(new CityResponse
            {
                CityId = cityId,
                StateId = request.StateId,
                CityName = request.CityName
            });
        }
    }
}
