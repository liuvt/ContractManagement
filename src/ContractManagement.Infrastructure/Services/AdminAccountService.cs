using ContractManagement.Application.Abstractions;
using ContractManagement.Application.Admins.AdminAccounts;
using ContractManagement.Application.Common;
using ContractManagement.Domain.Companies;
using ContractManagement.Domain.Identity;
using ContractManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ContractManagement.Infrastructure.Services;

public sealed class AdminAccountService(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    UserManager<ApplicationUser> userManager)
    : IAdminAccountService
{
    public async Task<IReadOnlyList<AdminAccountListItem>> GetAccountsAsync(
        string? keyword = null,
        CancellationToken cancellationToken = default)
    {
        await using var db =
            await dbFactory.CreateDbContextAsync(cancellationToken);

        var query = db.Users
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var value = keyword.Trim();

            query = query.Where(x =>
                (x.UserName != null && x.UserName.Contains(value)) ||
                x.FullName.Contains(value) ||
                (x.EmployeeCode != null && x.EmployeeCode.Contains(value)) ||
                (x.PhoneNumber != null && x.PhoneNumber.Contains(value)) ||
                (x.Email != null && x.Email.Contains(value)));
        }

        return await query
            .OrderBy(x => x.FullName)
            .Select(x => new AdminAccountListItem
            {
                Id = x.Id,
                UserName = x.UserName ?? string.Empty,
                FullName = x.FullName,
                EmployeeCode = x.EmployeeCode,
                PhoneNumber = x.PhoneNumber,
                Email = x.Email,
                IsActive = x.IsActive,
                MustChangePassword = x.MustChangePassword,

                CompanyCount = db.CompanyProfiles.Count(company =>
                    company.ManagedByUserId == x.Id)
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<AdminAccountDetail?> GetDetailAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return null;

        await using var db =
            await dbFactory.CreateDbContextAsync(cancellationToken);

        var user = await db.Users
            .AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => new AdminAccountDetail
            {
                Id = x.Id,
                UserName = x.UserName ?? string.Empty,
                FullName = x.FullName,
                EmployeeCode = x.EmployeeCode,
                PhoneNumber = x.PhoneNumber,
                Email = x.Email,
                IsActive = x.IsActive,
                MustChangePassword = x.MustChangePassword,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
            return null;

        user.Companies = await db.CompanyProfiles
            .AsNoTracking()
            .Where(x => x.ManagedByUserId == userId)
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.CompanyName)
            .Select(x => new CompanyProfileDto
            {
                Id = x.Id,
                CompanyName = x.CompanyName,
                TaxCode = x.TaxCode,
                BusinessLicenseNumber = x.BusinessLicenseNumber,
                Address = x.Address,
                PhoneNumber = x.PhoneNumber,
                Email = x.Email,
                RepresentativeName = x.RepresentativeName,
                RepresentativePosition = x.RepresentativePosition,
                RepresentativeCitizenId = x.RepresentativeCitizenId,

                RepresentativeCitizenIdIssuedDate =
                    x.RepresentativeCitizenIdIssuedDate,

                RepresentativeCitizenIdIssuedPlace =
                    x.RepresentativeCitizenIdIssuedPlace,

                BankAccountNumber = x.BankAccountNumber,
                BankName = x.BankName,
                IsActive = x.IsActive,
                IsDefault = x.IsDefault,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return user;
    }

    public async Task<ServiceResult> UpdateAccountAsync(
        UpdateAdminAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
            return ServiceResult.Failure("Thiếu mã tài khoản.");

        if (string.IsNullOrWhiteSpace(request.FullName))
            return ServiceResult.Failure("Vui lòng nhập họ và tên.");

        var user = await userManager.FindByIdAsync(request.UserId);

        if (user is null)
            return ServiceResult.Failure("Không tìm thấy tài khoản.");

        var employeeCode = NullIfEmpty(request.EmployeeCode);
        var phoneNumber = NullIfEmpty(request.PhoneNumber);
        var email = NullIfEmpty(request.Email);

        if (employeeCode is not null)
        {
            var duplicateEmployeeCode =
                await userManager.Users.AnyAsync(
                    x =>
                        x.Id != user.Id &&
                        x.EmployeeCode == employeeCode,
                    cancellationToken);

            if (duplicateEmployeeCode)
            {
                return ServiceResult.Failure(
                    "Mã nhân viên đã được sử dụng.");
            }
        }

        if (phoneNumber is not null)
        {
            var duplicatePhone =
                await userManager.Users.AnyAsync(
                    x =>
                        x.Id != user.Id &&
                        x.PhoneNumber == phoneNumber,
                    cancellationToken);

            if (duplicatePhone)
            {
                return ServiceResult.Failure(
                    "Số điện thoại đã được sử dụng.");
            }
        }

        if (email is not null)
        {
            var normalizedEmail =
                userManager.NormalizeEmail(email);

            var duplicateEmail =
                await userManager.Users.AnyAsync(
                    x =>
                        x.Id != user.Id &&
                        x.NormalizedEmail == normalizedEmail,
                    cancellationToken);

            if (duplicateEmail)
            {
                return ServiceResult.Failure(
                    "Email đã được sử dụng.");
            }
        }

        user.FullName = request.FullName.Trim();
        user.EmployeeCode = employeeCode;
        user.PhoneNumber = phoneNumber;
        user.IsActive = request.IsActive;
        user.MustChangePassword = request.MustChangePassword;
        user.UpdatedAt = DateTime.UtcNow;

        if (!string.Equals(
                user.Email,
                email,
                StringComparison.OrdinalIgnoreCase))
        {
            var setEmailResult =
                await userManager.SetEmailAsync(user, email);

            if (!setEmailResult.Succeeded)
            {
                return ServiceResult.Failure(
                    setEmailResult.Errors.Select(x => x.Description));
            }
        }

        var updateResult =
            await userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            return ServiceResult.Failure(
                updateResult.Errors.Select(x => x.Description));
        }

        return ServiceResult.Success(
            "Cập nhật tài khoản thành công.");
    }

    public async Task<ServiceResult> SaveCompanyProfileAsync(
        SaveCompanyProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationResult =
            ValidateCompanyRequest(request);

        if (!validationResult.Succeeded)
            return validationResult;

        await using var db =
            await dbFactory.CreateDbContextAsync(cancellationToken);

        var userExists = await db.Users
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == request.UserId,
                cancellationToken);

        if (!userExists)
        {
            return ServiceResult.Failure(
                "Không tìm thấy tài khoản quản lý.");
        }

        var taxCode = request.TaxCode.Trim();

        var duplicateTaxCode =
            await db.CompanyProfiles.AnyAsync(
                x =>
                    x.TaxCode == taxCode &&
                    (!request.Id.HasValue ||
                     x.Id != request.Id.Value),
                cancellationToken);

        if (duplicateTaxCode)
        {
            return ServiceResult.Failure(
                "Mã số thuế đã tồn tại.");
        }

        CompanyProfile company;
        var isCreating = !request.Id.HasValue;

        if (request.Id.HasValue)
        {
            company = await db.CompanyProfiles
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == request.Id.Value &&
                        x.ManagedByUserId == request.UserId,
                    cancellationToken)
                ?? throw new InvalidOperationException(
                    "Không tìm thấy hồ sơ doanh nghiệp.");
        }
        else
        {
            company = new CompanyProfile
            {
                Id = Guid.NewGuid(),
                ManagedByUserId = request.UserId,
                CreatedAt = DateTime.UtcNow
            };

            db.CompanyProfiles.Add(company);
        }

        if (request.IsDefault)
        {
            var defaultProfiles =
                await db.CompanyProfiles
                    .Where(x =>
                        x.ManagedByUserId == request.UserId &&
                        x.Id != company.Id &&
                        x.IsDefault)
                    .ToListAsync(cancellationToken);

            foreach (var item in defaultProfiles)
            {
                item.IsDefault = false;
                item.UpdatedAt = DateTime.UtcNow;
            }
        }
        else
        {
            var hasOtherDefault =
                await db.CompanyProfiles.AnyAsync(
                    x =>
                        x.ManagedByUserId == request.UserId &&
                        x.Id != company.Id &&
                        x.IsDefault,
                    cancellationToken);

            if (!hasOtherDefault)
                request.IsDefault = true;
        }

        MapCompany(request, company);

        await db.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success(
            isCreating
                ? "Thêm hồ sơ doanh nghiệp thành công."
                : "Cập nhật hồ sơ doanh nghiệp thành công.");
    }

    public async Task<ServiceResult> UpdateAccountAndProfileAsync(
        UpdateAdminAccountAndProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Account is null)
        {
            return ServiceResult.Failure(
                "Thiếu dữ liệu tài khoản.");
        }

        var updateAccountResult =
            await UpdateAccountAsync(
                request.Account,
                cancellationToken);

        if (!updateAccountResult.Succeeded)
            return updateAccountResult;

        if (request.Profile is null)
        {
            return ServiceResult.Success(
                "Cập nhật tài khoản thành công.");
        }

        request.Profile.UserId =
            request.Account.UserId;

        var updateProfileResult =
            await SaveCompanyProfileAsync(
                request.Profile,
                cancellationToken);

        if (!updateProfileResult.Succeeded)
        {
            return ServiceResult.Failure(
                new[]
                {
                    "Tài khoản đã được cập nhật, nhưng hồ sơ doanh nghiệp chưa cập nhật thành công."
                }
                .Concat(updateProfileResult.Errors));
        }

        return ServiceResult.Success(
            "Cập nhật tài khoản và hồ sơ doanh nghiệp thành công.");
    }

    public async Task<ServiceResult> SetDefaultCompanyAsync(
        string userId,
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        await using var db =
            await dbFactory.CreateDbContextAsync(cancellationToken);

        var companies = await db.CompanyProfiles
            .Where(x => x.ManagedByUserId == userId)
            .ToListAsync(cancellationToken);

        if (companies.Count == 0 ||
            companies.All(x => x.Id != companyId))
        {
            return ServiceResult.Failure(
                "Không tìm thấy hồ sơ doanh nghiệp.");
        }

        foreach (var company in companies)
        {
            company.IsDefault = company.Id == companyId;
            company.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success(
            "Đã cập nhật hồ sơ doanh nghiệp mặc định.");
    }

    public async Task<ServiceResult> DeleteCompanyProfileAsync(
        string userId,
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        await using var db =
            await dbFactory.CreateDbContextAsync(cancellationToken);

        var company = await db.CompanyProfiles
            .FirstOrDefaultAsync(
                x =>
                    x.Id == companyId &&
                    x.ManagedByUserId == userId,
                cancellationToken);

        if (company is null)
        {
            return ServiceResult.Failure(
                "Không tìm thấy hồ sơ doanh nghiệp.");
        }

        var wasDefault = company.IsDefault;

        db.CompanyProfiles.Remove(company);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return ServiceResult.Failure(
                "Không thể xóa hồ sơ vì đang được hợp đồng hoặc dữ liệu khác sử dụng.");
        }

        if (wasDefault)
        {
            var nextCompany = await db.CompanyProfiles
                .Where(x =>
                    x.ManagedByUserId == userId &&
                    x.IsActive)
                .OrderBy(x => x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (nextCompany is not null)
            {
                nextCompany.IsDefault = true;
                nextCompany.UpdatedAt = DateTime.UtcNow;

                await db.SaveChangesAsync(cancellationToken);
            }
        }

        return ServiceResult.Success(
            "Đã xóa hồ sơ doanh nghiệp.");
    }

    private static void MapCompany(
        SaveCompanyProfileRequest request,
        CompanyProfile company)
    {
        company.CompanyName =
            request.CompanyName.Trim();

        company.TaxCode =
            request.TaxCode.Trim();

        company.BusinessLicenseNumber =
            NullIfEmpty(request.BusinessLicenseNumber);

        company.Address =
            request.Address.Trim();

        company.PhoneNumber =
            NullIfEmpty(request.PhoneNumber);

        company.Email =
            NullIfEmpty(request.Email);

        company.RepresentativeName =
            request.RepresentativeName.Trim();

        company.RepresentativePosition =
            NullIfEmpty(request.RepresentativePosition);

        company.RepresentativeCitizenId =
            request.RepresentativeCitizenId.Trim();

        company.RepresentativeCitizenIdIssuedDate =
            request.RepresentativeCitizenIdIssuedDate;

        company.RepresentativeCitizenIdIssuedPlace =
            NullIfEmpty(
                request.RepresentativeCitizenIdIssuedPlace);

        company.BankAccountNumber =
            NullIfEmpty(request.BankAccountNumber);

        company.BankName =
            NullIfEmpty(request.BankName);

        company.IsActive =
            request.IsActive;

        company.IsDefault =
            request.IsDefault;

        company.UpdatedAt =
            DateTime.UtcNow;
    }

    private static ServiceResult ValidateCompanyRequest(
        SaveCompanyProfileRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
            return ServiceResult.Failure("Thiếu tài khoản quản lý.");

        if (string.IsNullOrWhiteSpace(request.CompanyName))
            return ServiceResult.Failure("Vui lòng nhập tên doanh nghiệp.");

        if (string.IsNullOrWhiteSpace(request.TaxCode))
            return ServiceResult.Failure("Vui lòng nhập mã số thuế.");

        if (string.IsNullOrWhiteSpace(request.Address))
            return ServiceResult.Failure("Vui lòng nhập địa chỉ.");

        if (string.IsNullOrWhiteSpace(request.RepresentativeName))
            return ServiceResult.Failure("Vui lòng nhập người đại diện.");

        if (string.IsNullOrWhiteSpace(
                request.RepresentativeCitizenId))
        {
            return ServiceResult.Failure(
                "Vui lòng nhập CCCD người đại diện.");
        }

        return ServiceResult.Success();
    }

    private static string? NullIfEmpty(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}