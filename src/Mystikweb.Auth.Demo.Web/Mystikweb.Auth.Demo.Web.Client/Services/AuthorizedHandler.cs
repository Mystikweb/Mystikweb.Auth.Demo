using System;
using System.Net;

namespace Mystikweb.Auth.Demo.Web.Client.Services;

public sealed class AuthorizedHandler(HostAuthenticationStateProvider authenticationStateProvider) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
        HttpResponseMessage responseMessage;

            // if user is not authenticated, immediately set response status to 401 Unauthorized
        if (authState.User.Identity == null || authState.User.Identity.IsAuthenticated == false)
            responseMessage = new HttpResponseMessage(HttpStatusCode.Unauthorized);
        else
            responseMessage = await base.SendAsync(request, cancellationToken);

        // if server returned 401 Unauthorized, redirect to login page
        if (responseMessage.StatusCode == HttpStatusCode.Unauthorized)
            authenticationStateProvider.SignIn();

        return responseMessage;
    }
}
