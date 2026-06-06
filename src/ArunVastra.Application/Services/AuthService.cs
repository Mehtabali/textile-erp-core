using ArunVastra.Application.DTOs.Auth;
using ArunVastra.Application.Interfaces;
using ArunVastra.Application.Models;

namespace ArunVastra.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;

    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordService passwordService,
        IJwtTokenService jwtTokenService,
        IRefreshTokenService refreshTokenService)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordService = passwordService;
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Failed("Email and password are required.");
        }

        var email = request.Email.Trim();

        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

        if (user is null || user.Locked)
        {
            return Failed("Invalid email or password.");
        }

        var isPasswordValid = user.PasswordMigrated
            ? VerifyMigratedPassword(user, request.Password)
            : await VerifyAndMigrateLegacyPasswordAsync(user, request.Password, cancellationToken);

        if (!isPasswordValid)
        {
            return Failed("Invalid email or password.");
        }

        await _userRepository.UpdateLastLoginAsync(user.UserId, cancellationToken);

        var token = _jwtTokenService.GenerateToken(user);
        var refreshToken = await CreateRefreshTokenAsync(user.UserId, cancellationToken);

        return new LoginResponse
        {
            Success = true,
            Token = token.Token,
            ExpiresAtUtc = token.ExpiresAtUtc,
            RefreshToken = refreshToken.Token,
            RefreshTokenExpiresAtUtc = refreshToken.ExpiresAtUtc,
            PasswordResetRequired = user.PasswordResetRequired,
            User = new AuthenticatedUserDto
            {
                UserId = user.UserId,
                Email = user.Email,
                Role = user.Role
            }
        };
    }

    public async Task<LoginResponse> RefreshAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Failed("Refresh token is required.");
        }

        var refreshTokenHash = _refreshTokenService.HashToken(request.RefreshToken);
        var storedRefreshToken = await _refreshTokenRepository.GetActiveByTokenHashAsync(
            refreshTokenHash,
            cancellationToken);

        if (storedRefreshToken?.User is null)
        {
            return Failed("Invalid refresh token.");
        }

        await _refreshTokenRepository.RevokeAsync(storedRefreshToken.Id, cancellationToken);

        var accessToken = _jwtTokenService.GenerateToken(storedRefreshToken.User);
        var newRefreshToken = await CreateRefreshTokenAsync(storedRefreshToken.User.UserId, cancellationToken);

        return new LoginResponse
        {
            Success = true,
            Token = accessToken.Token,
            ExpiresAtUtc = accessToken.ExpiresAtUtc,
            RefreshToken = newRefreshToken.Token,
            RefreshTokenExpiresAtUtc = newRefreshToken.ExpiresAtUtc,
            PasswordResetRequired = storedRefreshToken.User.PasswordResetRequired,
            User = new AuthenticatedUserDto
            {
                UserId = storedRefreshToken.User.UserId,
                Email = storedRefreshToken.User.Email,
                Role = storedRefreshToken.User.Role
            }
        };
    }

    public async Task<AuthActionResponse> LogoutAsync(
        LogoutRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return ActionFailed("Refresh token is required.");
        }

        var refreshTokenHash = _refreshTokenService.HashToken(request.RefreshToken);
        var storedRefreshToken = await _refreshTokenRepository.GetActiveByTokenHashAsync(
            refreshTokenHash,
            cancellationToken);

        if (storedRefreshToken is not null)
        {
            await _refreshTokenRepository.RevokeAsync(storedRefreshToken.Id, cancellationToken);
        }

        return new AuthActionResponse
        {
            Success = true,
            Message = "Logged out successfully."
        };
    }

    public async Task<AuthActionResponse> LogoutAllAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return ActionFailed("User not found.");
        }

        await _refreshTokenRepository.RevokeAllForUserAsync(user.UserId, cancellationToken);

        return new AuthActionResponse
        {
            Success = true,
            Message = "Logged out from all devices successfully."
        };
    }

    private bool VerifyMigratedPassword(UserAuthModel user, string password)
    {
        return !string.IsNullOrWhiteSpace(user.PasswordHash)
            && _passwordService.VerifyHashedPassword(user, user.PasswordHash, password);
    }

    private async Task<bool> VerifyAndMigrateLegacyPasswordAsync(
        UserAuthModel user,
        string password,
        CancellationToken cancellationToken)
    {
        if (!_passwordService.VerifyLegacyPassword(user.LegacyPassword, password))
        {
            return false;
        }

        var passwordHash = _passwordService.HashPassword(user, password);

        await _userRepository.UpdatePasswordMigrationAsync(
            user.UserId,
            passwordHash,
            cancellationToken);

        user.PasswordHash = passwordHash;
        user.PasswordMigrated = true;

        return true;
    }

    private async Task<RefreshTokenResult> CreateRefreshTokenAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        var refreshToken = _refreshTokenService.GenerateRefreshToken(userId);

        await _refreshTokenRepository.AddAsync(
            new RefreshTokenModel
            {
                UserId = userId,
                TokenHash = refreshToken.TokenHash,
                ExpiresAtUtc = refreshToken.ExpiresAtUtc,
                CreatedAtUtc = DateTime.UtcNow
            },
            cancellationToken);

        return refreshToken;
    }

    private static LoginResponse Failed(string message)
    {
        return new LoginResponse
        {
            Success = false,
            Message = message
        };
    }

    private static AuthActionResponse ActionFailed(string message)
    {
        return new AuthActionResponse
        {
            Success = false,
            Message = message
        };
    }
}
