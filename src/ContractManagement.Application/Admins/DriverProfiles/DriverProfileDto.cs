using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContractManagement.Application.Admins.DriverProfiles;

public sealed class DriverProfileDto
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string DriverCode { get; set; } = string.Empty;

    // Thông tin cá nhân của tài xế

    public string FullName { get; set; } = string.Empty;

    public string? CitizenId { get; set; }
    public DateTime? CitizenIdIssuedDate { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? Address { get; set; }

    public string? AreaCode { get; set; }

    // Thông tin công ty quản lý tài xế
    public Guid CompanyProfileId { get; set; }

    public string CompanyName { get; set; } = string.Empty;

    public string CompanyTaxCode { get; set; } = string.Empty;

    // Thông tin phương tiện của tài xế

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

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
