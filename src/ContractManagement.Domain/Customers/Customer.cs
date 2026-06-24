using ContractManagement.Domain.Common;
using ContractManagement.Domain.Contracts;
using ContractManagement.Domain.Identity;
namespace ContractManagement.Domain.Customers;

public class Customer : BaseEntity
{
    public string FullName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string? CitizenId { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? Address { get; set; }

    public string? Email { get; set; }

    public string CreatedByDriverId { get; set; } = string.Empty;

    public DateTime? LastUsedAt { get; set; }

    public int ContractCount { get; set; }

    public ApplicationUser CreatedByDriver { get; set; } = null!;

    public ICollection<Contract> Contracts { get; set; }
        = new List<Contract>();
}
