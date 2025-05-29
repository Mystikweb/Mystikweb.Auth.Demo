namespace Mystikweb.Auth.Demo.Web.Shared.Models;

public sealed class UserInfo
{
    public required bool IsAuthenticated { get; set; }

    public required string NameClaimType { get; set; }

    public required string RoleClaimType { get; set; }

    public required ICollection<ClaimValue> Claims { get; set; }

    public static UserInfo Anonymous => new()
    {
        IsAuthenticated = false,
        NameClaimType = string.Empty,
        RoleClaimType = string.Empty,
        Claims = []
    };

    public static UserInfo CreateFromValues(
        bool isAuthenticated,
        string nameClaimType,
        string roleClaimType,
        ICollection<ClaimValue> claims) => new()
        {
            IsAuthenticated = isAuthenticated,
            NameClaimType = nameClaimType ?? throw new ArgumentNullException(nameof(nameClaimType)),
            RoleClaimType = roleClaimType ?? throw new ArgumentNullException(nameof(roleClaimType)),
            Claims = claims ?? throw new ArgumentNullException(nameof(claims))
        };
}
