namespace ArunVastra.Application.Models;

public sealed class RefreshTokenModel
{
    public long Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? RevokedAtUtc { get; set; }

    public UserAuthModel? User { get; set; }
}
