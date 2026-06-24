using ContractManagement.Domain.Drivers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace ContractManagement.Infrastructure.Persistence.Configurations;
public sealed class DriverProfileConfiguration
    : IEntityTypeConfiguration<DriverProfile>
{
    public void Configure(
        EntityTypeBuilder<DriverProfile> builder)
    {
        builder.ToTable("DriverProfiles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(x => x.CitizenId)
            .HasMaxLength(20);

        builder.Property(x => x.Address)
            .HasMaxLength(500);

        builder.Property(x => x.AreaCode)
            .HasMaxLength(20);

        builder.Property(x => x.VehiclePlate)
            .HasMaxLength(20);

        builder.Property(x => x.VehicleCode)
            .HasMaxLength(30);

        builder.Property(x => x.VehicleType)
            .HasMaxLength(100);

        builder.Property(x => x.DriverLicenseNumber)
            .HasMaxLength(50);

        builder.Property(x => x.DriverLicenseClass)
            .HasMaxLength(20);

        builder.Property(x => x.AvatarUrl)
            .HasMaxLength(500);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasOne(x => x.User)
            .WithOne(x => x.DriverProfile)
            .HasForeignKey<DriverProfile>(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.UserId)
            .IsUnique()
            .HasDatabaseName("UX_DriverProfiles_UserId");

        builder.HasIndex(x => x.CitizenId)
            .HasFilter(
                "[CitizenId] IS NOT NULL AND [IsDeleted] = 0")
            .HasDatabaseName("IX_DriverProfiles_CitizenId");

        builder.HasIndex(x => x.VehiclePlate)
            .HasFilter(
                "[VehiclePlate] IS NOT NULL AND [IsDeleted] = 0")
            .HasDatabaseName("IX_DriverProfiles_VehiclePlate");

        builder.HasIndex(x => new
        {
            x.AreaCode,
            x.IsActive
        })
        .HasDatabaseName("IX_DriverProfiles_Area_Active");

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
