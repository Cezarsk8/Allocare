namespace Allocore.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Allocore.Domain.Entities.Users;
using Allocore.Domain.Entities.Companies;
using Allocore.Domain.Entities.Providers;
using Allocore.Domain.Entities.Contracts;
using Allocore.Domain.Entities.Notes;
using Allocore.Domain.Entities.CostCenters;
using Allocore.Domain.Entities.Employees;
using Allocore.Application.Abstractions.Persistence;

public class ApplicationDbContext : DbContext, IUnitOfWork
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<UserCompany> UserCompanies => Set<UserCompany>();
    public DbSet<Provider> Providers => Set<Provider>();
    public DbSet<ProviderContact> ProviderContacts => Set<ProviderContact>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<ContractService> ContractServices => Set<ContractService>();
    public DbSet<Note> Notes => Set<Note>();
    public DbSet<CostCenter> CostCenters => Set<CostCenter>();
    public DbSet<Employee> Employees => Set<Employee>();
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
