using Microsoft.EntityFrameworkCore;

using OpenIddict.Abstractions;

namespace Mystikweb.Auth.Demo.Identity.Services;

public sealed class MigrationStartupService(IHostEnvironment hostEnvironment,
    IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Database.EnsureCreatedAsync(stoppingToken);
        await context.Database.MigrateAsync(stoppingToken);

        // Seed the database with initial data when running in development
        if (hostEnvironment.IsDevelopment())
            await ConfigureDevelopmentApplicationsAsync(scope.ServiceProvider, stoppingToken);
    }

    private static async Task ConfigureDevelopmentApplicationsAsync(IServiceProvider provider, CancellationToken cancellationToken)
    {
        var applicationManager = provider.GetRequiredService<IOpenIddictApplicationManager>();

        if (await applicationManager.FindByClientIdAsync(DevelopmentApplications.ApiResource.ClientId!, cancellationToken) is null)
            await applicationManager.CreateAsync(DevelopmentApplications.ApiResource, cancellationToken);

        if (await applicationManager.FindByClientIdAsync(DevelopmentApplications.BlazorApplication.ClientId!, cancellationToken) is null)
            await applicationManager.CreateAsync(DevelopmentApplications.BlazorApplication, cancellationToken);

        var scopeManager = provider.GetRequiredService<IOpenIddictScopeManager>();

        if (await scopeManager.FindByNameAsync(DevelopmentApplications.ApiScope.Name!, cancellationToken) is null)
            await scopeManager.CreateAsync(DevelopmentApplications.ApiScope, cancellationToken);
    }
}
