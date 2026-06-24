using ContractManagement.Domain.Contracts;
using ContractManagement.Domain.Customers;
using ContractManagement.Domain.Drivers;
using ContractManagement.Domain.Signatures;
using Microsoft.AspNetCore.Identity;

namespace ContractManagement.Domain.Identity;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    public string? EmployeeCode { get; set; }

    public bool IsActive { get; set; } = true;

    public bool MustChangePassword { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public DriverProfile? DriverProfile { get; set; }

    public ICollection<DriverSignature> DriverSignatures { get; set; }
        = new List<DriverSignature>();

    public ICollection<Contract> Contracts { get; set; }
        = new List<Contract>();

    public ICollection<Customer> CreatedCustomers { get; set; }
        = new List<Customer>();
}
