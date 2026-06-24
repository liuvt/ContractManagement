
namespace ContractManagement.Infrastructure.Identity;

using global::ContractManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

public sealed class PasswordChangedHandler
    : AuthorizationHandler<PasswordChangedRequirement>
{
    private readonly ApplicationDbContext _dbContext;

    public PasswordChangedHandler(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PasswordChangedRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
            return;

        var userId = context.User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return;

        var user = await _dbContext.Users
            .AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => new
            {
                x.IsActive,
                x.MustChangePassword
            })
            .FirstOrDefaultAsync();

        if (user is null)
            return;

        if (!user.IsActive)
            return;

        if (user.MustChangePassword)
            return;

        context.Succeed(requirement);
    }
}
