using Microsoft.AspNetCore.Authorization;

namespace Backend.Dotnet.Controllers.Authorization;

internal static class AdminOnlyAuthorizationPolicy
{
    public const string Name = "AdminOnly";

    public static AuthorizationOptions AddAdminOnly(this AuthorizationOptions options)
    {
        options.AddPolicy(Name, policy => policy.RequireRole("Admin"));
        return options;
    }
}