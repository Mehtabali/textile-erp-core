using ArunVastra.Application.Interfaces;
using ArunVastra.Application.Models;
using Microsoft.AspNetCore.Identity;

namespace ArunVastra.Infrastructure.Security;

public sealed class PasswordService : IPasswordService
{
    private readonly PasswordHasher<UserAuthModel> _passwordHasher = new();

    public string HashPassword(UserAuthModel user, string password)
    {
        return _passwordHasher.HashPassword(user, password);
    }

    public bool VerifyHashedPassword(UserAuthModel user, string passwordHash, string password)
    {
        var result = _passwordHasher.VerifyHashedPassword(user, passwordHash, password);

        return result is PasswordVerificationResult.Success
            or PasswordVerificationResult.SuccessRehashNeeded;
    }

    public bool VerifyLegacyPassword(string? legacyPassword, string password)
    {
        return string.Equals(legacyPassword, password, StringComparison.Ordinal);
    }
}
