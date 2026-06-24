namespace ContractManagement.Application.Admins.DriverAccounts;

public sealed class UpdateDriverAccountRequest
{
    public string FullName { get; set; } = string.Empty;

    public string? EmployeeCode { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public bool IsActive { get; set; } = true;

    public bool MustChangePassword { get; set; }
}
