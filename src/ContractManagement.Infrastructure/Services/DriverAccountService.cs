using ContractManagement.Application.Abstractions;
using ContractManagement.Application.Admins.DriverAccounts;
using ContractManagement.Application.Admins.DriverProfiles;
using ContractManagement.Domain.Drivers;
using ContractManagement.Domain.Identity;
using ContractManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ContractManagement.Infrastructure.Services;

public sealed class DriverAccountService : IDriverAccountService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    public DriverAccountService(
        UserManager<ApplicationUser> userManager,
        IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _userManager = userManager;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<string> CreateAsync(
    CreateDriverAccountRequest request,
    CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        var userName = request.UserName.Trim();

        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new InvalidOperationException(
                "Tên đăng nhập không được để trống.");
        }

        if (request.CompanyProfileId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "Vui lòng chọn công ty quản lý tài xế.");
        }

        var companyExists =
            await dbContext.CompanyProfiles
                .AsNoTracking()
                .AnyAsync(
                    x =>
                        x.Id == request.CompanyProfileId &&
                        x.IsActive,
                    cancellationToken);

        if (!companyExists)
        {
            throw new InvalidOperationException(
                "Công ty không tồn tại hoặc đã ngừng hoạt động.");
        }

        var normalizedUserName =
            _userManager.NormalizeName(userName);

        var existingUser =
            await dbContext.Users
                .AsNoTracking()
                .AnyAsync(
                    x => x.NormalizedUserName == normalizedUserName,
                    cancellationToken);

        if (existingUser)
        {
            throw new InvalidOperationException(
                "Tên đăng nhập đã tồn tại.");
        }

        var employeeCode =
            NullIfEmpty(request.EmployeeCode);

        if (employeeCode is not null)
        {
            var employeeCodeExists =
                await dbContext.Users.AnyAsync(
                    x => x.EmployeeCode == employeeCode,
                    cancellationToken);

            if (employeeCodeExists)
            {
                throw new InvalidOperationException(
                    "Mã nhân viên đã tồn tại.");
            }
        }

        var phoneNumber =
            NullIfEmpty(request.PhoneNumber);

        if (phoneNumber is not null)
        {
            var phoneExists =
                await dbContext.Users.AnyAsync(
                    x => x.PhoneNumber == phoneNumber,
                    cancellationToken);

            if (phoneExists)
            {
                throw new InvalidOperationException(
                    "Số điện thoại đã được sử dụng.");
            }
        }

        var user = new ApplicationUser
        {
            UserName = userName,
            FullName = request.FullName.Trim(),
            EmployeeCode = employeeCode,
            PhoneNumber = phoneNumber,
            Email = NullIfEmpty(request.Email),
            IsActive = true,
            MustChangePassword = request.MustChangePassword,
            CreatedAt = DateTime.UtcNow
        };

        var createResult =
            await _userManager.CreateAsync(
                user,
                request.Password);

        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(
                    "; ",
                    createResult.Errors.Select(
                        x => x.Description)));
        }

        try
        {
            var roleResult =
                await _userManager.AddToRoleAsync(
                    user,
                    "Driver");

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(
                        "; ",
                        roleResult.Errors.Select(
                            x => x.Description)));
            }

            /*
             * UserManager sử dụng DbContext riêng.
             * Sau khi user được tạo, tạo DriverProfile bằng factory context.
             */
            dbContext.DriverProfiles.Add(
                new DriverProfile
                {
                    UserId = user.Id,
                    CompanyProfileId = request.CompanyProfileId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });

            await dbContext.SaveChangesAsync(
                cancellationToken);

            return user.Id;
        }
        catch
        {
            /*
             * Nếu tạo role hoặc profile thất bại,
             * xóa tài khoản vừa tạo để không sinh dữ liệu rác.
             */
            await _userManager.DeleteAsync(user);
            throw;
        }
    }

    private static string? NullIfEmpty(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    public async Task UpdateAsync(
        string userId,
        UpdateDriverAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var user =
            await _userManager.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException(
                "Không tìm thấy tài khoản tài xế.");

        if (!string.IsNullOrWhiteSpace(request.EmployeeCode))
        {
            var employeeCodeExists =
                await dbContext.Users.AnyAsync(
                    x =>
                        x.Id != userId &&
                        x.EmployeeCode == request.EmployeeCode,
                    cancellationToken);

            if (employeeCodeExists)
            {
                throw new InvalidOperationException(
                    "Mã nhân viên đã tồn tại.");
            }
        }

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            var phoneExists =
                await dbContext.Users.AnyAsync(
                    x =>
                        x.Id != userId &&
                        x.PhoneNumber == request.PhoneNumber,
                    cancellationToken);

            if (phoneExists)
            {
                throw new InvalidOperationException(
                    "Số điện thoại đã được sử dụng.");
            }
        }

        user.FullName = request.FullName.Trim();
        user.EmployeeCode = request.EmployeeCode?.Trim();
        user.PhoneNumber = request.PhoneNumber?.Trim();
        user.Email = request.Email?.Trim();
        user.IsActive = request.IsActive;
        user.MustChangePassword = request.MustChangePassword;
        user.UpdatedAt = DateTime.UtcNow;

        var result =
            await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(
                    "; ",
                    result.Errors.Select(
                        x => x.Description)));
        }
    }

    // Khóa/mở tài khoản tài xế, đồng thời cập nhật trạng thái profile nếu có
    public async Task SetActiveAsync(
    string userId,
    bool isActive,
    CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            var user = await dbContext.Users
                .FirstOrDefaultAsync(
                    x => x.Id == userId,
                    cancellationToken)
                ?? throw new KeyNotFoundException(
                    "Không tìm thấy tài khoản tài xế.");

            var profile = await dbContext.DriverProfiles
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(
                    x => x.UserId == userId,
                    cancellationToken);

            user.IsActive = isActive;
            user.UpdatedAt = DateTime.UtcNow;

            if (isActive)
            {
                user.LockoutEnabled = true;
                user.LockoutEnd = null;

                if (profile is not null && !profile.IsDeleted)
                {
                    profile.IsActive = true;
                    profile.UpdatedAt = DateTime.UtcNow;
                }
            }
            else
            {
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.MaxValue;

                if (profile is not null)
                {
                    profile.IsActive = false;
                    profile.UpdatedAt = DateTime.UtcNow;
                }
            }

            await dbContext.SaveChangesAsync(
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(
                cancellationToken);

            throw;
        }
    }

    // Đặt lại mật khẩu cho tài khoản tài xế, bắt buộc đổi mật khẩu khi đăng nhập lần sau
    public async Task ResetPasswordAsync(
    string userId,
    string newPassword,
    CancellationToken cancellationToken = default)
    {
        var user =
            await _userManager.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException(
                "Không tìm thấy tài khoản.");

        var token =
            await _userManager
                .GeneratePasswordResetTokenAsync(user);

        var result =
            await _userManager.ResetPasswordAsync(
                user,
                token,
                newPassword);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(
                    "; ",
                    result.Errors.Select(
                        x => x.Description)));
        }

        user.MustChangePassword = true;
        user.UpdatedAt = DateTime.UtcNow;

        var updateResult =
            await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(
                    "; ",
                    updateResult.Errors.Select(
                        x => x.Description)));
        }
    }


    // Xóa tài khoản tài xế, nếu đã phát sinh dữ liệu nghiệp vụ thì chỉ vô hiệu hóa
    public async Task DeleteAsync(
    string userId,
    CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            var user = await dbContext.Users
                .FirstOrDefaultAsync(
                    x => x.Id == userId,
                    cancellationToken)
                ?? throw new KeyNotFoundException(
                    "Không tìm thấy tài khoản tài xế.");

            var profile = await dbContext.DriverProfiles
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(
                    x => x.UserId == userId,
                    cancellationToken);

            var hasContracts =
                await dbContext.Contracts
                    .IgnoreQueryFilters()
                    .AnyAsync(
                        x => x.DriverId == userId,
                        cancellationToken);

            var hasSignatures =
                await dbContext.DriverSignatures
                    .IgnoreQueryFilters()
                    .AnyAsync(
                        x => x.DriverId == userId,
                        cancellationToken);

            if (hasContracts || hasSignatures)
            {
                // Không xóa vật lý vì đã có dữ liệu nghiệp vụ.
                user.IsActive = false;
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.MaxValue;
                user.UpdatedAt = DateTime.UtcNow;

                if (profile is not null)
                {
                    profile.IsActive = false;
                    profile.IsDeleted = true;
                    profile.DeletedAt = DateTime.UtcNow;
                    profile.UpdatedAt = DateTime.UtcNow;
                }

                await dbContext.SaveChangesAsync(
                    cancellationToken);

                await transaction.CommitAsync(
                    cancellationToken);

                return;
            }

            // Chưa phát sinh dữ liệu: xóa profile trước.
            if (profile is not null)
            {
                dbContext.DriverProfiles.Remove(profile);

                await dbContext.SaveChangesAsync(
                    cancellationToken);
            }

            var deleteResult =
                await _userManager.DeleteAsync(user);

            if (!deleteResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(
                        "; ",
                        deleteResult.Errors.Select(
                            x => x.Description)));
            }

            await transaction.CommitAsync(
                cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(
                cancellationToken);

            throw;
        }
    }

    public async Task<DriverAccountDto?> GetByIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.Users
            .AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => new DriverAccountDto
            {
                Id = x.Id,
                UserName = x.UserName ?? string.Empty,
                FullName = x.FullName,
                EmployeeCode = x.EmployeeCode,
                PhoneNumber = x.PhoneNumber,
                Email = x.Email,
                IsActive = x.IsActive,
                MustChangePassword = x.MustChangePassword,
                HasDriverProfile =
                    dbContext.DriverProfiles.Any(
                        profile => profile.UserId == x.Id),
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DriverAccountDto>>
        GetListAsync(
            DriverAccountFilter filter,
            CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var page = Math.Max(filter.Page, 1);

        var pageSize = Math.Clamp(
            filter.PageSize,
            1,
            100);

        var driverUserIds =
            dbContext.UserRoles
                .Where(userRole =>
                    dbContext.Roles.Any(role =>
                        role.Id == userRole.RoleId &&
                        role.Name == "Driver"))
                .Select(x => x.UserId);

        var query =
            dbContext.Users
                .AsNoTracking()
                .Where(x => driverUserIds.Contains(x.Id));

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword = filter.Keyword.Trim();

            query = query.Where(x =>
                (x.UserName != null &&
                 x.UserName.Contains(keyword)) ||
                x.FullName.Contains(keyword) ||
                (x.EmployeeCode != null &&
                 x.EmployeeCode.Contains(keyword)) ||
                (x.PhoneNumber != null &&
                 x.PhoneNumber.Contains(keyword)));
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(
                x => x.IsActive == filter.IsActive.Value);
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new DriverAccountDto
            {
                Id = x.Id,
                UserName = x.UserName ?? string.Empty,
                FullName = x.FullName,
                EmployeeCode = x.EmployeeCode,
                PhoneNumber = x.PhoneNumber,
                Email = x.Email,
                IsActive = x.IsActive,
                MustChangePassword = x.MustChangePassword,
                HasDriverProfile =
                    dbContext.DriverProfiles.Any(
                        profile => profile.UserId == x.Id),
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<DriverAccountDetailDto?> GetDetailAsync(
    string userId,
    CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return null;

        await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        var detail = await dbContext.Users
            .AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => new DriverAccountDetailDto
            {
                UserId = user.Id,
                UserName = user.UserName ?? string.Empty,
                FullName = user.FullName,
                EmployeeCode = user.EmployeeCode,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                IsActive = user.IsActive,
                MustChangePassword = user.MustChangePassword,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,

                /*
                 * Quan trọng:
                 * Không có DriverProfile thì phải trả Profile = null.
                 */
                Profile = user.DriverProfile == null
                    ? null
                    : new DriverProfileDto
                    {
                        Id = user.DriverProfile.Id,
                        UserId = user.DriverProfile.UserId,

                        UserName =
                            user.UserName ?? string.Empty,

                        DriverCode =
                            user.EmployeeCode ?? string.Empty,

                        FullName =
                            user.FullName,

                        CitizenId =
                            user.DriverProfile.CitizenId,

                        CitizenIdIssuedDate =
                            user.DriverProfile.CitizenIdIssuedDate,

                        DateOfBirth =
                            user.DriverProfile.DateOfBirth,

                        Address =
                            user.DriverProfile.Address,

                        AreaCode =
                            user.DriverProfile.AreaCode,

                        CompanyProfileId =
                            user.DriverProfile.CompanyProfileId,

                        CompanyName =
                            user.DriverProfile.CompanyProfile.CompanyName,

                        CompanyTaxCode =
                            user.DriverProfile.CompanyProfile.TaxCode,

                        VehiclePlate =
                            user.DriverProfile.VehiclePlate,

                        VehicleCode =
                            user.DriverProfile.VehicleCode,

                        VehicleType =
                            user.DriverProfile.VehicleType,

                        DriverLicenseNumber =
                            user.DriverProfile.DriverLicenseNumber,

                        DriverLicenseClass =
                            user.DriverProfile.DriverLicenseClass,

                        DriverLicenseIssuedDate =
                            user.DriverProfile.DriverLicenseIssuedDate,

                        DriverLicenseExpiryDate =
                            user.DriverProfile.DriverLicenseExpiryDate,

                        AvatarUrl =
                            user.DriverProfile.AvatarUrl,

                        CitizenIdFrontUrl =
                            user.DriverProfile.CitizenIdFrontUrl,

                        CitizenIdBackUrl =
                            user.DriverProfile.CitizenIdBackUrl,

                        DriverLicenseFrontUrl =
                            user.DriverProfile.DriverLicenseFrontUrl,

                        DriverLicenseBackUrl =
                            user.DriverProfile.DriverLicenseBackUrl,

                        IsActive =
                            user.DriverProfile.IsActive,

                        CreatedAt =
                            user.DriverProfile.CreatedAt,

                        UpdatedAt =
                            user.DriverProfile.UpdatedAt
                    },

                /*
                 * Công ty cũng phải null nếu chưa có DriverProfile.
                 */
                Company = user.DriverProfile == null
                    ? null
                    : new DriverCompanyProfileDto
                    {
                        Id =
                            user.DriverProfile.CompanyProfile.Id,

                        CompanyName =
                            user.DriverProfile.CompanyProfile.CompanyName,

                        TaxCode =
                            user.DriverProfile.CompanyProfile.TaxCode,

                        BusinessLicenseNumber =
                            user.DriverProfile.CompanyProfile
                                .BusinessLicenseNumber,

                        Address =
                            user.DriverProfile.CompanyProfile.Address,

                        PhoneNumber =
                            user.DriverProfile.CompanyProfile.PhoneNumber,

                        Email =
                            user.DriverProfile.CompanyProfile.Email,

                        RepresentativeName =
                            user.DriverProfile.CompanyProfile
                                .RepresentativeName,

                        RepresentativePosition =
                            user.DriverProfile.CompanyProfile
                                .RepresentativePosition,

                        RepresentativeCitizenId =
                            user.DriverProfile.CompanyProfile
                                .RepresentativeCitizenId,

                        RepresentativeCitizenIdIssuedDate =
                            user.DriverProfile.CompanyProfile
                                .RepresentativeCitizenIdIssuedDate,

                        RepresentativeCitizenIdIssuedPlace =
                            user.DriverProfile.CompanyProfile
                                .RepresentativeCitizenIdIssuedPlace,

                        BankAccountNumber =
                            user.DriverProfile.CompanyProfile
                                .BankAccountNumber,

                        BankName =
                            user.DriverProfile.CompanyProfile.BankName,

                        IsActive =
                            user.DriverProfile.CompanyProfile.IsActive,

                        IsDefault =
                            user.DriverProfile.CompanyProfile.IsDefault,

                        CreatedAt =
                            user.DriverProfile.CompanyProfile.CreatedAt,

                        UpdatedAt =
                            user.DriverProfile.CompanyProfile.UpdatedAt
                    }
            })
            .FirstOrDefaultAsync(cancellationToken);

        return detail;
    }

    public async Task RequirePasswordChangeAsync(
    string userId,
    CancellationToken cancellationToken = default)
    {
        var user =
            await _userManager.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException(
                "Không tìm thấy tài khoản.");

        user.MustChangePassword = true;
        user.UpdatedAt = DateTime.UtcNow;

        var result =
            await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(
                    "; ",
                    result.Errors.Select(
                        x => x.Description)));
        }

        // Làm mất hiệu lực cookie đăng nhập cũ.
        var stampResult =
            await _userManager.UpdateSecurityStampAsync(
                user);

        if (!stampResult.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(
                    "; ",
                    stampResult.Errors.Select(
                        x => x.Description)));
        }
    }
}