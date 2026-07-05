using ArunVastra.Application.DTOs.Users.Agency;
using ArunVastra.Application.Interfaces;
using ArunVastra.Application.Models;
using ArunVastra.Application.Models.Users;
using ArunVastra.Application.Services;

namespace ArunVastra.Application.Tests;

public sealed class AgencyUserServiceTests
{
    [Fact]
    public async Task CreateAsync_WhenRequestIsValid_NormalizesAndCreatesAgencyUser()
    {
        var repository = new FakeAgencyUserRepository();
        var passwordService = new FakePasswordService();
        var service = new AgencyUserService(repository, passwordService, new FakeCityRepository());

        var response = await service.CreateAsync(new CreateAgencyUserRequest
        {
            Name = "  AVB Agency  ",
            Email = "  AGENCY@Example.COM ",
            Phone = " 011 ",
            Mobile = " 9999999999 ",
            Remarks = " Agency desk "
        });

        Assert.Equal(1, response.UserId);
        Assert.Equal("AVB Agency", response.Name);
        Assert.Equal("agency@example.com", response.Email);
        Assert.Equal("hash:Agency@123", repository.CreatedModel?.PasswordHash);
        Assert.Equal("Agency@123", repository.CreatedModel?.LegacyPassword);
        Assert.Equal("011", repository.CreatedModel?.Phone);
        Assert.Equal("9999999999", repository.CreatedModel?.Mobile);
        Assert.Equal("Agency desk", repository.CreatedModel?.Remarks);
        Assert.False(repository.CreatedModel?.Status);
        Assert.Equal("agency@example.com", passwordService.LastUser?.Email);
        Assert.Equal("4", passwordService.LastUser?.Role);
    }

    [Fact]
    public async Task CreateAsync_WhenEmailAlreadyExists_Throws()
    {
        var repository = new FakeAgencyUserRepository
        {
            EmailExists = true
        };
        var service = new AgencyUserService(repository, new FakePasswordService(), new FakeCityRepository());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateAgencyUserRequest
            {
                Name = "AVB Agency",
                Email = "agency@example.com"
            }));

        Assert.Equal("Email already exists.", ex.Message);
        Assert.Null(repository.CreatedModel);
    }

    [Fact]
    public async Task UpdateAsync_WhenRequestIsValid_NormalizesAndUpdatesAgencyUser()
    {
        var repository = new FakeAgencyUserRepository();
        var service = new AgencyUserService(repository, new FakePasswordService(), new FakeCityRepository());

        var response = await service.UpdateAsync(
            7,
            new UpdateAgencyUserRequest
            {
                Name = " Updated Agency ",
                Email = " UPDATED@Example.COM ",
                Phone = " 123 ",
                Mobile = " 456 ",
                Remarks = " Notes "
            });

        Assert.NotNull(response);
        Assert.Equal(7, response.UserId);
        Assert.Equal("Updated Agency", repository.UpdatedModel?.Name);
        Assert.Equal("updated@example.com", repository.UpdatedModel?.Email);
        Assert.Equal("123", repository.UpdatedModel?.Phone);
        Assert.Equal("456", repository.UpdatedModel?.Mobile);
        Assert.Equal("Notes", repository.UpdatedModel?.Remarks);
        Assert.False(repository.UpdatedModel?.Status);
    }

    [Fact]
    public async Task UpdateAsync_WhenEmailAlreadyExists_Throws()
    {
        var repository = new FakeAgencyUserRepository
        {
            EmailExists = true
        };
        var service = new AgencyUserService(repository, new FakePasswordService(), new FakeCityRepository());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateAsync(
                7,
                new UpdateAgencyUserRequest
                {
                    Name = "Updated Agency",
                    Email = "agency@example.com"
                }));

        Assert.Equal("Email already exists.", ex.Message);
        Assert.Null(repository.UpdatedModel);
    }

    [Fact]
    public async Task ListAsync_DelegatesToRepository()
    {
        var repository = new FakeAgencyUserRepository();
        var service = new AgencyUserService(repository, new FakePasswordService(), new FakeCityRepository());

        var request = new AgencyUserListRequest
        {
            SearchKeyword = "agency",
            IncludeLocked = false,
            PageNumber = 2,
            PageSize = 20,
            Filters = new AgencyUserListFiltersRequest
            {
                Email = "agency"
            },
            Sort = new AgencyUserListSortRequest
            {
                Field = "email",
                Direction = "desc"
            }
        };

        var result = await service.ListAsync(request);

        Assert.Single(result.Items);
        Assert.Equal(1, result.TotalRecords);
        Assert.Same(request, repository.LastListRequest);
    }

    [Fact]
    public async Task GetByIdAsync_DelegatesToRepository()
    {
        var repository = new FakeAgencyUserRepository();
        var service = new AgencyUserService(repository, new FakePasswordService(), new FakeCityRepository());

        var user = await service.GetByIdAsync(11);

        Assert.NotNull(user);
        Assert.Equal(11, repository.LastRequestedUserId);
    }

    [Fact]
    public async Task CreateAsync_WhenCityIsSelectedWithoutState_Throws()
    {
        var service = new AgencyUserService(
            new FakeAgencyUserRepository(),
            new FakePasswordService(),
            new FakeCityRepository());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateAgencyUserRequest
            {
                Name = "AVB Agency",
                Email = "agency@example.com",
                CityId = 11
            }));

        Assert.Equal("State is required when city is selected.", ex.Message);
    }

    [Fact]
    public async Task CreateAsync_WhenCityDoesNotBelongToState_Throws()
    {
        var service = new AgencyUserService(
            new FakeAgencyUserRepository(),
            new FakePasswordService(),
            new FakeCityRepository { CityBelongsToState = false });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateAgencyUserRequest
            {
                Name = "AVB Agency",
                Email = "agency@example.com",
                StateId = 9,
                CityId = 11
            }));

        Assert.Equal("City does not belong to selected state.", ex.Message);
    }

    private sealed class FakeAgencyUserRepository : IAgencyUserRepository
    {
        public bool EmailExists { get; set; }

        public AgencyUserListRequest? LastListRequest { get; private set; }

        public int LastRequestedUserId { get; private set; }

        public AgencyUserCreateModel? CreatedModel { get; private set; }

        public AgencyUserUpdateModel? UpdatedModel { get; private set; }

        public Task<AgencyUserListResponse> ListAsync(
            AgencyUserListRequest request,
            CancellationToken cancellationToken = default)
        {
            LastListRequest = request;

            IReadOnlyList<AgencyUserListItemResponse> users =
            [
                new AgencyUserListItemResponse
                {
                    UserId = 1,
                    Name = "AVB Agency",
                    Email = "agency@example.com",
                    Role = 4,
                    Phone = "011",
                    Status = true
                }
            ];

            return Task.FromResult(new AgencyUserListResponse
            {
                Items = users,
                TotalRecords = users.Count,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            });
        }

        public Task<AgencyUserResponse?> GetByIdAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            LastRequestedUserId = userId;

            return Task.FromResult<AgencyUserResponse?>(new AgencyUserResponse
            {
                UserId = userId,
                Name = "AVB Agency",
                Email = "agency@example.com",
                Role = 4,
                Status = true
            });
        }

        public Task<bool> EmailExistsAsync(
            string email,
            int? excludingUserId = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(EmailExists);
        }

        public Task<AgencyUserResponse> CreateAsync(
            AgencyUserCreateModel model,
            CancellationToken cancellationToken = default)
        {
            CreatedModel = model;

            return Task.FromResult(new AgencyUserResponse
            {
                UserId = 1,
                Name = model.Name,
                Email = model.Email,
                Role = 4,
                Phone = model.Phone,
                Mobile = model.Mobile,
                Remarks = model.Remarks,
                Status = model.Status
            });
        }

        public Task<AgencyUserResponse?> UpdateAsync(
            int userId,
            AgencyUserUpdateModel model,
            CancellationToken cancellationToken = default)
        {
            UpdatedModel = model;

            return Task.FromResult<AgencyUserResponse?>(new AgencyUserResponse
            {
                UserId = userId,
                Name = model.Name,
                Email = model.Email,
                Role = 4,
                Phone = model.Phone,
                Mobile = model.Mobile,
                Remarks = model.Remarks,
                Status = model.Status
            });
        }
    }

    private sealed class FakePasswordService : IPasswordService
    {
        public UserAuthModel? LastUser { get; private set; }

        public string HashPassword(UserAuthModel user, string password)
        {
            LastUser = user;

            return $"hash:{password}";
        }

        public bool VerifyHashedPassword(UserAuthModel user, string passwordHash, string password)
        {
            return passwordHash == $"hash:{password}";
        }

        public bool VerifyLegacyPassword(string? legacyPassword, string password)
        {
            return legacyPassword == password;
        }
    }

    private sealed class FakeCityRepository : ICityRepository
    {
        public bool StateExists { get; set; } = true;

        public bool CityBelongsToState { get; set; } = true;

        public Task<IReadOnlyList<ArunVastra.Application.DTOs.Locations.CityResponse>> ListAsync(
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<ArunVastra.Application.DTOs.Locations.CityResponse>> ListByStateAsync(
            int stateId,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<ArunVastra.Application.DTOs.Locations.CityResponse?> GetByIdAsync(
            int cityId,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> StateExistsAsync(int stateId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(StateExists);
        }

        public Task<bool> CityBelongsToStateAsync(
            int cityId,
            int stateId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CityBelongsToState);
        }

        public Task<bool> NameExistsInStateAsync(
            int stateId,
            string cityName,
            int? excludingCityId = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<ArunVastra.Application.DTOs.Locations.CityResponse> CreateAsync(
            ArunVastra.Application.DTOs.Locations.CreateCityRequest request,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<ArunVastra.Application.DTOs.Locations.CityResponse?> UpdateAsync(
            int cityId,
            ArunVastra.Application.DTOs.Locations.UpdateCityRequest request,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
