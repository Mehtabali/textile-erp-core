using ArunVastra.Application.DTOs.Users.Internal;
using ArunVastra.Application.Interfaces;
using ArunVastra.Application.Models;
using ArunVastra.Application.Models.Users;
using ArunVastra.Application.Services;

namespace ArunVastra.Application.Tests;

public sealed class InternalUserServiceTests
{
    [Fact]
    public async Task CreateAsync_WhenRequestIsValid_NormalizesAndCreatesInternalUser()
    {
        var repository = new FakeInternalUserRepository();
        var passwordService = new FakePasswordService();
        var service = new InternalUserService(repository, passwordService);

        var response = await service.CreateAsync(new CreateInternalUserRequest
        {
            Name = "  AVB Internal  ",
            Email = "  TEST@Example.COM ",
            Password = "321",
            ConfirmPassword = "321",
            Role = 5,
            Phone = " 011 ",
            Mobile = " 9999999999 ",
            Remarks = " Buyer desk ",
            Status = false
        });

        Assert.Equal(1, response.UserId);
        Assert.Equal("AVB Internal", response.Name);
        Assert.Equal("test@example.com", response.Email);
        Assert.Equal("hash:321", repository.CreatedModel?.PasswordHash);
        Assert.Equal(5, repository.CreatedModel?.Role);
        Assert.Equal("011", repository.CreatedModel?.Phone);
        Assert.Equal("9999999999", repository.CreatedModel?.Mobile);
        Assert.Equal("Buyer desk", repository.CreatedModel?.Remarks);
        Assert.False(repository.CreatedModel?.Status);
        Assert.Equal("test@example.com", passwordService.LastUser?.Email);
        Assert.Equal("5", passwordService.LastUser?.Role);
    }

    [Fact]
    public async Task CreateAsync_WhenConfirmPasswordDoesNotMatch_Throws()
    {
        var service = new InternalUserService(
            new FakeInternalUserRepository(),
            new FakePasswordService());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateInternalUserRequest
            {
                Name = "AVB Internal",
                Email = "test@example.com",
                Role = 6,
                Password = "321",
                ConfirmPassword = "123"
            }));

        Assert.Equal("Password and confirm password do not match.", ex.Message);
    }

    [Fact]
    public async Task CreateAsync_WhenEmailAlreadyExists_Throws()
    {
        var repository = new FakeInternalUserRepository
        {
            EmailExists = true
        };
        var service = new InternalUserService(repository, new FakePasswordService());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateInternalUserRequest
            {
                Name = "AVB Internal",
                Email = "test@example.com",
                Role = 6,
                Password = "321",
                ConfirmPassword = "321"
            }));

        Assert.Equal("Email already exists.", ex.Message);
        Assert.Null(repository.CreatedModel);
    }

    [Fact]
    public async Task UpdateAsync_WhenRequestIsValid_NormalizesAndUpdatesInternalUser()
    {
        var repository = new FakeInternalUserRepository();
        var service = new InternalUserService(repository, new FakePasswordService());

        var response = await service.UpdateAsync(
            7,
            new UpdateInternalUserRequest
            {
                Name = " Updated User ",
                Email = " UPDATED@Example.COM ",
                Role = 6,
                Phone = " 123 ",
                Mobile = " 456 ",
                Remarks = " Notes ",
                Status = false
            });

        Assert.NotNull(response);
        Assert.Equal(7, response.UserId);
        Assert.Equal("Updated User", repository.UpdatedModel?.Name);
        Assert.Equal("updated@example.com", repository.UpdatedModel?.Email);
        Assert.Equal(6, repository.UpdatedModel?.Role);
        Assert.Equal("123", repository.UpdatedModel?.Phone);
        Assert.Equal("456", repository.UpdatedModel?.Mobile);
        Assert.Equal("Notes", repository.UpdatedModel?.Remarks);
        Assert.False(repository.UpdatedModel?.Status);
    }

    [Fact]
    public async Task UpdateAsync_WhenEmailAlreadyExists_Throws()
    {
        var repository = new FakeInternalUserRepository
        {
            EmailExists = true
        };
        var service = new InternalUserService(repository, new FakePasswordService());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateAsync(
                7,
                new UpdateInternalUserRequest
                {
                    Name = "Updated User",
                    Email = "test@example.com",
                    Role = 6
                }));

        Assert.Equal("Email already exists.", ex.Message);
        Assert.Null(repository.UpdatedModel);
    }

    [Fact]
    public async Task CreateAsync_WhenRoleIsInvalid_Throws()
    {
        var service = new InternalUserService(
            new FakeInternalUserRepository(),
            new FakePasswordService());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateInternalUserRequest
            {
                Name = "AVB Internal",
                Email = "test@example.com",
                Password = "321",
                ConfirmPassword = "321",
                Role = 0
            }));

        Assert.Equal("Internal user type is invalid.", ex.Message);
    }

    [Fact]
    public async Task ListAsync_DelegatesToRepository()
    {
        var repository = new FakeInternalUserRepository();
        var service = new InternalUserService(repository, new FakePasswordService());

        var request = new InternalUserListRequest
        {
            SearchKeyword = "avb",
            IncludeLocked = false,
            PageNumber = 2,
            PageSize = 20,
            Filters = new InternalUserListFiltersRequest
            {
                Email = "admin"
            },
            Sort = new InternalUserListSortRequest
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
        var repository = new FakeInternalUserRepository();
        var service = new InternalUserService(repository, new FakePasswordService());

        var user = await service.GetByIdAsync(11);

        Assert.NotNull(user);
        Assert.Equal(11, repository.LastRequestedUserId);
    }

    private sealed class FakeInternalUserRepository : IInternalUserRepository
    {
        public bool EmailExists { get; set; }

        public InternalUserListRequest? LastListRequest { get; private set; }

        public int LastRequestedUserId { get; private set; }

        public InternalUserCreateModel? CreatedModel { get; private set; }

        public InternalUserUpdateModel? UpdatedModel { get; private set; }

        public Task<InternalUserListResponse> ListAsync(
            InternalUserListRequest request,
            CancellationToken cancellationToken = default)
        {
            LastListRequest = request;

            IReadOnlyList<InternalUserListItemResponse> users =
            [
                new InternalUserListItemResponse
                {
                    UserId = 1,
                    Name = "AVB",
                    Email = "avb@example.com",
                    Role = 6,
                    Phone = "011",
                    Status = true
                }
            ];

            return Task.FromResult(new InternalUserListResponse
            {
                Items = users,
                TotalRecords = users.Count,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            });
        }

        public Task<InternalUserResponse?> GetByIdAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            LastRequestedUserId = userId;

            return Task.FromResult<InternalUserResponse?>(new InternalUserResponse
            {
                UserId = userId,
                Name = "AVB",
                Email = "avb@example.com",
                Role = 6,
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

        public Task<InternalUserResponse> CreateAsync(
            InternalUserCreateModel model,
            CancellationToken cancellationToken = default)
        {
            CreatedModel = model;

            return Task.FromResult(new InternalUserResponse
            {
                UserId = 1,
                Name = model.Name,
                Email = model.Email,
                Role = model.Role,
                Phone = model.Phone,
                Mobile = model.Mobile,
                Remarks = model.Remarks,
                Status = model.Status
            });
        }

        public Task<InternalUserResponse?> UpdateAsync(
            int userId,
            InternalUserUpdateModel model,
            CancellationToken cancellationToken = default)
        {
            UpdatedModel = model;

            return Task.FromResult<InternalUserResponse?>(new InternalUserResponse
            {
                UserId = userId,
                Name = model.Name,
                Email = model.Email,
                Role = model.Role,
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
}
