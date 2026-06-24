using ContractManagement.Application.Abstractions;
using ContractManagement.Application.Admins.DriverProfiles;
using ContractManagement.Domain.Drivers;
using ContractManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContractManagement.Infrastructure.Services;

public sealed class DriverProfileService : IDriverProfileService
{
    private readonly ApplicationDbContext _dbContext;

    public DriverProfileService(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> CreateAsync(
        CreateDriverProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var userExists =
            await _dbContext.Users.AnyAsync(
                x => x.Id == request.UserId,
                cancellationToken);

        if (!userExists)
        {
            throw new KeyNotFoundException(
                "Không tìm thấy tài khoản tài xế.");
        }

        var profileExists =
            await _dbContext.DriverProfiles.AnyAsync(
                x => x.UserId == request.UserId,
                cancellationToken);

        if (profileExists)
        {
            throw new InvalidOperationException(
                "Tài khoản đã có hồ sơ tài xế.");
        }

        await ValidateUniqueDataAsync(
            null,
            request.CitizenId,
            request.VehiclePlate,
            cancellationToken);

        var profile = new DriverProfile
        {
            UserId = request.UserId,
            CitizenId = request.CitizenId?.Trim(),
            DateOfBirth = request.DateOfBirth,
            Address = request.Address?.Trim(),
            AreaCode = request.AreaCode?.Trim(),
            VehiclePlate = NormalizeVehiclePlate(
                request.VehiclePlate),
            VehicleCode = request.VehicleCode?.Trim(),
            VehicleType = request.VehicleType?.Trim(),
            DriverLicenseNumber =
                request.DriverLicenseNumber?.Trim(),
            DriverLicenseClass =
                request.DriverLicenseClass?.Trim(),
            DriverLicenseIssuedDate =
                request.DriverLicenseIssuedDate,
            DriverLicenseExpiryDate =
                request.DriverLicenseExpiryDate,
            AvatarUrl = request.AvatarUrl,
            CitizenIdFrontUrl =
                request.CitizenIdFrontUrl,
            CitizenIdBackUrl =
                request.CitizenIdBackUrl,
            DriverLicenseFrontUrl =
                request.DriverLicenseFrontUrl,
            DriverLicenseBackUrl =
                request.DriverLicenseBackUrl,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.DriverProfiles.Add(profile);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return profile.Id;
    }

    public async Task UpdateAsync(
        Guid id,
        UpdateDriverProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var profile =
            await _dbContext.DriverProfiles
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken)
            ?? throw new KeyNotFoundException(
                "Không tìm thấy hồ sơ tài xế.");

        await ValidateUniqueDataAsync(
            id,
            request.CitizenId,
            request.VehiclePlate,
            cancellationToken);

        profile.CitizenId = request.CitizenId?.Trim();
        profile.DateOfBirth = request.DateOfBirth;
        profile.Address = request.Address?.Trim();
        profile.AreaCode = request.AreaCode?.Trim();
        profile.VehiclePlate =
            NormalizeVehiclePlate(request.VehiclePlate);
        profile.VehicleCode =
            request.VehicleCode?.Trim();
        profile.VehicleType =
            request.VehicleType?.Trim();
        profile.DriverLicenseNumber =
            request.DriverLicenseNumber?.Trim();
        profile.DriverLicenseClass =
            request.DriverLicenseClass?.Trim();
        profile.DriverLicenseIssuedDate =
            request.DriverLicenseIssuedDate;
        profile.DriverLicenseExpiryDate =
            request.DriverLicenseExpiryDate;
        profile.AvatarUrl = request.AvatarUrl;
        profile.CitizenIdFrontUrl =
            request.CitizenIdFrontUrl;
        profile.CitizenIdBackUrl =
            request.CitizenIdBackUrl;
        profile.DriverLicenseFrontUrl =
            request.DriverLicenseFrontUrl;
        profile.DriverLicenseBackUrl =
            request.DriverLicenseBackUrl;
        profile.IsActive = request.IsActive;
        profile.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var profile =
            await _dbContext.DriverProfiles
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken)
            ?? throw new KeyNotFoundException(
                "Không tìm thấy hồ sơ tài xế.");

        var hasContracts =
            await _dbContext.Contracts.AnyAsync(
                x => x.DriverId == profile.UserId,
                cancellationToken);

        // Luôn dùng soft delete để bảo toàn dữ liệu.
        profile.IsDeleted = true;
        profile.IsActive = false;
        profile.DeletedAt = DateTime.UtcNow;
        profile.UpdatedAt = DateTime.UtcNow;

        var user =
            await _dbContext.Users.FirstOrDefaultAsync(
                x => x.Id == profile.UserId,
                cancellationToken);

        if (user is not null)
        {
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            if (hasContracts)
            {
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.MaxValue;
            }
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<DriverProfileDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await BuildQuery()
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<DriverProfileDto?> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await BuildQuery()
            .FirstOrDefaultAsync(
                x => x.UserId == userId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<DriverProfileDto>>
        GetListAsync(
            DriverProfileFilter filter,
            CancellationToken cancellationToken = default)
    {
        var page = Math.Max(filter.Page, 1);

        var pageSize = Math.Clamp(
            filter.PageSize,
            1,
            100);

        var query = BuildQuery();

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword = filter.Keyword.Trim();

            query = query.Where(x =>
                (x.CitizenId != null &&
                 x.CitizenId.Contains(keyword)) ||
                (x.VehiclePlate != null &&
                 x.VehiclePlate.Contains(keyword)));
        }

        if (!string.IsNullOrWhiteSpace(filter.AreaCode))
        {
            query = query.Where(
                x => x.AreaCode == filter.AreaCode);
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
            .ToListAsync(cancellationToken);
    }

    private IQueryable<DriverProfileDto> BuildQuery()
    {
        return _dbContext.DriverProfiles
            .AsNoTracking()
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
            });
    }

    private async Task ValidateUniqueDataAsync(
        Guid? currentId,
        string? citizenId,
        string? vehiclePlate,
        CancellationToken cancellationToken)
    {
        var driverCodeExists =
            await _dbContext.DriverProfiles.AnyAsync(
                x =>
                    (!currentId.HasValue ||
                     x.Id != currentId.Value),
                cancellationToken);

        if (!string.IsNullOrWhiteSpace(citizenId))
        {
            var normalizedCitizenId =
                citizenId.Trim();

            var citizenIdExists =
                await _dbContext.DriverProfiles.AnyAsync(
                    x =>
                        (!currentId.HasValue ||
                         x.Id != currentId.Value) &&
                        x.CitizenId == normalizedCitizenId,
                    cancellationToken);

            if (citizenIdExists)
            {
                throw new InvalidOperationException(
                    "Số CCCD đã tồn tại.");
            }
        }

        var normalizedVehiclePlate =
            NormalizeVehiclePlate(vehiclePlate);

        if (!string.IsNullOrWhiteSpace(
                normalizedVehiclePlate))
        {
            var vehiclePlateExists =
                await _dbContext.DriverProfiles.AnyAsync(
                    x =>
                        (!currentId.HasValue ||
                         x.Id != currentId.Value) &&
                        x.VehiclePlate ==
                            normalizedVehiclePlate,
                    cancellationToken);

            if (vehiclePlateExists)
            {
                throw new InvalidOperationException(
                    "Biển số xe đã được sử dụng.");
            }
        }
    }

    public async Task<Guid> UpsertByUserIdAsync(
    string userId,
    UpdateDriverProfileRequest request,
    CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(
                x => x.Id == userId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Không tìm thấy tài khoản tài xế.");

        var profile = await _dbContext.DriverProfiles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                x => x.UserId == userId,
                cancellationToken);

        if (profile is null)
        {
            profile = new DriverProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                IsActive = request.IsActive
            };

            _dbContext.DriverProfiles.Add(profile);
        }
        else
        {
            if (profile.IsDeleted)
            {
                profile.IsDeleted = false;
                profile.DeletedAt = null;
            }

            profile.UpdatedAt = DateTime.UtcNow;
        }

        await ValidateUniqueDataAsync(
            profile.Id,
            request.CitizenId,
            request.VehiclePlate,
            cancellationToken);

        profile.CitizenId =
            NormalizeValue(request.CitizenId);

        profile.DateOfBirth =
            request.DateOfBirth;

        profile.Address =
            NormalizeValue(request.Address);

        profile.AreaCode =
            NormalizeValue(request.AreaCode);

        profile.VehiclePlate =
            NormalizeVehiclePlate(request.VehiclePlate);

        profile.VehicleCode =
            NormalizeValue(request.VehicleCode);

        profile.VehicleType =
            NormalizeValue(request.VehicleType);

        profile.DriverLicenseNumber =
            NormalizeValue(request.DriverLicenseNumber);

        profile.DriverLicenseClass =
            NormalizeValue(request.DriverLicenseClass);

        profile.DriverLicenseIssuedDate =
            request.DriverLicenseIssuedDate;

        profile.DriverLicenseExpiryDate =
            request.DriverLicenseExpiryDate;

        profile.AvatarUrl =
            NormalizeValue(request.AvatarUrl);

        profile.CitizenIdFrontUrl =
            NormalizeValue(request.CitizenIdFrontUrl);

        profile.CitizenIdBackUrl =
            NormalizeValue(request.CitizenIdBackUrl);

        profile.DriverLicenseFrontUrl =
            NormalizeValue(request.DriverLicenseFrontUrl);

        profile.DriverLicenseBackUrl =
            NormalizeValue(request.DriverLicenseBackUrl);

        profile.IsActive =
            request.IsActive;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return profile.Id;
    }

    private async Task ValidateUniqueDataAsync(
    Guid currentId,
    string? citizenId,
    string? vehiclePlate,
    CancellationToken cancellationToken)
    {
        var normalizedCitizenId =
            NormalizeValue(citizenId);

        if (!string.IsNullOrWhiteSpace(normalizedCitizenId))
        {
            var exists = await _dbContext.DriverProfiles
                .IgnoreQueryFilters()
                .AnyAsync(
                    x =>
                        x.Id != currentId &&
                        x.CitizenId == normalizedCitizenId &&
                        !x.IsDeleted,
                    cancellationToken);

            if (exists)
            {
                throw new InvalidOperationException(
                    "Số CCCD đã tồn tại trên hồ sơ tài xế khác.");
            }
        }

        var normalizedPlate =
            NormalizeVehiclePlate(vehiclePlate);

        if (!string.IsNullOrWhiteSpace(normalizedPlate))
        {
            var exists = await _dbContext.DriverProfiles
                .IgnoreQueryFilters()
                .AnyAsync(
                    x =>
                        x.Id != currentId &&
                        x.VehiclePlate == normalizedPlate &&
                        !x.IsDeleted,
                    cancellationToken);

            if (exists)
            {
                throw new InvalidOperationException(
                    "Biển số xe đã tồn tại trên hồ sơ tài xế khác.");
            }
        }
    }

    private static string? NormalizeValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static string? NormalizeVehiclePlate(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value
            .Trim()
            .Replace(" ", string.Empty)
            .ToUpperInvariant();
    }
}