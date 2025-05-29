using static OpenIddict.Client.OpenIddictClientModels;

namespace Mystikweb.Auth.Demo.Web.Services;

internal sealed class TokenRefreshingHttpResponseMessage : HttpResponseMessage
{
    public TokenRefreshingHttpResponseMessage(RefreshTokenAuthenticationResult result, HttpResponseMessage response)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(result);

        RefreshTokenAuthenticationResult = result;

        Content = response.Content;
        StatusCode = response.StatusCode;
        Version = response.Version;

        foreach (var header in response.Headers)
        {
            Headers.Add(header.Key, header.Value);
        }
    }

    public RefreshTokenAuthenticationResult RefreshTokenAuthenticationResult { get; }
}
