using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Bolao.Api.Authorization;

public sealed class AdminOnlyHandler : AuthorizationHandler<AdminOnlyRequirement>
{
    private readonly IConfiguration _configuration;

    public AdminOnlyHandler(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AdminOnlyRequirement requirement)
    {
        var firebaseUid =
            context.User.FindFirstValue("firebase_uid")
            ?? context.User.FindFirstValue("uid")
            ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(firebaseUid))
            return Task.CompletedTask;

        var adminUids = _configuration
            .GetSection("AdminFirebaseUids")
            .Get<string[]>() ?? [];

        if (adminUids.Contains(firebaseUid))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}