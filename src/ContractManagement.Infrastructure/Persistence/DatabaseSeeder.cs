using ContractManagement.Domain.Contracts;
using ContractManagement.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ContractManagement.Infrastructure.Persistence;
public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope=services.CreateScope();
        var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
        await using var db = await dbFactory.CreateDbContextAsync();
        await db.Database.MigrateAsync();
        var roleManager=scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager=scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        foreach(var role in new[]{"Admin","Driver"}) if(!await roleManager.RoleExistsAsync(role)) await roleManager.CreateAsync(new IdentityRole(role));
        const string adminName="admin";
        if(await userManager.FindByNameAsync(adminName) is null){var admin=new ApplicationUser{UserName=adminName,FullName="Quản trị hệ thống",IsActive=true,MustChangePassword=true};var result=await userManager.CreateAsync(admin,"Admin@123456");if(result.Succeeded) await userManager.AddToRoleAsync(admin,"Admin");}
        if(!await db.ContractTypes.AnyAsync()) { db.ContractTypes.Add(new ContractType{Code="VAN_CHUYEN",Name="Hợp đồng vận chuyển",Description="Mẫu hợp đồng vận chuyển hành khách",IsActive=true}); await db.SaveChangesAsync(); }
    }
}
