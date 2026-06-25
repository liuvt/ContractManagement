using ContractManagement.Application.Admins.AdminAccounts;
using ContractManagement.Application.Common;

namespace ContractManagement.Application.Abstractions;

public interface IAdminAccountService
{
    Task<IReadOnlyList<AdminAccountListItem>> GetAccountsAsync(
        string? keyword = null,
        CancellationToken cancellationToken = default);

    Task<AdminAccountDetail?> GetDetailAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult> UpdateAccountAsync(
        UpdateAdminAccountRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult> SaveCompanyProfileAsync(
        SaveCompanyProfileRequest request,
        CancellationToken cancellationToken = default);

    // Cập nhật thông tin tài khoản và thông tin công ty cùng lúc
    Task<ServiceResult> UpdateAccountAndProfileAsync(
        UpdateAdminAccountAndProfileRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult> SetDefaultCompanyAsync(
        string userId,
        Guid companyId,
        CancellationToken cancellationToken = default);

    Task<ServiceResult> DeleteCompanyProfileAsync(
        string userId,
        Guid companyId,
        CancellationToken cancellationToken = default);
}