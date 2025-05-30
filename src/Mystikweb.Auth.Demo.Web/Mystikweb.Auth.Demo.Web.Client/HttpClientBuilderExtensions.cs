using System.Net.Http.Headers;
using System.Net.Mime;

using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Mystikweb.Auth.Demo.Web.Client;

internal static class HttpClientBuilderExtensions
{
    internal static IServiceCollection AddHttpClientWithJson<TInterface, TImplementation>(this IServiceCollection services, Uri baseAddress, bool useAuthorizedHandler = false)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        var clientBuilder = services.AddHttpClient(nameof(TImplementation), client =>
        {
            client.BaseAddress = baseAddress;
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
        });

        if (useAuthorizedHandler)
        {
            services.TryAddTransient<AuthorizedHandler>();
            clientBuilder.AddHttpMessageHandler<AuthorizedHandler>();
        }

        services.AddScoped<TInterface, TImplementation>(sp =>
        {
            var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(TImplementation));
            return (TImplementation)Activator.CreateInstance(typeof(TImplementation), client)!;
        });

        return services;
    }
}
