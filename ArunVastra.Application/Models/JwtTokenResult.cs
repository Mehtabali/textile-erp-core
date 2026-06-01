namespace ArunVastra.Application.Models;

public sealed class JwtTokenResult
{
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }
}
