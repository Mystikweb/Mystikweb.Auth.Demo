using System.Diagnostics;

using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

using OpenIddict.Abstractions;

namespace Mystikweb.Auth.Demo.Identity.Services;

public sealed class MigrationStartupService(IHostEnvironment hostEnvironment,
    IConfiguration configuration,
    IServiceProvider serviceProvider) : BackgroundService
{
    public const string ActivitySourceName = "Identity Service Database Migrations";

    public bool IsCompleted { get; private set; } = false;

    private static readonly ActivitySource _activitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var activity = _activitySource.StartActivity("Setup Identity Service database.", ActivityKind.Client);

        try
        {
            using var scope = serviceProvider.CreateScope();
            using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            activity?.AddBaggage("Database", dbContext.Database.GetDbConnection().Database);

            await EnsureDatabaseAsync(dbContext, activity, stoppingToken);
            await RunMigrationAsync(dbContext, activity, stoppingToken);

            // Seed the database with initial data when running in development
            if (hostEnvironment.IsDevelopment())
                await ConfigureDevelopmentApplicationsAsync(scope.ServiceProvider, configuration, activity, stoppingToken);

            activity?.AddEvent(new ActivityEvent("Database migration complete"));
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex) when (ex is not OperationCanceledException or TaskCanceledException)
        {
            activity?.AddException(ex);
            activity?.SetStatus(ActivityStatusCode.Error);
        }

        activity?.Stop();

        IsCompleted = true;
    }

    private static async Task ConfigureDevelopmentApplicationsAsync(IServiceProvider provider, IConfiguration configuration, Activity? activity, CancellationToken cancellationToken)
    {
        activity?.AddEvent(new ActivityEvent("Configuring development identity application resources."));

        var applicationManager = provider.GetRequiredService<IOpenIddictApplicationManager>();
        var frontendUri = ResolveBlazorFrontendUri(configuration);

        if (await applicationManager.FindByClientIdAsync(DevelopmentApplications.ApiResource.ClientId!, cancellationToken) is null)
        {
            activity?.AddEvent(new ActivityEvent("Creating API resource application."));
            await applicationManager.CreateAsync(DevelopmentApplications.ApiResource, cancellationToken);
        }
        else
            activity?.AddEvent(new ActivityEvent("API resource application already exists."));

        var blazorApplication = DevelopmentApplications.CreateBlazorApplication(frontendUri);
        var existingBlazorApplication = await applicationManager.FindByClientIdAsync(blazorApplication.ClientId!, cancellationToken);
        if (existingBlazorApplication is null)
        {
            activity?.AddEvent(new ActivityEvent("Creating Blazor resource application."));
            await applicationManager.CreateAsync(blazorApplication, cancellationToken);
        }
        else
        {
            activity?.AddEvent(new ActivityEvent("Updating Blazor resource application redirect URIs."));
            await applicationManager.UpdateAsync(existingBlazorApplication, blazorApplication, cancellationToken);
        }

        var scopeManager = provider.GetRequiredService<IOpenIddictScopeManager>();

        if (await scopeManager.FindByNameAsync(DevelopmentApplications.ApiScope.Name!, cancellationToken) is null)
        {
            activity?.AddEvent(new ActivityEvent("Creating API scope."));
            await scopeManager.CreateAsync(DevelopmentApplications.ApiScope, cancellationToken);
        }
        else
            activity?.AddEvent(new ActivityEvent("API scope already exists."));
    }

    private static Uri ResolveBlazorFrontendUri(IConfiguration configuration)
    {
        var frontendUri = configuration.GetValue<string>(ServiceConstants.BLAZOR_FRONTEND_URI_ENVIRONMENT_VARIABLE);
        return Uri.TryCreate(frontendUri, UriKind.Absolute, out var uri)
            ? uri
            : new Uri("https://localhost:7211/", UriKind.Absolute);
    }

    private static async Task EnsureDatabaseAsync(ApplicationDbContext dbContext, Activity? activity, CancellationToken cancellationToken)
    {
        var dbCreator = dbContext.GetService<IRelationalDatabaseCreator>();

        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            // Create the database if it does not exist.
            // Do this first so there is then a database to start a transaction against.
            if (!await dbCreator.ExistsAsync(cancellationToken))
            {
                activity?.AddEvent(new ActivityEvent("Creating database"));
                await dbCreator.CreateAsync(cancellationToken);
            }
        });
    }

    private static async Task RunMigrationAsync(ApplicationDbContext dbContext, Activity? activity, CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
            if (pendingMigrations.Any())
            {
                foreach (var migration in pendingMigrations)
                {
                    activity?.AddEvent(new ActivityEvent("Applying migration.", tags: [
                        new KeyValuePair<string, object?>("Migration", migration)
                    ]));
                    await dbContext.Database.MigrateAsync(migration, cancellationToken);
                }
            }
        });
    }
}
