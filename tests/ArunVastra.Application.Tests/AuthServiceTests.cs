using ArunVastra.Application.DTOs.Auth;
using ArunVastra.Application.Interfaces;
using ArunVastra.Application.Models;
using ArunVastra.Application.Services;

namespace ArunVastra.Application.Tests;

public sealed class AuthServiceTests
{
    [Fact]
    public async Task LoginAsync_WhenMigratedPasswordIsValid_ReturnsTokensAndUpdatesLastLogin()
    {
        var userRepository = new FakeUserRepository
        {
            UserByEmail = new UserAuthModel
            {
                UserId = "10",
                Email = "test@example.com",
                Role = "5",
                PasswordHash = "hash:321",
                PasswordMigrated = true
            }
        };
        var refreshTokenRepository = new FakeRefreshTokenRepository();
        var service = CreateService(userRepository, refreshTokenRepository);

        var response = await service.LoginAsync(new LoginRequest
        {
            Email = " test@example.com ",
            Password = "321"
        });

        Assert.True(response.Success);
        Assert.Equal("access-token-10", response.Token);
        Assert.Equal("refresh-token-10", response.RefreshToken);
        Assert.Equal("10", userRepository.LastLoginUserId);
        Assert.Single(refreshTokenRepository.AddedTokens);
        Assert.Equal("refresh-hash-10", refreshTokenRepository.AddedTokens[0].TokenHash);
    }

    [Fact]
    public async Task LoginAsync_WhenLegacyPasswordIsValid_MigratesPassword()
    {
        var userRepository = new FakeUserRepository
        {
            UserByEmail = new UserAuthModel
            {
                UserId = "10",
                Email = "test@example.com",
                Role = "5",
                LegacyPassword = "321",
                PasswordMigrated = false
            }
        };
        var service = CreateService(userRepository, new FakeRefreshTokenRepository());

        var response = await service.LoginAsync(new LoginRequest
        {
            Email = "test@example.com",
            Password = "321"
        });

        Assert.True(response.Success);
        Assert.Equal("10", userRepository.MigratedUserId);
        Assert.Equal("hash:321", userRepository.MigratedPasswordHash);
    }

    [Fact]
    public async Task LoginAsync_WhenEmailIsMissing_ReturnsFailure()
    {
        var service = CreateService(new FakeUserRepository(), new FakeRefreshTokenRepository());

        var response = await service.LoginAsync(new LoginRequest
        {
            Email = null!,
            Password = "321"
        });

        Assert.False(response.Success);
        Assert.Equal("Email and password are required.", response.Message);
    }

    [Fact]
    public async Task LoginAsync_WhenUserIsLocked_ReturnsInvalidCredentials()
    {
        var service = CreateService(
            new FakeUserRepository
            {
                UserByEmail = new UserAuthModel
                {
                    UserId = "10",
                    Email = "test@example.com",
                    PasswordHash = "hash:321",
                    PasswordMigrated = true,
                    Locked = true
                }
            },
            new FakeRefreshTokenRepository());

        var response = await service.LoginAsync(new LoginRequest
        {
            Email = "test@example.com",
            Password = "321"
        });

        Assert.False(response.Success);
        Assert.Equal("Invalid email or password.", response.Message);
    }

    [Fact]
    public async Task RefreshAsync_WhenTokenIsValid_RevokesOldTokenAndCreatesNewToken()
    {
        var refreshTokenRepository = new FakeRefreshTokenRepository
        {
            ActiveToken = new RefreshTokenModel
            {
                Id = 99,
                UserId = "10",
                User = new UserAuthModel
                {
                    UserId = "10",
                    Email = "test@example.com",
                    Role = "5"
                }
            }
        };
        var service = CreateService(new FakeUserRepository(), refreshTokenRepository);

        var response = await service.RefreshAsync(new RefreshTokenRequest
        {
            RefreshToken = "old-refresh"
        });

        Assert.True(response.Success);
        Assert.Equal(99, refreshTokenRepository.RevokedTokenId);
        Assert.Equal("access-token-10", response.Token);
        Assert.Equal("refresh-token-10", response.RefreshToken);
        Assert.Single(refreshTokenRepository.AddedTokens);
    }

    [Fact]
    public async Task LogoutAsync_WhenRefreshTokenIsActive_RevokesToken()
    {
        var refreshTokenRepository = new FakeRefreshTokenRepository
        {
            ActiveToken = new RefreshTokenModel { Id = 9, UserId = "10" }
        };
        var service = CreateService(new FakeUserRepository(), refreshTokenRepository);

        var response = await service.LogoutAsync(new LogoutRequest
        {
            RefreshToken = "refresh-token"
        });

        Assert.True(response.Success);
        Assert.Equal(9, refreshTokenRepository.RevokedTokenId);
    }

    [Fact]
    public async Task LogoutAllAsync_WhenUserExists_RevokesAllUserTokens()
    {
        var userRepository = new FakeUserRepository
        {
            UserById = new UserAuthModel { UserId = "10" }
        };
        var refreshTokenRepository = new FakeRefreshTokenRepository();
        var service = CreateService(userRepository, refreshTokenRepository);

        var response = await service.LogoutAllAsync("10");

        Assert.True(response.Success);
        Assert.Equal("10", refreshTokenRepository.RevokedAllUserId);
    }

    private static AuthService CreateService(
        FakeUserRepository userRepository,
        FakeRefreshTokenRepository refreshTokenRepository)
    {
        return new AuthService(
            userRepository,
            refreshTokenRepository,
            new FakePasswordService(),
            new FakeJwtTokenService(),
            new FakeRefreshTokenService());
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public UserAuthModel? UserByEmail { get; set; }

        public UserAuthModel? UserById { get; set; }

        public string? LastLoginUserId { get; private set; }

        public string? MigratedUserId { get; private set; }

        public string? MigratedPasswordHash { get; private set; }

        public Task<UserAuthModel?> GetByEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(UserByEmail);
        }

        public Task<UserAuthModel?> GetByIdAsync(
            string userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(UserById);
        }

        public Task UpdatePasswordMigrationAsync(
            string userId,
            string passwordHash,
            CancellationToken cancellationToken = default)
        {
            MigratedUserId = userId;
            MigratedPasswordHash = passwordHash;

            return Task.CompletedTask;
        }

        public Task UpdateLastLoginAsync(string userId, CancellationToken cancellationToken = default)
        {
            LastLoginUserId = userId;

            return Task.CompletedTask;
        }
    }

    private sealed class FakeRefreshTokenRepository : IRefreshTokenRepository
    {
        public RefreshTokenModel? ActiveToken { get; set; }

        public List<RefreshTokenModel> AddedTokens { get; } = [];

        public long? RevokedTokenId { get; private set; }

        public string? RevokedAllUserId { get; private set; }

        public Task AddAsync(RefreshTokenModel refreshToken, CancellationToken cancellationToken = default)
        {
            AddedTokens.Add(refreshToken);

            return Task.CompletedTask;
        }

        public Task<RefreshTokenModel?> GetActiveByTokenHashAsync(
            string tokenHash,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ActiveToken);
        }

        public Task RevokeAsync(long id, CancellationToken cancellationToken = default)
        {
            RevokedTokenId = id;

            return Task.CompletedTask;
        }

        public Task RevokeAllForUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            RevokedAllUserId = userId;

            return Task.CompletedTask;
        }
    }

    private sealed class FakePasswordService : IPasswordService
    {
        public string HashPassword(UserAuthModel user, string password)
        {
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

    private sealed class FakeJwtTokenService : IJwtTokenService
    {
        public JwtTokenResult GenerateToken(UserAuthModel user)
        {
            return new JwtTokenResult
            {
                Token = $"access-token-{user.UserId}",
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(60)
            };
        }
    }

    private sealed class FakeRefreshTokenService : IRefreshTokenService
    {
        public RefreshTokenResult GenerateRefreshToken(string userId)
        {
            return new RefreshTokenResult
            {
                Token = $"refresh-token-{userId}",
                TokenHash = $"refresh-hash-{userId}",
                ExpiresAtUtc = DateTime.UtcNow.AddDays(30)
            };
        }

        public string HashToken(string refreshToken)
        {
            return $"hash:{refreshToken}";
        }
    }
}
