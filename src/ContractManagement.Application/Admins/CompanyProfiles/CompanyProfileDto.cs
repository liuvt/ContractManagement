namespace ContractManagement.Application.Admins.CompanyProfiles;

public sealed class CompanyProfileDto
{
    public Guid Id { get; set; }

    public string CompanyName { get; set; } = string.Empty;

    public string TaxCode { get; set; } = string.Empty;

    public string? BusinessLicenseNumber { get; set; }

    public string Address { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public string RepresentativeName { get; set; } = string.Empty;

    public string? RepresentativePosition { get; set; }

    public string RepresentativeCitizenId { get; set; } = string.Empty;

    public DateTime? RepresentativeCitizenIdIssuedDate { get; set; }

    public string? RepresentativeCitizenIdIssuedPlace { get; set; }

    public string? BankAccountNumber { get; set; }

    public string? BankName { get; set; }

    public bool IsActive { get; set; }

    public bool IsDefault { get; set; }

    public string ManagedByUserId { get; set; } = string.Empty;

    public string ManagedByUserName { get; set; } = string.Empty;

    public string ManagedByFullName { get; set; } = string.Empty;

    public int DriverCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}