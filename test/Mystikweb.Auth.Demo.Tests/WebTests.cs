using System.Text.Json;
using System.Text.RegularExpressions;

namespace Mystikweb.Auth.Demo.Tests;

public class WebTests
{
    [Fact]
    public async Task GetWebResourceRootReturnsOkStatusCode()
    {
        using var _ = new EnvironmentVariableScope(new Dictionary<string, string>
        {
            ["Parameters__IdentityDbUsername"] = "identityuser",
            ["Parameters__IdentityDbPassword"] = "IdentityPass123",
            ["Parameters__ApiDbUsername"] = "apiuser",
            ["Parameters__ApiDbPassword"] = "ApiPass123",
            ["Parameters__BlazorDbUsername"] = "blazoruser",
            ["Parameters__BlazorDbPassword"] = "BlazorPass123"
        });

        // Arrange
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Mystikweb_Auth_Demo_AppHost>();
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });
        // To output logs to the xUnit.net ITestOutputHelper, consider adding a package from https://www.nuget.org/packages?q=xunit+logging

        await using var app = await appHost.BuildAsync();
        var resourceNotificationService = app.Services.GetRequiredService<ResourceNotificationService>();
        await app.StartAsync();

        // Act
        var httpClient = app.CreateHttpClient("blazorfrontend");
        await resourceNotificationService.WaitForResourceAsync("blazorfrontend", KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(30));
        var response = await httpClient.GetAsync("/");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public void CreateBlazorApplicationUsesProvidedFrontendUri()
    {
        var descriptor = DevelopmentApplications.CreateBlazorApplication(new Uri("https://localhost:12345/"));

        Assert.Contains(descriptor.RedirectUris, uri => uri == new Uri("https://localhost:12345/authentication/login-callback"));
        Assert.Contains(descriptor.PostLogoutRedirectUris, uri => uri == new Uri("https://localhost:12345/authentication/logout-callback"));
    }

    [Fact]
    public async Task RegisterConfirmAndLocalLoginFlowAuthenticatesIdentityUser()
    {
        using var _ = new EnvironmentVariableScope(new Dictionary<string, string>
        {
            ["Parameters__IdentityDbUsername"] = "identityuser",
            ["Parameters__IdentityDbPassword"] = "IdentityPass123",
            ["Parameters__ApiDbUsername"] = "apiuser",
            ["Parameters__ApiDbPassword"] = "ApiPass123",
            ["Parameters__BlazorDbUsername"] = "blazoruser",
            ["Parameters__BlazorDbPassword"] = "BlazorPass123"
        });

        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Mystikweb_Auth_Demo_AppHost>();
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        await using var app = await appHost.BuildAsync();
        var resourceNotificationService = app.Services.GetRequiredService<ResourceNotificationService>();
        await app.StartAsync();

        await resourceNotificationService.WaitForResourceAsync("identityserver", KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(30));

        var identityBaseAddress = app.CreateHttpClient("identityserver", "https").BaseAddress
            ?? throw new InvalidOperationException("The identityserver base address is not available.");

        var cookieContainer = new CookieContainer();
        using var handler = new HttpClientHandler
        {
            AllowAutoRedirect = true,
            CookieContainer = cookieContainer,
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        using var client = new HttpClient(handler);

        var email = $"sandbox-{Guid.NewGuid():N}@example.com";
        const string password = "Pass123!";

        var loginResponse = await client.GetAsync(new Uri(identityBaseAddress, "Identity/Account/Login?returnUrl=%2F"));
        loginResponse.EnsureSuccessStatusCode();
        var loginUrl = loginResponse.RequestMessage?.RequestUri
            ?? throw new InvalidOperationException("The login page URL was not resolved.");
        var loginHtml = await loginResponse.Content.ReadAsStringAsync();

        var registerHref = Extract(loginHtml, "<a[^>]*href=\"([^\"]+)\"[^>]*>Register as a new user</a>", "register link")
            .Replace("&amp;", "&", StringComparison.Ordinal);
        var registerUrl = new Uri(loginUrl, registerHref);

        var registerPageResponse = await client.GetAsync(registerUrl);
        registerPageResponse.EnsureSuccessStatusCode();
        var registerPageHtml = await registerPageResponse.Content.ReadAsStringAsync();
        var registerToken = Extract(registerPageHtml, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"", "register token");

        using var registerContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.Email"] = email,
            ["Input.Password"] = password,
            ["Input.ConfirmPassword"] = password,
            ["__RequestVerificationToken"] = registerToken
        });

        var registerPostResponse = await client.PostAsync(registerUrl, registerContent);
        registerPostResponse.EnsureSuccessStatusCode();
        var registerConfirmationHtml = await registerPostResponse.Content.ReadAsStringAsync();
        var confirmUrlString = Extract(registerConfirmationHtml, "id=\"confirm-link\"[^>]*href=\"([^\"]+)\"", "confirm link")
            .Replace("&amp;", "&", StringComparison.Ordinal);
        var confirmUrl = new Uri(confirmUrlString, UriKind.Absolute);

        var confirmResponse = await client.GetAsync(confirmUrl);
        confirmResponse.EnsureSuccessStatusCode();

        var freshLoginResponse = await client.GetAsync(new Uri(identityBaseAddress, "Identity/Account/Login?returnUrl=%2F"));
        freshLoginResponse.EnsureSuccessStatusCode();
        var freshLoginUrl = freshLoginResponse.RequestMessage?.RequestUri
            ?? throw new InvalidOperationException("The fresh login page URL was not resolved.");
        var freshLoginHtml = await freshLoginResponse.Content.ReadAsStringAsync();
        var loginToken = Extract(freshLoginHtml, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"", "login token");

        using var loginContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.Email"] = email,
            ["Input.Password"] = password,
            ["Input.RememberMe"] = "false",
            ["__RequestVerificationToken"] = loginToken
        });

        var postLoginResponse = await client.PostAsync(freshLoginUrl, loginContent);
        postLoginResponse.EnsureSuccessStatusCode();

        var manageResponse = await client.GetAsync(new Uri(identityBaseAddress, "Identity/Account/Manage"));
        manageResponse.EnsureSuccessStatusCode();
        var manageHtml = await manageResponse.Content.ReadAsStringAsync();

        Assert.Contains("Hello", manageHtml, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(email, manageHtml, StringComparison.OrdinalIgnoreCase);
    }

    private static string Extract(string text, string pattern, string label)
    {
        var match = Regex.Match(text, pattern, RegexOptions.Singleline);
        return match.Success
            ? match.Groups[1].Value
            : throw new InvalidOperationException($"Could not extract {label}.");
    }

    private sealed class EnvironmentVariableScope : IDisposable
    {
        private readonly Dictionary<string, string?> _originalValues = new();

        public EnvironmentVariableScope(IReadOnlyDictionary<string, string> values)
        {
            foreach (var (key, value) in values)
            {
                _originalValues[key] = Environment.GetEnvironmentVariable(key);
                Environment.SetEnvironmentVariable(key, value);
            }
        }

        public void Dispose()
        {
            foreach (var (key, value) in _originalValues)
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }
}
