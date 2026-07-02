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
        services.AddScoped<ISupplierUserService, SupplierUserService>();
        services.AddScoped<IAgencyUserService, AgencyUserService>();
        services.AddScoped<ITransportUserService, TransportUserService>();
        services.AddScoped<IStateService, StateService>();
        services.AddScoped<ICityService, CityService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IGstRuleService, GstRuleService>();
        services.AddScoped<IAdditionalChargeService, AdditionalChargeService>();
        services.AddScoped<ISaleVoucherService, SaleVoucherService>();
        services.AddScoped<ISupplierTransportMappingService, SupplierTransportMappingService>();
        services.AddScoped<ISupplierCategoryMappingService, SupplierCategoryMappingService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IInternalUserRepository, InternalUserRepository>();
        services.AddScoped<ISupplierUserRepository, SupplierUserRepository>();
        services.AddScoped<IAgencyUserRepository, AgencyUserRepository>();
        services.AddScoped<ITransportUserRepository, TransportUserRepository>();
        services.AddScoped<IStateRepository, StateRepository>();
        services.AddScoped<ICityRepository, CityRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IGstRuleRepository, GstRuleRepository>();
        services.AddScoped<IAdditionalChargeRepository, AdditionalChargeRepository>();
        services.AddScoped<ISaleVoucherRepository, SaleVoucherRepository>();
        services.AddScoped<ISupplierTransportMappingRepository, SupplierTransportMappingRepository>();
        services.AddScoped<ISupplierCategoryMappingRepository, SupplierCategoryMappingRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();

        return services;
    }
}
