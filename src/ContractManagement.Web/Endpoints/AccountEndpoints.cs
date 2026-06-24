using ContractManagement.Domain.Identity;
using Microsoft.AspNetCore.Identity;

namespace ContractManagement.Web.Endpoints;

public static class AccountEndpoints
{
    public static void MapAccountEndpoints(
        this WebApplication app)
    {
        app.MapPost(
                "/account/login/submit",
                LoginAsync)
            .AllowAnonymous()
            .DisableAntiforgery();

        app.MapPost(
                "/account/logout/submit",
                LogoutAsync)
            .RequireAuthorization()
            .DisableAntiforgery();
    }

    private static async Task<IResult> LoginAsync(
        HttpContext httpContext,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        var form =
            await httpContext.Request.ReadFormAsync();

        var loginName =
            form["LoginName"]
                .ToString()
                .Trim();

        var password =
            form["Password"].ToString();

        var rememberMe =
            string.Equals(
                form["RememberMe"],
                "true",
                StringComparison.OrdinalIgnoreCase);

        var returnUrl =
            form["ReturnUrl"].ToString();

        if (string.IsNullOrWhiteSpace(loginName) ||
            string.IsNullOrWhiteSpace(password))
        {
            return Results.Redirect(
                "/account/login?error=invalid");
        }

        ApplicationUser? user = null;

        if (loginName.Contains('@'))
        {
            user =
                await userManager.FindByEmailAsync(
                    loginName);
        }

        user ??=
            await userManager.FindByNameAsync(
                loginName);

        if (user is null)
        {
            user = userManager.Users
                .FirstOrDefault(x =>
                    x.PhoneNumber == loginName ||
                    x.EmployeeCode == loginName);
        }

        if (user is null)
        {
            return Results.Redirect(
                "/account/login?error=invalid");
        }

        if (!user.IsActive)
        {
            return Results.Redirect(
                "/account/login?error=inactive");
        }

        var result =
            await signInManager.PasswordSignInAsync(
                user,
                password,
                rememberMe,
                lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            return Results.Redirect(
                "/account/login?error=locked");
        }

        if (!result.Succeeded)
        {
            return Results.Redirect(
                "/account/login?error=invalid");
        }

        if (IsLocalUrl(returnUrl))
        {
            return Results.Redirect(returnUrl);
        }

        var roles =
            await userManager.GetRolesAsync(user);

        return Results.Redirect(
            roles.Contains("Admin")
                ? "/admin/dashboard"
                : "/driver/dashboard");
    }

    private static async Task<IResult> LogoutAsync(
        SignInManager<ApplicationUser> signInManager)
    {
        await signInManager.SignOutAsync();

        return Results.Redirect("/account/login");
    }

    private static bool IsLocalUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return url.StartsWith('/') &&
               !url.StartsWith("//") &&
               !url.StartsWith("/\\");
    }
}