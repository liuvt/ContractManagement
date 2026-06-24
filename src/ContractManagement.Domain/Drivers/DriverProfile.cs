using ContractManagement.Domain.Common;
using ContractManagement.Domain.Identity;

namespace ContractManagement.Domain.Drivers;
public class DriverProfile : BaseEntity
{
    public string UserId { get; set; } = string.Empty;

    public string? CitizenId { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? Address { get; set; }

    public string? AreaCode { get; set; }

    public string? VehiclePlate { get; set; }

    public string? VehicleCode { get; set; }

    public string? VehicleType { get; set; }

    public string? DriverLicenseNumber { get; set; }

    public string? DriverLicenseClass { get; set; }

    public DateTime? DriverLicenseIssuedDate { get; set; }

    public DateTime? DriverLicenseExpiryDate { get; set; }

    public string? AvatarUrl { get; set; }

    public string? CitizenIdFrontUrl { get; set; }

    public string? CitizenIdBackUrl { get; set; }

    public string? DriverLicenseFrontUrl { get; set; }

    public string? DriverLicenseBackUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public ApplicationUser User { get; set; } = null!;
}