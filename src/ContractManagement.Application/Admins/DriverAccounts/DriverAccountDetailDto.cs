using ContractManagement.Application.Admins.DriverProfiles;

namespace ContractManagement.Application.Admins.DriverAccounts;

public sealed class DriverAccountDetailDto
{
    public string UserId { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string? EmployeeCode { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public bool IsActive { get; set; }

    public bool MustChangePassword { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Null khi chưa có bản ghi DriverProfile.
    /// </summary>
    public DriverProfileDto? Profile { get; set; }

    /// <summary>
    /// Công ty quản lý của tài xế.
    /// Null khi chưa có hồ sơ hoặc chưa chọn công ty.
    /// </summary>
    public DriverCompanyProfileDto? Company { get; set; }
}