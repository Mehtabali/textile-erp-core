using ArunVastra.Application.Interfaces;
using ArunVastra.Application.Services;
using ArunVastra.Infrastructure.Data;
using ArunVastra.Infrastructure.Repositories;
using ArunVastra.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArunVastra.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ArunVastraDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IInternalUserService, InternalUserService>();
        services.AddScoped<IAgencyUserService, AgencyUserService>();
        services.AddScoped<ITransportUserService, TransportUserService>();
        services.AddScoped<IStateService, StateService>();
        services.AddScoped<ICityService, CityService>();
        services.AddScoped<ISaleVoucherService, SaleVoucherService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IInternalUserRepository, InternalUserRepository>();
        services.AddScoped<IAgencyUserRepository, AgencyUserRepository>();
        services.AddScoped<ITransportUserRepository, TransportUserRepository>();
        services.AddScoped<IStateRepository, StateRepository>();
        services.AddScoped<ICityRepository, CityRepository>();
        services.AddScoped<ISaleVoucherRepository, SaleVoucherRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();

        return services;
    }
}
