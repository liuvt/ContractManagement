namespace ContractManagement.Application.Admins.DriverProfiles;

public sealed class DriverProfileFilter
{
    public string? Keyword { get; set; }

    public string? AreaCode { get; set; }

    public bool? IsActive { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
