using ArunVastra.Application.DTOs.Locations;
using ArunVastra.Application.Interfaces;
using ArunVastra.Application.Services;

namespace ArunVastra.Application.Tests;

public sealed class StateServiceTests
{
    [Fact]
    public async Task ListAsync_DelegatesToRepository()
    {
        var repository = new FakeStateRepository();
        var service = new StateService(repository);

        var states = await service.ListAsync();

        Assert.Single(states);
        Assert.True(repository.ListWasCalled);
    }

    [Fact]
    public async Task GetByIdAsync_WhenIdIsValid_DelegatesToRepository()
    {
        var repository = new FakeStateRepository();
        var service = new StateService(repository);

        var state = await service.GetByIdAsync(9);

        Assert.NotNull(state);
        Assert.Equal(9, repository.LastRequestedStateId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetByIdAsync_WhenIdIsInvalid_Throws(int stateId)
    {
        var service = new StateService(new FakeStateRepository());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GetByIdAsync(stateId));

        Assert.Equal("State id must be greater than zero.", ex.Message);
    }

    [Fact]
    public async Task CreateAsync_WhenRequestIsValid_NormalizesAndCreatesState()
    {
        var repository = new FakeStateRepository();
        var service = new StateService(repository);

        var state = await service.CreateAsync(new CreateStateRequest
        {
            StateName = " Uttar Pradesh ",
            GstStateCode = 9
        });

        Assert.Equal(1, state.StateId);
        Assert.Equal("Uttar Pradesh", repository.CreatedRequest?.StateName);
        Assert.Equal(9, repository.CreatedRequest?.GstStateCode);
    }

    [Fact]
    public async Task UpdateAsync_WhenRequestIsValid_NormalizesAndUpdatesState()
    {
        var repository = new FakeStateRepository();
        var service = new StateService(repository);

        var state = await service.UpdateAsync(
            2,
            new UpdateStateRequest
            {
                StateName = " Delhi ",
                GstStateCode = 7
            });

        Assert.NotNull(state);
        Assert.Equal(2, repository.LastUpdatedStateId);
        Assert.Equal("Delhi", repository.UpdatedRequest?.StateName);
        Assert.Equal(7, repository.UpdatedRequest?.GstStateCode);
    }

    [Fact]
    public async Task CreateAsync_WhenNameAlreadyExists_Throws()
    {
        var repository = new FakeStateRepository
        {
            NameExists = true
        };
        var service = new StateService(repository);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateStateRequest { StateName = "Delhi" }));

        Assert.Equal("State name already exists.", ex.Message);
        Assert.Null(repository.CreatedRequest);
    }

    [Fact]
    public async Task CreateAsync_WhenNameIsMissing_Throws()
    {
        var service = new StateService(new FakeStateRepository());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateStateRequest { StateName = " " }));

        Assert.Equal("State name is required.", ex.Message);
    }

    private sealed class FakeStateRepository : IStateRepository
    {
        public bool ListWasCalled { get; private set; }

        public bool NameExists { get; set; }

        public int LastRequestedStateId { get; private set; }

        public int LastUpdatedStateId { get; private set; }

        public CreateStateRequest? CreatedRequest { get; private set; }

        public UpdateStateRequest? UpdatedRequest { get; private set; }

        public Task<IReadOnlyList<StateResponse>> ListAsync(
            CancellationToken cancellationToken = default)
        {
            ListWasCalled = true;

            IReadOnlyList<StateResponse> states =
            [
                new StateResponse
                {
                    StateId = 1,
                    StateName = "Uttar Pradesh",
                    GstStateCode = 9
                }
            ];

            return Task.FromResult(states);
        }

        public Task<StateResponse?> GetByIdAsync(
            int stateId,
            CancellationToken cancellationToken = default)
        {
            LastRequestedStateId = stateId;

            return Task.FromResult<StateResponse?>(new StateResponse
            {
                StateId = stateId,
                StateName = "Uttar Pradesh",
                GstStateCode = 9
            });
        }

        public Task<bool> NameExistsAsync(
            string stateName,
            int? excludingStateId = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(NameExists);
        }

        public Task<StateResponse> CreateAsync(
            CreateStateRequest request,
            CancellationToken cancellationToken = default)
        {
            CreatedRequest = request;

            return Task.FromResult(new StateResponse
            {
                StateId = 1,
                StateName = request.StateName,
                GstStateCode = request.GstStateCode
            });
        }

        public Task<StateResponse?> UpdateAsync(
            int stateId,
            UpdateStateRequest request,
            CancellationToken cancellationToken = default)
        {
            LastUpdatedStateId = stateId;
            UpdatedRequest = request;

            return Task.FromResult<StateResponse?>(new StateResponse
            {
                StateId = stateId,
                StateName = request.StateName,
                GstStateCode = request.GstStateCode
            });
        }
    }
}
