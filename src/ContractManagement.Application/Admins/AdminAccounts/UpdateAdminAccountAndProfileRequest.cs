namespace ContractManagement.Application.Admins.AdminAccounts;

public sealed class UpdateAdminAccountAndProfileRequest
{
    public UpdateAdminAccountRequest Account { get; set; } = new();

    public SaveCompanyProfileRequest? Profile { get; set; }
}
