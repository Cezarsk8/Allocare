namespace Allocore.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Allocore.Application.Abstractions.Persistence;
using Allocore.Application.Abstractions.Services;
using Allocore.Application.Features.Auth.Commands;
using Allocore.Infrastructure.Persistence;
using Allocore.Infrastructure.Persistence.Repositories;
using Allocore.Infrastructure.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));
        
        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IUserCompanyRepository, UserCompanyRepository>();
        services.AddScoped<IProviderRepository, ProviderRepository>();
        services.AddScoped<IContractRepository, ContractRepository>();
        services.AddScoped<INoteRepository, NoteRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());
        
        // Services
        services.Configure<JwtSettings>(options => configuration.GetSection("Jwt").Bind(options));
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IDateTime, DateTimeService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ICurrentUser, CurrentUserService>();
        
        return services;
    }
}
