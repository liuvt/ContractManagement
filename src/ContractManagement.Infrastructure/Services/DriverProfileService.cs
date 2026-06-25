using ContractManagement.Application.Abstractions;
using ContractManagement.Application.Admins.DriverProfiles;
using ContractManagement.Domain.Drivers;
using ContractManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContractManagement.Infrastructure.Services;

public sealed class DriverProfileService(
    IDbContextFactory<ApplicationDbContext> dbContextFactory)
    : IDriverProfileService
{
    public async Task<Guid> CreateAsync(
        CreateDriverProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            await dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        ValidateUserId(request.UserId);

        await EnsureUserExistsAsync(
            dbContext,
            request.UserId,
            cancellationToken);

        await EnsureCompanyExistsAsync(
            dbContext,
            request.CompanyProfileId,
            cancellationToken);

        var profileExists =
            await dbContext.DriverProfiles
                .IgnoreQueryFilters()
                .AnyAsync(
                    x => x.UserId == request.UserId,
                    cancellationToken);

        if (profileExists)
        {
            throw new InvalidOperationException(
                "Tài khoản đã có hồ sơ tài xế.");
        }

        await ValidateUniqueDataAsync(
            dbContext,
            currentId: null,
            request.CitizenId,
            request.VehiclePlate,
            cancellationToken);

        var profile = new DriverProfile
        {
            Id = Guid.NewGuid(),

            UserId = request.UserId,
            CompanyProfileId = request.CompanyProfileId,

            CitizenId =
                NormalizeValue(request.CitizenId),

            CitizenIdIssuedDate =
                request.CitizenIdIssuedDate,

            DateOfBirth =
                request.DateOfBirth,

            Address =
                NormalizeValue(request.Address),

            AreaCode =
                NormalizeValue(request.AreaCode),

            VehiclePlate =
                NormalizeVehiclePlate(request.VehiclePlate),

            VehicleCode =
                NormalizeValue(request.VehicleCode),

            VehicleType =
                NormalizeValue(request.VehicleType),

            DriverLicenseNumber =
                NormalizeValue(request.DriverLicenseNumber),

            DriverLicenseClass =
                NormalizeValue(request.DriverLicenseClass),

            DriverLicenseIssuedDate =
                request.DriverLicenseIssuedDate,

            DriverLicenseExpiryDate =
                request.DriverLicenseExpiryDate,

            AvatarUrl =
                NormalizeValue(request.AvatarUrl),

            CitizenIdFrontUrl =
                NormalizeValue(request.CitizenIdFrontUrl),

            CitizenIdBackUrl =
                NormalizeValue(request.CitizenIdBackUrl),

            DriverLicenseFrontUrl =
                NormalizeValue(request.DriverLicenseFrontUrl),

            DriverLicenseBackUrl =
                NormalizeValue(request.DriverLicenseBackUrl),

            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.DriverProfiles.Add(profile);

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return profile.Id;
    }

    public async Task UpdateAsync(
        Guid id,
        UpdateDriverProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            await dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        var profile =
            await dbContext.DriverProfiles
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken)
            ?? throw new KeyNotFoundException(
                "Không tìm thấy hồ sơ tài xế.");

        await EnsureCompanyExistsAsync(
            dbContext,
            request.CompanyProfileId,
            cancellationToken);

        await ValidateUniqueDataAsync(
            dbContext,
            id,
            request.CitizenId,
            request.VehiclePlate,
            cancellationToken);

        MapUpdateRequest(
            request,
            profile);

        profile.UpdatedAt =
            DateTime.UtcNow;

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<Guid> UpsertByUserIdAsync(
        string userId,
        UpdateDriverProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            await dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        ValidateUserId(userId);

        await EnsureUserExistsAsync(
            dbContext,
            userId,
            cancellationToken);

        await EnsureCompanyExistsAsync(
            dbContext,
            request.CompanyProfileId,
            cancellationToken);

        var profile =
            await dbContext.DriverProfiles
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(
                    x => x.UserId == userId,
                    cancellationToken);

        var isCreating =
            profile is null;

        if (profile is null)
        {
            profile = new DriverProfile
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.DriverProfiles.Add(profile);
        }
        else
        {
            if (profile.IsDeleted)
            {
                profile.IsDeleted = false;
                profile.DeletedAt = null;
            }

            profile.UpdatedAt =
                DateTime.UtcNow;
        }

        await ValidateUniqueDataAsync(
            dbContext,
            profile.Id,
            request.CitizenId,
            request.VehiclePlate,
            cancellationToken);

        MapUpdateRequest(
            request,
            profile);

        if (isCreating)
        {
            profile.CreatedAt =
                DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return profile.Id;
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            await dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        var profile =
            await dbContext.DriverProfiles
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken)
            ?? throw new KeyNotFoundException(
                "Không tìm thấy hồ sơ tài xế.");

        var hasContracts =
            await dbContext.Contracts
                .AnyAsync(
                    x => x.DriverId == profile.UserId,
                    cancellationToken);

        profile.IsDeleted = true;
        profile.IsActive = false;
        profile.DeletedAt = DateTime.UtcNow;
        profile.UpdatedAt = DateTime.UtcNow;

        var user =
            await dbContext.Users
                .FirstOrDefaultAsync(
                    x => x.Id == profile.UserId,
                    cancellationToken);

        if (user is not null)
        {
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            if (hasContracts)
            {
                user.LockoutEnabled = true;
                user.LockoutEnd =
                    DateTimeOffset.MaxValue;
            }
        }

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<DriverProfileDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            await dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        return await BuildQuery(dbContext)
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<DriverProfileDto?> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return null;

        await using var dbContext =
            await dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        return await BuildQuery(dbContext)
            .FirstOrDefaultAsync(
                x => x.UserId == userId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<DriverProfileDto>> GetListAsync(
        DriverProfileFilter filter,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            await dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        var page =
            Math.Max(filter.Page, 1);

        var pageSize =
            Math.Clamp(
                filter.PageSize,
                1,
                100);

        var query =
            BuildQuery(dbContext);

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword =
                filter.Keyword.Trim();

            query = query.Where(x =>
                (x.CitizenId != null &&
                 x.CitizenId.Contains(keyword)) ||

                (x.VehiclePlate != null &&
                 x.VehiclePlate.Contains(keyword)) ||

                (x.CompanyName != null &&
                 x.CompanyName.Contains(keyword)) ||

                (x.CompanyTaxCode != null &&
                 x.CompanyTaxCode.Contains(keyword)));
        }

        if (!string.IsNullOrWhiteSpace(filter.AreaCode))
        {
            var areaCode =
                filter.AreaCode.Trim();

            query = query.Where(
                x => x.AreaCode == areaCode);
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(
                x => x.IsActive ==
                     filter.IsActive.Value);
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<DriverProfileDto> BuildQuery(
    ApplicationDbContext dbContext)
    {
        return dbContext.DriverProfiles
            .AsNoTracking()
            .Select(x => new DriverProfileDto
            {
                Id = x.Id,
                UserId = x.UserId,

                UserName =
                    x.User.UserName ?? string.Empty,

                DriverCode =
                    x.User.EmployeeCode ?? string.Empty,

                FullName =
                    x.User.FullName,

                CitizenId =
                    x.CitizenId,

                CitizenIdIssuedDate =
                    x.CitizenIdIssuedDate,

                DateOfBirth =
                    x.DateOfBirth,

                Address =
                    x.Address,

                AreaCode =
                    x.AreaCode,

                CompanyProfileId =
                    x.CompanyProfileId,

                CompanyName =
                    x.CompanyProfile.CompanyName,

                CompanyTaxCode =
                    x.CompanyProfile.TaxCode,

                VehiclePlate =
                    x.VehiclePlate,

                VehicleCode =
                    x.VehicleCode,

                VehicleType =
                    x.VehicleType,

                DriverLicenseNumber =
                    x.DriverLicenseNumber,

                DriverLicenseClass =
                    x.DriverLicenseClass,

                DriverLicenseIssuedDate =
                    x.DriverLicenseIssuedDate,

                DriverLicenseExpiryDate =
                    x.DriverLicenseExpiryDate,

                AvatarUrl =
                    x.AvatarUrl,

                CitizenIdFrontUrl =
                    x.CitizenIdFrontUrl,

                CitizenIdBackUrl =
                    x.CitizenIdBackUrl,

                DriverLicenseFrontUrl =
                    x.DriverLicenseFrontUrl,

                DriverLicenseBackUrl =
                    x.DriverLicenseBackUrl,

                IsActive =
                    x.IsActive,

                CreatedAt =
                    x.CreatedAt,

                UpdatedAt =
                    x.UpdatedAt
            });
    }

    private static void MapUpdateRequest(
        UpdateDriverProfileRequest request,
        DriverProfile profile)
    {
        profile.CompanyProfileId =
            request.CompanyProfileId;

        profile.CitizenId =
            NormalizeValue(request.CitizenId);

        profile.CitizenIdIssuedDate =
            request.CitizenIdIssuedDate;

        profile.DateOfBirth =
            request.DateOfBirth;

        profile.Address =
            NormalizeValue(request.Address);

        profile.AreaCode =
            NormalizeValue(request.AreaCode);

        profile.VehiclePlate =
            NormalizeVehiclePlate(
                request.VehiclePlate);

        profile.VehicleCode =
            NormalizeValue(request.VehicleCode);

        profile.VehicleType =
            NormalizeValue(request.VehicleType);

        profile.DriverLicenseNumber =
            NormalizeValue(
                request.DriverLicenseNumber);

        profile.DriverLicenseClass =
            NormalizeValue(
                request.DriverLicenseClass);

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
            NormalizeValue(
                request.DriverLicenseFrontUrl);

        profile.DriverLicenseBackUrl =
            NormalizeValue(
                request.DriverLicenseBackUrl);

        profile.IsActive =
            request.IsActive;
    }

    private static async Task EnsureUserExistsAsync(
        ApplicationDbContext dbContext,
        string userId,
        CancellationToken cancellationToken)
    {
        var userExists =
            await dbContext.Users
                .AsNoTracking()
                .AnyAsync(
                    x => x.Id == userId,
                    cancellationToken);

        if (!userExists)
        {
            throw new KeyNotFoundException(
                "Không tìm thấy tài khoản tài xế.");
        }
    }

    private static async Task EnsureCompanyExistsAsync(
        ApplicationDbContext dbContext,
        Guid companyProfileId,
        CancellationToken cancellationToken)
    {
        if (companyProfileId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "Vui lòng chọn công ty quản lý tài xế.");
        }

        var companyExists =
            await dbContext.CompanyProfiles
                .AsNoTracking()
                .AnyAsync(
                    x =>
                        x.Id == companyProfileId &&
                        x.IsActive,
                    cancellationToken);

        if (!companyExists)
        {
            throw new InvalidOperationException(
                "Công ty không tồn tại hoặc đã ngừng hoạt động.");
        }
    }

    private static async Task ValidateUniqueDataAsync(
        ApplicationDbContext dbContext,
        Guid? currentId,
        string? citizenId,
        string? vehiclePlate,
        CancellationToken cancellationToken)
    {
        var normalizedCitizenId =
            NormalizeValue(citizenId);

        if (!string.IsNullOrWhiteSpace(
                normalizedCitizenId))
        {
            var citizenIdExists =
                await dbContext.DriverProfiles
                    .IgnoreQueryFilters()
                    .AnyAsync(
                        x =>
                            (!currentId.HasValue ||
                             x.Id != currentId.Value) &&

                            !x.IsDeleted &&

                            x.CitizenId ==
                            normalizedCitizenId,
                        cancellationToken);

            if (citizenIdExists)
            {
                throw new InvalidOperationException(
                    "Số CCCD đã tồn tại trên hồ sơ tài xế khác.");
            }
        }

        var normalizedVehiclePlate =
            NormalizeVehiclePlate(vehiclePlate);

        if (!string.IsNullOrWhiteSpace(
                normalizedVehiclePlate))
        {
            var vehiclePlateExists =
                await dbContext.DriverProfiles
                    .IgnoreQueryFilters()
                    .AnyAsync(
                        x =>
                            (!currentId.HasValue ||
                             x.Id != currentId.Value) &&

                            !x.IsDeleted &&

                            x.VehiclePlate ==
                            normalizedVehiclePlate,
                        cancellationToken);

            if (vehiclePlateExists)
            {
                throw new InvalidOperationException(
                    "Biển số xe đã tồn tại trên hồ sơ tài xế khác.");
            }
        }
    }

    private static void ValidateUserId(
        string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new InvalidOperationException(
                "Thiếu mã tài khoản tài xế.");
        }
    }

    private static string? NormalizeValue(
        string? value)
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