using ContractManagement.Domain.Companies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContractManagement.Infrastructure.Persistence.Configurations;

public sealed class CompanyProfileConfiguration
    : IEntityTypeConfiguration<CompanyProfile>
{
    public void Configure(EntityTypeBuilder<CompanyProfile> builder)
    {
        builder.ToTable("CompanyProfiles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.CompanyName)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(x => x.TaxCode)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.BusinessLicenseNumber)
            .HasMaxLength(100);

        builder.Property(x => x.Address)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(x => x.Email)
            .HasMaxLength(256);

        builder.Property(x => x.RepresentativeName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.RepresentativePosition)
            .HasMaxLength(100);

        builder.Property(x => x.RepresentativeCitizenId)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.RepresentativeCitizenIdIssuedDate)
            .HasColumnType("date");

        builder.Property(x => x.RepresentativeCitizenIdIssuedPlace)
            .HasMaxLength(200);

        builder.Property(x => x.BankAccountNumber)
            .HasMaxLength(50);

        builder.Property(x => x.BankName)
            .HasMaxLength(200);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.IsDefault)
            .IsRequired();

        builder.Property(x => x.ManagedByUserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("datetime2");

        builder.HasIndex(x => x.TaxCode)
            .IsUnique();

        builder.HasIndex(x => x.ManagedByUserId);

        builder.HasIndex(x => x.IsActive);

        builder.HasIndex(x => new
        {
            x.ManagedByUserId,
            x.IsDefault
        });


        // 1 Tài khoản - n hồ sơ doanh nghiệp
        builder.HasOne(x => x.ManagedByUser)
            .WithMany(x => x.ManagedCompanies)
            .HasForeignKey(x => x.ManagedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}