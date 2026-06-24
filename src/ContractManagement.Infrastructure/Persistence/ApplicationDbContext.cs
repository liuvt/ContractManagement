using ContractManagement.Domain.Contracts;
using ContractManagement.Domain.Customers;
using ContractManagement.Domain.Drivers;
using ContractManagement.Domain.Identity;
using ContractManagement.Domain.Signatures;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ContractManagement.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<DriverProfile> DriverProfiles => Set<DriverProfile>();
    public DbSet<DriverSignature> DriverSignatures => Set<DriverSignature>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<ContractType> ContractTypes => Set<ContractType>();
    public DbSet<ContractTemplate> ContractTemplates => Set<ContractTemplate>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<ContractSignature> ContractSignatures => Set<ContractSignature>();
    public DbSet<ContractAttachment> ContractAttachments => Set<ContractAttachment>();
    public DbSet<ContractAuditLog> ContractAuditLogs => Set<ContractAuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(x => x.FullName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.EmployeeCode)
                .HasMaxLength(30);

            entity.Property(x => x.CreatedAt)
                .IsRequired();

            entity.HasIndex(x => x.EmployeeCode)
                .IsUnique()
                .HasFilter("[EmployeeCode] IS NOT NULL");

            entity.HasIndex(x => x.PhoneNumber)
                .HasFilter("[PhoneNumber] IS NOT NULL");

            entity.HasIndex(x => x.IsActive);
        });
    }
}
