using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.FluentUI.AspNetCore.Components;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.Services.AddFluentUIComponents();

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

builder.Services.TryAddSingleton<AuthenticationStateProvider, HostAuthenticationStateProvider>();
builder.Services.TryAddSingleton(provider => (HostAuthenticationStateProvider)provider.GetRequiredService<AuthenticationStateProvider>());

builder.Services.AddHttpClientWithJson<IUserApiClient, UserApiClient>(
    new Uri(builder.HostEnvironment.BaseAddress));

builder.Services.AddHttpClientWithJson<IAddressBookApiClient, AddressBookApiClient>(
    new Uri(builder.HostEnvironment.BaseAddress), true);

await builder.Build().RunAsync();
