using ContractManagement.Domain.Companies;
using ContractManagement.Domain.Contracts;
using ContractManagement.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ContractManagement.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var serviceProvider = scope.ServiceProvider;

        var dbFactory =
            serviceProvider.GetRequiredService<
                IDbContextFactory<ApplicationDbContext>>();

        var roleManager =
            serviceProvider.GetRequiredService<
                RoleManager<IdentityRole>>();

        var userManager =
            serviceProvider.GetRequiredService<
                UserManager<ApplicationUser>>();

        await using var db =
            await dbFactory.CreateDbContextAsync();

        await db.Database.MigrateAsync();

        await SeedRolesAsync(roleManager);

        var ownerUser =
            await SeedOwnerUserAsync(
                userManager,
                roleManager);

        await SeedCompanyProfilesAsync(
            db,
            ownerUser);

        await SeedContractTypesAsync(db);
    }

    private static async Task SeedRolesAsync(
        RoleManager<IdentityRole> roleManager)
    {
        var roles = new[]
        {
            "Admin",
            "Driver"
        };

        foreach (var roleName in roles)
        {
            if (await roleManager.RoleExistsAsync(roleName))
                continue;

            var result =
                await roleManager.CreateAsync(
                    new IdentityRole(roleName));

            EnsureIdentitySucceeded(
                result,
                $"Không thể tạo quyền {roleName}");
        }
    }

    private static async Task<ApplicationUser> SeedOwnerUserAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        const string adminName = "admin";
        const string adminPassword = "Admin@123456";
        const string adminRole = "Admin";

        var normalizedAdminName =
            userManager.NormalizeName(adminName);

        var adminUsers =
            await userManager.Users
                .Where(x =>
                    x.NormalizedUserName ==
                    normalizedAdminName)
                .Take(2)
                .ToListAsync();

        if (adminUsers.Count > 1)
        {
            throw new InvalidOperationException(
                "Có nhiều hơn một tài khoản admin trong database. " +
                "Vui lòng xử lý dữ liệu trùng trước khi chạy seeding.");
        }

        var admin = adminUsers.SingleOrDefault();

        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = adminName,
                FullName = "Nguyễn Việt Kiều Anh",
                EmployeeCode = "ADMIN",
                PhoneNumber = "0920365507",
                IsActive = true,
                MustChangePassword = true,
                CreatedAt = DateTime.UtcNow
            };

            var createResult =
                await userManager.CreateAsync(
                    admin,
                    adminPassword);

            EnsureIdentitySucceeded(
                createResult,
                "Không thể tạo tài khoản chủ");
        }
        else
        {
            var changed = false;

            if (admin.FullName != "Nguyễn Việt Kiều Anh")
            {
                admin.FullName = "Nguyễn Việt Kiều Anh";
                changed = true;
            }

            if (admin.EmployeeCode != "ADMIN")
            {
                admin.EmployeeCode = "ADMIN";
                changed = true;
            }

            if (admin.PhoneNumber != "0920365507")
            {
                admin.PhoneNumber = "0920365507";
                changed = true;
            }

            if (!admin.IsActive)
            {
                admin.IsActive = true;
                changed = true;
            }

            if (changed)
            {
                admin.UpdatedAt = DateTime.UtcNow;

                var updateResult =
                    await userManager.UpdateAsync(admin);

                EnsureIdentitySucceeded(
                    updateResult,
                    "Không thể cập nhật tài khoản chủ");
            }
        }

        if (!await roleManager.RoleExistsAsync(adminRole))
        {
            var createRoleResult =
                await roleManager.CreateAsync(
                    new IdentityRole(adminRole));

            EnsureIdentitySucceeded(
                createRoleResult,
                "Không thể tạo quyền Admin");
        }

        if (!await userManager.IsInRoleAsync(
                admin,
                adminRole))
        {
            var addRoleResult =
                await userManager.AddToRoleAsync(
                    admin,
                    adminRole);

            EnsureIdentitySucceeded(
                addRoleResult,
                "Không thể gán quyền Admin cho tài khoản chủ");
        }

        return admin;
    }

    private static async Task SeedCompanyProfilesAsync(
        ApplicationDbContext db,
        ApplicationUser ownerUser)
    {
        var companySeedData =
            new List<CompanySeedData>
            {
                new()
                {
                    CompanyName =
                        "HỢP TÁC XÃ VẬN TẢI 586 - CẦN THƠ",

                    TaxCode =
                        "1801774247",

                    BusinessLicenseNumber =
                        "92240166/GPKDVT",

                    Address =
                        "Khu dân cư lô số 11B - KĐT Nam Cần Thơ, " +
                        "Phường Cái Răng, Thành phố Cần Thơ",

                    PhoneNumber =
                        "0920365507",

                    Email =
                        null,

                    RepresentativeName =
                        "Nguyễn Việt Kiều Anh",

                    RepresentativePosition =
                        "Chủ tịch Hội đồng quản trị",

                    RepresentativeCitizenId =
                        "092195007693",

                    RepresentativeCitizenIdIssuedDate =
                        new DateTime(2021, 8, 14),

                    RepresentativeCitizenIdIssuedPlace =
                        "Cục Cảnh sát quản lý hành chính về trật tự xã hội",

                    BankAccountNumber =
                        null,

                    BankName =
                        null,

                    IsDefault =
                        true
                },

                new()
                {
                    CompanyName =
                        "CÔNG TY THÀNH VIÊN THỨ HAI",

                    TaxCode =
                        "1801774248",

                    BusinessLicenseNumber =
                        null,

                    Address =
                        "Thành phố Cần Thơ",

                    PhoneNumber =
                        "0920365507",

                    Email =
                        null,

                    RepresentativeName =
                        "Lưu Thiện Văn",

                    RepresentativePosition =
                        "Giám đốc",

                    RepresentativeCitizenId =
                        "092094003129",

                    RepresentativeCitizenIdIssuedDate =
                        new DateTime(2021, 8, 14),

                    RepresentativeCitizenIdIssuedPlace =
                        "Cục Cảnh sát quản lý hành chính về trật tự xã hội",

                    BankAccountNumber =
                        null,

                    BankName =
                        null,

                    IsDefault =
                        false
                }
            };

        foreach (var seed in companySeedData)
        {
            var company =
                await db.CompanyProfiles
                    .FirstOrDefaultAsync(x =>
                        x.TaxCode == seed.TaxCode);

            if (company is null)
            {
                company = new CompanyProfile
                {
                    CompanyName =
                        seed.CompanyName,

                    TaxCode =
                        seed.TaxCode,

                    BusinessLicenseNumber =
                        seed.BusinessLicenseNumber,

                    Address =
                        seed.Address,

                    PhoneNumber =
                        seed.PhoneNumber,

                    Email =
                        seed.Email,

                    RepresentativeName =
                        seed.RepresentativeName,

                    RepresentativePosition =
                        seed.RepresentativePosition,

                    RepresentativeCitizenId =
                        seed.RepresentativeCitizenId,

                    RepresentativeCitizenIdIssuedDate =
                        seed.RepresentativeCitizenIdIssuedDate,

                    RepresentativeCitizenIdIssuedPlace =
                        seed.RepresentativeCitizenIdIssuedPlace,

                    BankAccountNumber =
                        seed.BankAccountNumber,

                    BankName =
                        seed.BankName,

                    IsActive =
                        true,

                    IsDefault =
                        seed.IsDefault,

                    ManagedByUserId =
                        ownerUser.Id,

                    CreatedAt =
                        DateTime.UtcNow
                };

                db.CompanyProfiles.Add(company);
            }
            else
            {
                company.CompanyName =
                    seed.CompanyName;

                company.BusinessLicenseNumber =
                    seed.BusinessLicenseNumber;

                company.Address =
                    seed.Address;

                company.PhoneNumber =
                    seed.PhoneNumber;

                company.Email =
                    seed.Email;

                company.RepresentativeName =
                    seed.RepresentativeName;

                company.RepresentativePosition =
                    seed.RepresentativePosition;

                company.RepresentativeCitizenId =
                    seed.RepresentativeCitizenId;

                company.RepresentativeCitizenIdIssuedDate =
                    seed.RepresentativeCitizenIdIssuedDate;

                company.RepresentativeCitizenIdIssuedPlace =
                    seed.RepresentativeCitizenIdIssuedPlace;

                company.BankAccountNumber =
                    seed.BankAccountNumber;

                company.BankName =
                    seed.BankName;

                company.IsActive =
                    true;

                company.IsDefault =
                    seed.IsDefault;

                company.ManagedByUserId =
                    ownerUser.Id;

                company.UpdatedAt =
                    DateTime.UtcNow;
            }
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedContractTypesAsync(
        ApplicationDbContext db)
    {
        const string contractTypeCode = "VAN_CHUYEN";

        var contractType =
            await db.ContractTypes
                .FirstOrDefaultAsync(x =>
                    x.Code == contractTypeCode);

        if (contractType is null)
        {
            db.ContractTypes.Add(
                new ContractType
                {
                    Code =
                        contractTypeCode,

                    Name =
                        "Hợp đồng vận chuyển",

                    Description =
                        "Mẫu hợp đồng vận chuyển hành khách",

                    IsActive =
                        true
                });
        }
        else
        {
            contractType.Name =
                "Hợp đồng vận chuyển";

            contractType.Description =
                "Mẫu hợp đồng vận chuyển hành khách";

            contractType.IsActive =
                true;
        }

        await db.SaveChangesAsync();
    }

    private static void EnsureIdentitySucceeded(
        IdentityResult result,
        string message)
    {
        if (result.Succeeded)
            return;

        var errors =
            string.Join(
                "; ",
                result.Errors.Select(x =>
                    $"{x.Code}: {x.Description}"));

        throw new InvalidOperationException(
            $"{message}: {errors}");
    }

    private sealed class CompanySeedData
    {
        public string CompanyName { get; init; }
            = string.Empty;

        public string TaxCode { get; init; }
            = string.Empty;

        public string? BusinessLicenseNumber { get; init; }

        public string Address { get; init; }
            = string.Empty;

        public string? PhoneNumber { get; init; }

        public string? Email { get; init; }

        public string RepresentativeName { get; init; }
            = string.Empty;

        public string? RepresentativePosition { get; init; }

        public string RepresentativeCitizenId { get; init; }
            = string.Empty;

        public DateTime? RepresentativeCitizenIdIssuedDate
        {
            get;
            init;
        }

        public string? RepresentativeCitizenIdIssuedPlace
        {
            get;
            init;
        }

        public string? BankAccountNumber { get; init; }

        public string? BankName { get; init; }

        public bool IsDefault { get; init; }
    }
}