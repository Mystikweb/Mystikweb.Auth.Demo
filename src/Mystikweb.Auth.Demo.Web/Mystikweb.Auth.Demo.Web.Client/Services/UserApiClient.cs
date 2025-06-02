using System.Net.Http.Json;

using Mystikweb.Auth.Demo.Web.Shared.Models;

namespace Mystikweb.Auth.Demo.Web.Client.Services;

internal sealed class UserApiClient(HttpClient httpClient) : IUserApiClient
{
    public async Task<UserInfo> GetCurrentUserAsync(CancellationToken cancellationToken = default) =>
        await httpClient.GetFromJsonAsync<UserInfo>("user", cancellationToken)
            ?? throw new InvalidOperationException("Failed to retrieve user information.");
}
