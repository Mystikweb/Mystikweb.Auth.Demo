using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Mystikweb.Auth.Demo.Web.Shared.Models;

namespace Mystikweb.Auth.Demo.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public sealed class UserController : ControllerBase
{
    [HttpGet]
    public IActionResult GetCurrentUser() => Ok(GetUserInfo(User));

    private static UserInfo GetUserInfo(ClaimsPrincipal user)
    {
        if (user.Identity is not { IsAuthenticated: true })
            return UserInfo.Anonymous;

        var claimsIdentity = user.Identity is ClaimsIdentity identity
            ? identity
            : null;

        var nameClaimType = claimsIdentity?.NameClaimType ?? Claims.Name;
        var roleClaimType = claimsIdentity?.RoleClaimType ?? Claims.Role;
        var claims = user.Claims.Select(c => new ClaimValue(c.Type, c.Value)).ToList();

        return UserInfo.CreateFromValues(true, nameClaimType, roleClaimType, claims);
    }
}
