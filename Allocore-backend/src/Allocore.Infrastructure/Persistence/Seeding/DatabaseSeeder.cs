namespace Allocore.Infrastructure.Persistence.Seeding;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Allocore.Domain.Entities.Users;
using Allocore.Domain.Entities.Companies;
using BCrypt.Net;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            // Apply pending migrations
            await context.Database.MigrateAsync();
            
            // Seed admin user if not exists
            if (!await context.Users.AnyAsync(u => u.Role == Role.Admin))
            {
                var adminPasswordHash = BCrypt.HashPassword("Admin@123!", workFactor: 12);
                var admin = User.Create(
                    "admin@allocore.com",
                    adminPasswordHash,
                    "Admin",
                    "User",
                    Role.Admin
                );
                
                await context.Users.AddAsync(admin);
                await context.SaveChangesAsync();
                
                logger.LogInformation("Admin user seeded: admin@allocore.com");
            }

            // Seed test company if not exists
            if (!await context.Companies.AnyAsync())
            {
                var testCompany = Company.Create(
                    name: "Test Company",
                    legalName: "Test Company LLC",
                    taxId: "12-3456789"
                );

                await context.Companies.AddAsync(testCompany);
                await context.SaveChangesAsync();

                // Link admin to test company as Owner
                var adminUser = await context.Users.FirstAsync(u => u.Email == "admin@allocore.com");
                var userCompany = UserCompany.Create(adminUser.Id, testCompany.Id, RoleInCompany.Owner);

                await context.UserCompanies.AddAsync(userCompany);
                await context.SaveChangesAsync();

                logger.LogInformation("Test company seeded and linked to admin user");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }
}
