namespace ContractManagement.Application.Admins.AdminAccounts;

public sealed class SaveCompanyProfileRequest
{
    public Guid? Id { get; set; }

    public string UserId { get; set; } = string.Empty;

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

    public bool IsActive { get; set; } = true;

    public bool IsDefault { get; set; }
}