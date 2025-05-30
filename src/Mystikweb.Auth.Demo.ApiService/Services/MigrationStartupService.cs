using System.Diagnostics;

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Mystikweb.Auth.Demo.ApiService.Services;

public sealed class MigrationStartupService(IServiceProvider serviceProvider) : BackgroundService
{
    public const string ActivitySourceName = "API Service Database Migrations";

    public bool IsCompleted { get; private set; } = false;

    private static readonly ActivitySource _activitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var activity = _activitySource.StartActivity("Setup API Service database.", ActivityKind.Client);

        try
        {
            using var scope = serviceProvider.CreateScope();
            using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            activity?.AddBaggage("Database", dbContext.Database.GetDbConnection().Database);

            await EnsureDatabaseAsync(dbContext, activity, stoppingToken);
            await RunMigrationAsync(dbContext, activity, stoppingToken);

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
