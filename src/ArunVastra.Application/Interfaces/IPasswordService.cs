using ArunVastra.Application.Models;

namespace ArunVastra.Application.Interfaces;

public interface IPasswordService
{
    string HashPassword(UserAuthModel user, string password);

    bool VerifyHashedPassword(UserAuthModel user, string passwordHash, string password);

    bool VerifyLegacyPassword(string? legacyPassword, string password);
}
