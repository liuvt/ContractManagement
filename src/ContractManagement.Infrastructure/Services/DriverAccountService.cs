using ContractManagement.Application.Abstractions;
using ContractManagement.Application.Admins.DriverAccounts;
using ContractManagement.Application.Admins.DriverProfiles;
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
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var userName = request.UserName.Trim();

        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new InvalidOperationException(
                "Tên đăng nhập không được để trống.");
        }

        var existingUser =
            await _userManager.FindByNameAsync(userName);

        if (existingUser is not null)
        {
            throw new InvalidOperationException(
                "Tên đăng nhập đã tồn tại.");
        }

        if (!string.IsNullOrWhiteSpace(request.EmployeeCode))
        {
            var employeeCodeExists =
                await dbContext.Users.AnyAsync(
                    x => x.EmployeeCode == request.EmployeeCode,
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
                    x => x.PhoneNumber == request.PhoneNumber,
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
            EmployeeCode = request.EmployeeCode?.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            Email = request.Email?.Trim(),
            IsActive = true,

            // Bắt buộc đổi trong lần đăng nhập đầu tiên.
            MustChangePassword = true,

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

        var roleResult =
            await _userManager.AddToRoleAsync(
                user,
                "Driver");

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            throw new InvalidOperationException(
                string.Join(
                    "; ",
                    roleResult.Errors.Select(
                        x => x.Description)));
        }

        return user.Id;
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
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var account = await dbContext.Users
            .AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => new DriverAccountDetailDto
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

        if (account is null)
            return null;

        account.Profile = await dbContext.DriverProfiles
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => new DriverProfileDto
            {
                Id = x.Id,
                UserId = x.UserId,

                CitizenId = x.CitizenId,
                DateOfBirth = x.DateOfBirth,
                Address = x.Address,
                AreaCode = x.AreaCode,
                VehiclePlate = x.VehiclePlate,
                VehicleCode = x.VehicleCode,
                VehicleType = x.VehicleType,

                DriverLicenseNumber =
                    x.DriverLicenseNumber,

                DriverLicenseClass =
                    x.DriverLicenseClass,

                DriverLicenseIssuedDate =
                    x.DriverLicenseIssuedDate,

                DriverLicenseExpiryDate =
                    x.DriverLicenseExpiryDate,

                AvatarUrl = x.AvatarUrl,

                CitizenIdFrontUrl =
                    x.CitizenIdFrontUrl,

                CitizenIdBackUrl =
                    x.CitizenIdBackUrl,

                DriverLicenseFrontUrl =
                    x.DriverLicenseFrontUrl,

                DriverLicenseBackUrl =
                    x.DriverLicenseBackUrl,

                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        return account;
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