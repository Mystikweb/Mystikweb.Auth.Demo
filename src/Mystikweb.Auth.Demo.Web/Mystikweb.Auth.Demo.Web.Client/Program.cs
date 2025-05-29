using System.Net.Http.Headers;
using System.Net.Mime;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.FluentUI.AspNetCore.Components;

using Mystikweb.Auth.Demo.Web.Client.Services;
using Mystikweb.Auth.Demo.Web.Shared.Abstractions;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddFluentUIComponents();

builder.Services.AddBlazoredLocalStorageAsSingleton();

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

builder.Services.TryAddSingleton<AuthenticationStateProvider, HostAuthenticationStateProvider>();
builder.Services.TryAddSingleton(provider => (HostAuthenticationStateProvider)provider.GetRequiredService<AuthenticationStateProvider>());
builder.Services.AddTransient<AuthorizedHandler>();

builder.Services.AddHttpClient<IUserApiClient, UserApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
});


await builder.Build().RunAsync();
