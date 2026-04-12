using Crm.Identity.Application.Interfaces;
using Crm.Identity.Application.Services;
using Crm.Identity.Domain.Entities;
using Crm.Identity.Domain.Interfaces;
using Crm.Identity.Infrastructure.Persistence;
using Crm.Identity.Infrastructure.Repositories;
using Crm.Identity.Infrastructure.Security;
using Crm.Shared.Domain;
using Crm.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crm.Identity.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<IdentityDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Identity")));

        services.AddScoped<BaseDbContext>(sp => sp.GetRequiredService<IdentityDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRepository<User>, Repository<User>>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<IdentityDbContext>());
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
