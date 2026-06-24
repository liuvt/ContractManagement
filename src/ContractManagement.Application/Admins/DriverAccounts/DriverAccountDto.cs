namespace ContractManagement.Application.Admins.DriverAccounts;

public sealed class DriverAccountDto
{
    public string Id { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string? EmployeeCode { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public bool IsActive { get; set; }

    public bool MustChangePassword { get; set; }

    public bool HasDriverProfile { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
