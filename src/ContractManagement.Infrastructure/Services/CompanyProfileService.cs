using ContractManagement.Application.Abstractions;
using ContractManagement.Application.Admins.CompanyProfiles;
using ContractManagement.Domain.Companies;
using ContractManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ContractManagement.Infrastructure.Services;

public sealed class CompanyProfileService(
    IDbContextFactory<ApplicationDbContext> dbContextFactory)
    : ICompanyProfileService
{
    public async Task<IReadOnlyList<CompanyProfileListItemDto>> GetListAsync(
        CompanyProfileFilter filter,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            await dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        var page = Math.Max(filter.Page, 1);

        var pageSize = Math.Clamp(
            filter.PageSize,
            1,
            500);

        var query = dbContext.CompanyProfiles
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword = filter.Keyword.Trim();

            query = query.Where(x =>
                x.CompanyName.Contains(keyword) ||
                x.TaxCode.Contains(keyword) ||
                x.RepresentativeName.Contains(keyword) ||
                (x.PhoneNumber != null &&
                 x.PhoneNumber.Contains(keyword)) ||
                (x.Email != null &&
                 x.Email.Contains(keyword)));
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(
                x => x.IsActive == filter.IsActive.Value);
        }

        return await query
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.CompanyName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new CompanyProfileListItemDto
            {
                Id = x.Id,
                CompanyName = x.CompanyName,
                TaxCode = x.TaxCode,
                PhoneNumber = x.PhoneNumber,
                Email = x.Email,
                RepresentativeName = x.RepresentativeName,
                IsActive = x.IsActive,
                IsDefault = x.IsDefault,
                DriverCount = x.DriverProfiles.Count,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<CompanyProfileDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            await dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        return await dbContext.CompanyProfiles
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new CompanyProfileDto
            {
                Id = x.Id,
                CompanyName = x.CompanyName,
                TaxCode = x.TaxCode,
                BusinessLicenseNumber =
                    x.BusinessLicenseNumber,
                Address = x.Address,
                PhoneNumber = x.PhoneNumber,
                Email = x.Email,
                RepresentativeName =
                    x.RepresentativeName,
                RepresentativePosition =
                    x.RepresentativePosition,
                RepresentativeCitizenId =
                    x.RepresentativeCitizenId,
                RepresentativeCitizenIdIssuedDate =
                    x.RepresentativeCitizenIdIssuedDate,
                RepresentativeCitizenIdIssuedPlace =
                    x.RepresentativeCitizenIdIssuedPlace,
                BankAccountNumber =
                    x.BankAccountNumber,
                BankName = x.BankName,
                IsActive = x.IsActive,
                IsDefault = x.IsDefault,
                ManagedByUserId =
                    x.ManagedByUserId,
                ManagedByUserName =
                    x.ManagedByUser.UserName ?? string.Empty,
                ManagedByFullName =
                    x.ManagedByUser.FullName,
                DriverCount =
                    x.DriverProfiles.Count,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CompanyProfileOptionDto>>
        GetActiveOptionsAsync(
            CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            await dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        return await dbContext.CompanyProfiles
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.CompanyName)
            .Select(x => new CompanyProfileOptionDto
            {
                Id = x.Id,
                CompanyName = x.CompanyName,
                TaxCode = x.TaxCode,
                IsDefault = x.IsDefault
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<Guid> CreateAsync(
        CreateCompanyProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(
            request.CompanyName,
            request.TaxCode,
            request.Address,
            request.RepresentativeName,
            request.RepresentativeCitizenId);

        if (string.IsNullOrWhiteSpace(
                request.ManagedByUserId))
        {
            throw new InvalidOperationException(
                "Không xác định được tài khoản quản lý.");
        }

        await using var dbContext =
            await dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        var managedUserExists =
            await dbContext.Users
                .AsNoTracking()
                .AnyAsync(
                    x => x.Id == request.ManagedByUserId,
                    cancellationToken);

        if (!managedUserExists)
        {
            throw new KeyNotFoundException(
                "Không tìm thấy tài khoản quản lý.");
        }

        var taxCode = request.TaxCode.Trim();

        var taxCodeExists =
            await dbContext.CompanyProfiles
                .AnyAsync(
                    x => x.TaxCode == taxCode,
                    cancellationToken);

        if (taxCodeExists)
        {
            throw new InvalidOperationException(
                "Mã số thuế đã tồn tại.");
        }

        var company = new CompanyProfile
        {
            Id = Guid.NewGuid(),
            CompanyName = request.CompanyName.Trim(),
            TaxCode = taxCode,
            BusinessLicenseNumber =
                NormalizeValue(
                    request.BusinessLicenseNumber),
            Address = request.Address.Trim(),
            PhoneNumber =
                NormalizeValue(request.PhoneNumber),
            Email =
                NormalizeValue(request.Email),
            RepresentativeName =
                request.RepresentativeName.Trim(),
            RepresentativePosition =
                NormalizeValue(
                    request.RepresentativePosition),
            RepresentativeCitizenId =
                request.RepresentativeCitizenId.Trim(),
            RepresentativeCitizenIdIssuedDate =
                request.RepresentativeCitizenIdIssuedDate,
            RepresentativeCitizenIdIssuedPlace =
                NormalizeValue(
                    request
                        .RepresentativeCitizenIdIssuedPlace),
            BankAccountNumber =
                NormalizeValue(
                    request.BankAccountNumber),
            BankName =
                NormalizeValue(request.BankName),
            IsActive = request.IsActive,
            IsDefault = request.IsDefault,
            ManagedByUserId =
                request.ManagedByUserId,
            CreatedAt = DateTime.UtcNow
        };

        if (company.IsDefault)
        {
            await ClearDefaultAsync(
                dbContext,
                null,
                cancellationToken);
        }
        else
        {
            var hasDefault =
                await dbContext.CompanyProfiles
                    .AnyAsync(
                        x => x.IsDefault,
                        cancellationToken);

            if (!hasDefault)
                company.IsDefault = true;
        }

        dbContext.CompanyProfiles.Add(company);

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return company.Id;
    }

    public async Task UpdateAsync(
        Guid id,
        UpdateCompanyProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(
            request.CompanyName,
            request.TaxCode,
            request.Address,
            request.RepresentativeName,
            request.RepresentativeCitizenId);

        await using var dbContext =
            await dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        var company =
            await dbContext.CompanyProfiles
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken)
            ?? throw new KeyNotFoundException(
                "Không tìm thấy hồ sơ doanh nghiệp.");

        var taxCode = request.TaxCode.Trim();

        var duplicateTaxCode =
            await dbContext.CompanyProfiles
                .AnyAsync(
                    x =>
                        x.Id != id &&
                        x.TaxCode == taxCode,
                    cancellationToken);

        if (duplicateTaxCode)
        {
            throw new InvalidOperationException(
                "Mã số thuế đã tồn tại.");
        }

        if (request.IsDefault)
        {
            await ClearDefaultAsync(
                dbContext,
                id,
                cancellationToken);
        }
        else if (company.IsDefault)
        {
            var hasOtherDefault =
                await dbContext.CompanyProfiles
                    .AnyAsync(
                        x =>
                            x.Id != id &&
                            x.IsDefault,
                        cancellationToken);

            if (!hasOtherDefault)
            {
                throw new InvalidOperationException(
                    "Phải có ít nhất một công ty mặc định.");
            }
        }

        company.CompanyName =
            request.CompanyName.Trim();

        company.TaxCode = taxCode;

        company.BusinessLicenseNumber =
            NormalizeValue(
                request.BusinessLicenseNumber);

        company.Address =
            request.Address.Trim();

        company.PhoneNumber =
            NormalizeValue(request.PhoneNumber);

        company.Email =
            NormalizeValue(request.Email);

        company.RepresentativeName =
            request.RepresentativeName.Trim();

        company.RepresentativePosition =
            NormalizeValue(
                request.RepresentativePosition);

        company.RepresentativeCitizenId =
            request.RepresentativeCitizenId.Trim();

        company.RepresentativeCitizenIdIssuedDate =
            request.RepresentativeCitizenIdIssuedDate;

        company.RepresentativeCitizenIdIssuedPlace =
            NormalizeValue(
                request
                    .RepresentativeCitizenIdIssuedPlace);

        company.BankAccountNumber =
            NormalizeValue(
                request.BankAccountNumber);

        company.BankName =
            NormalizeValue(request.BankName);

        company.IsActive =
            request.IsActive;

        company.IsDefault =
            request.IsDefault;

        company.UpdatedAt =
            DateTime.UtcNow;

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task SetDefaultAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            await dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        var company =
            await dbContext.CompanyProfiles
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken)
            ?? throw new KeyNotFoundException(
                "Không tìm thấy hồ sơ doanh nghiệp.");

        if (!company.IsActive)
        {
            throw new InvalidOperationException(
                "Không thể đặt công ty ngừng hoạt động làm mặc định.");
        }

        await ClearDefaultAsync(
            dbContext,
            id,
            cancellationToken);

        company.IsDefault = true;
        company.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            await dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        var company =
            await dbContext.CompanyProfiles
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken)
            ?? throw new KeyNotFoundException(
                "Không tìm thấy hồ sơ doanh nghiệp.");

        var hasDrivers =
            await dbContext.DriverProfiles
                .IgnoreQueryFilters()
                .AnyAsync(
                    x => x.CompanyProfileId == id,
                    cancellationToken);

        if (hasDrivers)
        {
            throw new InvalidOperationException(
                "Không thể xóa công ty vì đang có tài xế trực thuộc.");
        }

        if (company.IsDefault)
        {
            throw new InvalidOperationException(
                "Không thể xóa công ty mặc định. "
                + "Hãy chọn công ty mặc định khác trước.");
        }

        dbContext.CompanyProfiles.Remove(company);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static async Task ClearDefaultAsync(
        ApplicationDbContext dbContext,
        Guid? excludedId,
        CancellationToken cancellationToken)
    {
        var defaultCompanies =
            await dbContext.CompanyProfiles
                .Where(x =>
                    x.IsDefault &&
                    (!excludedId.HasValue ||
                     x.Id != excludedId.Value))
                .ToListAsync(cancellationToken);

        foreach (var item in defaultCompanies)
        {
            item.IsDefault = false;
            item.UpdatedAt = DateTime.UtcNow;
        }
    }

    private static void ValidateRequest(
        string companyName,
        string taxCode,
        string address,
        string representativeName,
        string representativeCitizenId)
    {
        if (string.IsNullOrWhiteSpace(companyName))
        {
            throw new InvalidOperationException(
                "Vui lòng nhập tên doanh nghiệp.");
        }

        if (string.IsNullOrWhiteSpace(taxCode))
        {
            throw new InvalidOperationException(
                "Vui lòng nhập mã số thuế.");
        }

        if (string.IsNullOrWhiteSpace(address))
        {
            throw new InvalidOperationException(
                "Vui lòng nhập địa chỉ doanh nghiệp.");
        }

        if (string.IsNullOrWhiteSpace(
                representativeName))
        {
            throw new InvalidOperationException(
                "Vui lòng nhập người đại diện.");
        }

        if (string.IsNullOrWhiteSpace(
                representativeCitizenId))
        {
            throw new InvalidOperationException(
                "Vui lòng nhập CCCD người đại diện.");
        }
    }

    private static string? NormalizeValue(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}