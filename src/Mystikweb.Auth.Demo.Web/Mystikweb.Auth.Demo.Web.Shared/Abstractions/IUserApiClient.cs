using Mystikweb.Auth.Demo.Web.Shared.Models;

namespace Mystikweb.Auth.Demo.Web.Shared.Abstractions;

public interface IUserApiClient
{
    /// <summary>
    /// Gets the current user information.
    /// </summary>
    /// <returns>The current user information.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when the user is not authenticated.</exception>
    Task<UserInfo> GetCurrentUserAsync(CancellationToken cancellationToken = default);
}
