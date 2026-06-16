using Microsoft.AspNetCore.Authorization;

namespace Bolao.Api.Authorization;

public sealed class AdminOnlyRequirement : IAuthorizationRequirement
{
}