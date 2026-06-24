using Microsoft.AspNetCore.Authorization;

namespace ContractManagement.Infrastructure.Identity;

public sealed class PasswordChangedRequirement
 : IAuthorizationRequirement
{
}
