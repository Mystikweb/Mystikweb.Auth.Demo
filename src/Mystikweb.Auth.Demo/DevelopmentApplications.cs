namespace Mystikweb.Auth.Demo;

public static class DevelopmentApplications
{
    public static OpenIddictApplicationDescriptor ApiResource => new()
    {
        ClientId = "api-resource",
        ClientSecret = "v9X!p2Qw#8LmZr5@",
        Permissions =
        {
            Permissions.Endpoints.Introspection
        }
    };

    public static OpenIddictApplicationDescriptor BlazorApplication => new()
    {
        ClientId = "blazor-app",
        ClientSecret = "bL4z0r$3cr3t",
        ConsentType = ConsentTypes.Explicit,
        DisplayName = "Blazor Application",
        RedirectUris =
        {
            new Uri("https://localhost:7211/authentication/login-callback")
        },
        PostLogoutRedirectUris =
        {
            new Uri("https://localhost:7211/authentication/logout-callback")
        },
        Permissions =
        {
            Permissions.Endpoints.Authorization,
            Permissions.Endpoints.EndSession,
            Permissions.Endpoints.Token,
            Permissions.GrantTypes.AuthorizationCode,
            Permissions.GrantTypes.RefreshToken,
            Permissions.ResponseTypes.Code,
            Permissions.Scopes.Email,
            Permissions.Scopes.Profile,
            Permissions.Scopes.Roles,
            Permissions.Prefixes.Scope + "api-data"
        },
        Requirements =
        {
            Requirements.Features.ProofKeyForCodeExchange
        }
    };

    public static OpenIddictScopeDescriptor ApiScope => new()
    {
        Name = "api-data",
        DisplayName = "API Data Access",
        Resources =
        {
            ApiResource.ClientId!
        }
    };
}
