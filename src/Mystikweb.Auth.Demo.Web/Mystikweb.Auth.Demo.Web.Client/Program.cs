using System.Net.Http.Headers;
using System.Net.Mime;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.FluentUI.AspNetCore.Components;

using Mystikweb.Auth.Demo.Web.Shared;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddFluentUIComponents();

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

builder.Services.TryAddSingleton<AuthenticationStateProvider, HostAuthenticationStateProvider>();
builder.Services.TryAddSingleton(provider => (HostAuthenticationStateProvider)provider.GetRequiredService<AuthenticationStateProvider>());

builder.Services.TryAddTransient<AuthorizedHandler>();
builder.Services.AddHttpClient(HttpClientConstants.AddressBookClient, client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
}).AddHttpMessageHandler<AuthorizedHandler>();

builder.Services.AddHttpClient(HttpClientConstants.UserClient, client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
});

builder.Services.AddScoped(sp =>
{
    var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientConstants.AddressBookClient);
    return new AddressBookApiClient(httpClient);
});

builder.Services.AddScoped(sp =>
{
    var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientConstants.UserClient);
    return new UserApiClient(httpClient);
});

await builder.Build().RunAsync();
