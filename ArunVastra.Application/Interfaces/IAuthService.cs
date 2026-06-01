using ArunVastra.Application.DTOs.Auth;

namespace ArunVastra.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<LoginResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);

    Task<AuthActionResponse> LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default);

    Task<AuthActionResponse> LogoutAllAsync(string userId, CancellationToken cancellationToken = default);
}
