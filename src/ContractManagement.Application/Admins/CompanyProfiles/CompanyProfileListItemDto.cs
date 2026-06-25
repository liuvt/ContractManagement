namespace ContractManagement.Application.Admins.CompanyProfiles;

public sealed class CompanyProfileListItemDto
{
    public Guid Id { get; set; }

    public string CompanyName { get; set; } = string.Empty;

    public string TaxCode { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public string RepresentativeName { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public bool IsDefault { get; set; }

    public int DriverCount { get; set; }

    public DateTime CreatedAt { get; set; }
}