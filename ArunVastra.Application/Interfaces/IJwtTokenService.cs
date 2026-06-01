using ArunVastra.Application.Models;

namespace ArunVastra.Application.Interfaces;

public interface IJwtTokenService
{
    JwtTokenResult GenerateToken(UserAuthModel user);
}
