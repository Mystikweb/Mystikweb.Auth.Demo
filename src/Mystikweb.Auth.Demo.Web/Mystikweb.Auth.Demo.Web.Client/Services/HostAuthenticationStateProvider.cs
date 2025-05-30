using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

using Mystikweb.Auth.Demo.Web.Shared;
using Mystikweb.Auth.Demo.Web.Shared.Models;

namespace Mystikweb.Auth.Demo.Web.Client.Services;

public sealed class HostAuthenticationStateProvider(ILogger<HostAuthenticationStateProvider> logger,
    IServiceScopeFactory scopeFactory,
    NavigationManager navigationManager,
    IJSRuntime jsRuntime) : AuthenticationStateProvider, IAsyncDisposable
{
    private static readonly TimeSpan UserCahceRefrehInterval = TimeSpan.FromSeconds(60);
    private const string LogInPath = "Account/Login";
    // private const string LogOutPath = "Account/Logout";

    private DateTimeOffset _userLastCheck = DateTimeOffset.FromUnixTimeSeconds(0);
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
        "import", "./_content/Mystikweb.Auth.Demo.Web.Client/userCacheStorage.js").AsTask());

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var user = await GetUserAsync();
        return new AuthenticationState(user.ToClaimsPrincipal());
    }

    public void SignIn(string? customReturnUrl = null)
    {
        var returnUrl = customReturnUrl != null ? navigationManager.ToAbsoluteUri(customReturnUrl).ToString() : null;
        var encodedReturnUrl = Uri.EscapeDataString(returnUrl ?? navigationManager.Uri);
        var logInUrl = navigationManager.ToAbsoluteUri($"{LogInPath}?returnUrl={encodedReturnUrl}");
        navigationManager.NavigateTo(logInUrl.ToString(), true);
    }

    public async ValueTask DisposeAsync()
    {
        if (_moduleTask.IsValueCreated)
        {
            var module = await _moduleTask.Value;
            await module.DisposeAsync();
        }
    }

    private async Task<IJSObjectReference> GetModuleAsync() => await _moduleTask.Value;

    private async ValueTask<UserInfo> GetUserAsync(CancellationToken cancellationToken = default)
    {
        var currentTime = DateTimeOffset.UtcNow;
        var module = await GetModuleAsync();

        UserInfo result;
        if (currentTime < _userLastCheck + UserCahceRefrehInterval)
        {
            var isUserCached = await module.InvokeAsync<bool>("isUserCached");
            if (isUserCached)
            {
                result = await module.InvokeAsync<UserInfo>("getCachedUser");
                if (result is not null)
                {
                    logger.LogInformation("Returning cached user.");
                    _userLastCheck = currentTime;
                    return result;
                }
            }
        }

        logger.LogInformation("Fetching user from API.");

        using var scope = scopeFactory.CreateScope();
        var userApiClient = scope.ServiceProvider.GetRequiredService<IUserApiClient>();

        result = await userApiClient.GetCurrentUserAsync(cancellationToken);

        if (result is null)
        {
            logger.LogWarning("No user found in API, returning anonymous user.");
            await module.InvokeVoidAsync("clearUserCache");
            result = UserInfo.Anonymous;
        }
        else
        {
            await module.InvokeVoidAsync("cacheUser", result);
            logger.LogInformation("User fetched and cached successfully.");
        }

        _userLastCheck = currentTime;

        return result;
    }
}
