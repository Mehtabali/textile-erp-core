namespace ArunVastra.Application.Models;

public sealed class RefreshTokenResult
{
    public string Token { get; set; } = string.Empty;

    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }
}
