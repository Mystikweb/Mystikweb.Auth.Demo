using System.Globalization;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.FluentUI.AspNetCore.Components;

using Mystikweb.Auth.Demo.Web.Components;
using Mystikweb.Auth.Demo.Web.Services;

using OpenIddict.Client;

using Quartz;

using Yarp.ReverseProxy.Forwarder;
using Yarp.ReverseProxy.Transforms;

using static OpenIddict.Client.AspNetCore.OpenIddictClientAspNetCoreConstants;
using static OpenIddict.Client.OpenIddictClientModels;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddRedisOutputCache(ServiceConstants.CacheService.RESOURCE_NAME);

builder.AddNpgsqlDbContext<ApplicationDbContext>(ServiceConstants.BlazorService.DATABASE_NAME,
    configureDbContextOptions: configure => configure.UseOpenIddict());

if (builder.Environment.IsDevelopment())
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configure the antiforgery stack to allow extracting
// antiforgery tokens from the X-XSRF-TOKEN header.
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
    options.Cookie.Name = "__Host-X-XSRF-TOKEN";
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Configure the cookie authentication options.
        options.Cookie.Name = "__Host-Auth";
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

// OpenIddict offers native integration with Quartz.NET to perform scheduled tasks
// (like pruning orphaned authorizations/tokens from the database) at regular intervals.
builder.Services.AddQuartz(options =>
{
    options.UseSimpleTypeLoader();
    options.UseInMemoryStore();
});

// Register the Quartz.NET service and configure it to block shutdown until jobs are complete.
builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        // Configure OpenIddict to use the Entity Framework Core stores and models.
        // Note: call ReplaceDefaultEntities() to replace the default OpenIddict entities.
        options.UseEntityFrameworkCore()
            .UseDbContext<ApplicationDbContext>();

        // Enable Quartz.NET integration.
        options.UseQuartz();
    })
    .AddClient(options =>
    {
        // Note: this sample uses the authorization code and refresh token
        // flows, but you can enable the other flows if necessary.
        options.AllowAuthorizationCodeFlow()
            .AllowRefreshTokenFlow();

        // Register the signing and encryption credentials used to protect
        // sensitive data like the state tokens produced by OpenIddict.
        options.AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate();

        // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
        options.UseAspNetCore()
            .EnableStatusCodePagesIntegration()
            .EnableRedirectionEndpointPassthrough()
            .EnablePostLogoutRedirectionEndpointPassthrough();

        // Register the System.Net.Http integration and use the identity of the current
        // assembly as a more specific user agent, which can be useful when dealing with
        // providers that use the user agent as a way to throttle requests (e.g Reddit).
        options.UseSystemNetHttp()
            .SetProductInformation(typeof(Program).Assembly);

        // Add the client registraiton matching the client application for the identity server.
        var issuerString = builder.Configuration.GetValue<string>(ServiceConstants.IDENTITY_URI_ENVIRONMENT_VARIABLE);
        if (string.IsNullOrEmpty(issuerString))
            throw new InvalidOperationException($"The configuration value for '{ServiceConstants.IDENTITY_URI_ENVIRONMENT_VARIABLE}' is missing or empty.");

        options.AddRegistration(new OpenIddictClientRegistration
        {
            Issuer = new Uri(issuerString, UriKind.Absolute),
            ClientId = DevelopmentApplications.BlazorApplication.ClientId,
            ClientSecret = DevelopmentApplications.BlazorApplication.ClientSecret,
            Scopes =
            {
                Scopes.Profile,
                Scopes.OfflineAccess,
                DevelopmentApplications.ApiScope.Name!
            },
            RedirectUri = new Uri("authentication/login-callback", UriKind.Relative),
            PostLogoutRedirectUri = new Uri("authentication/logout-callback", UriKind.Relative)
        });
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(ServiceConstants.AUTHORIZATION_POLICY_NAME, policy =>
    {
        // Configure the default authorization policy to require a valid authentication cookie.
        policy.AuthenticationSchemes.Add(CookieAuthenticationDefaults.AuthenticationScheme);
        policy.RequireAuthenticatedUser();
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpForwarderWithServiceDiscovery();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(builder =>
    {
        builder.AddRequestTransform(async context =>
        {
            // Attach the access token, access token expiration date and refresh token resolved from the authentication
            // cookie to the request options so they can later be resolved from the delegating handler and attached
            // to the request message or used to refresh the tokens if the server returned a 401 error response.
            //
            // Alternatively, the user tokens could be stored in a database or a distributed cache.

            var result = await context.HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            context.ProxyRequest.Options.Set(
                key: new(Tokens.BackchannelAccessToken),
                value: result.Properties?.GetTokenValue(Tokens.BackchannelAccessToken));

            context.ProxyRequest.Options.Set(
                key: new(Tokens.BackchannelAccessTokenExpirationDate),
                value: result.Properties?.GetTokenValue(Tokens.BackchannelAccessTokenExpirationDate));

            context.ProxyRequest.Options.Set(
                key: new(Tokens.RefreshToken),
                value: result.Properties?.GetTokenValue(Tokens.RefreshToken));
        });

        builder.AddResponseTransform(async context =>
        {
            // If tokens were refreshed during the request handling (e.g due to the stored access token being
            // expired or a 401 error response being returned by the resource server), extract and attach them
            // to the authentication cookie that will be returned to the browser: doing that is essential as
            // OpenIddict uses rolling refresh tokens: if the refresh token wasn't replaced, future refresh
            // token requests would end up being rejected as they would be treated as replayed requests.

            if (context.ProxyResponse is not TokenRefreshingHttpResponseMessage {
                RefreshTokenAuthenticationResult: RefreshTokenAuthenticationResult } response)
                return;

            var result = await context.HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Override the tokens using the values returned in the token response.
            var properties = result.Properties?.Clone() ?? new AuthenticationProperties();
            properties.UpdateTokenValue(Tokens.BackchannelAccessToken, response.RefreshTokenAuthenticationResult.AccessToken);

            properties.UpdateTokenValue(Tokens.BackchannelAccessTokenExpirationDate,
                response.RefreshTokenAuthenticationResult.AccessTokenExpirationDate?.ToString(CultureInfo.InvariantCulture) ?? string.Empty);

            // Note: if no refresh token was returned, preserve the refresh token initially returned.
            if (!string.IsNullOrEmpty(response.RefreshTokenAuthenticationResult.RefreshToken))
                properties.UpdateTokenValue(Tokens.RefreshToken, response.RefreshTokenAuthenticationResult.RefreshToken);

            // Remove the redirect URI from the authentication properties
            // to prevent the cookies handler from genering a 302 response.
            properties.RedirectUri = null;

            // Note: this event handler can be called concurrently for the same user if multiple HTTP
            // responses are returned in parallel: in this case, the browser will always store the latest
            // cookie received and the refresh tokens stored in the other cookies will be discarded.
            if (result.Ticket?.AuthenticationScheme != null && result.Principal != null)
                await context.HttpContext.SignInAsync(result.Ticket.AuthenticationScheme, result.Principal, properties);
        });
    });

// Replace the default HTTP client factory used by YARP by an instance able to inject the HTTP delegating
// handler that will be used to attach the access tokens to HTTP requests or refresh tokens if necessary.
builder.Services.AddSingleton<IForwarderHttpClientFactory, TokenRefreshingForwarderHttpClientFactory>();

builder.Services.AddHostedService<MigrationStartupService>();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddControllers();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options => options.DetailedErrors = builder.Environment.IsDevelopment())
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization(options => options.SerializeAllClaims = true);

builder.Services.AddFluentUIComponents();


var app = builder.Build();

app.UseOutputCache();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Mystikweb.Auth.Demo.Web.Client._Imports).Assembly);

app.MapDefaultEndpoints();
app.MapDefaultControllerRoute();
app.MapReverseProxy();

app.Run();
