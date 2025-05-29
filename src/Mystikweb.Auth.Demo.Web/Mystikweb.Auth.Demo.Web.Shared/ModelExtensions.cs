using System.Security.Claims;

using Mystikweb.Auth.Demo.Web.Shared.Models;

namespace Mystikweb.Auth.Demo.Web.Shared;

public static class ModelExtensions
{
    public static ClaimsPrincipal ToClaimsPrincipal(this UserInfo userInfo, string identityName = "Mystikweb.Auth.Demo.Web")
    {
        ArgumentNullException.ThrowIfNull(userInfo);

        if (userInfo == UserInfo.Anonymous)
            return new ClaimsPrincipal(new ClaimsIdentity());

        var identity = new ClaimsIdentity(userInfo.Claims.Select(s => s.ToClaim()),
            identityName,
            userInfo.NameClaimType,
            userInfo.RoleClaimType);

        return new ClaimsPrincipal(identity);
    }

    private static Claim ToClaim(this ClaimValue value) =>
        new(value.Type, value.Value);
}
