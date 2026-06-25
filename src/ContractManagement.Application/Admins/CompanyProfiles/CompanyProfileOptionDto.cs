namespace ContractManagement.Application.Admins.CompanyProfiles;

public sealed class CompanyProfileOptionDto
{
    public Guid Id { get; set; }

    public string CompanyName { get; set; } = string.Empty;

    public string TaxCode { get; set; } = string.Empty;

    public bool IsDefault { get; set; }
}