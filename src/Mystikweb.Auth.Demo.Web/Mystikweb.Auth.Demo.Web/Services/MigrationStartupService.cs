using Microsoft.EntityFrameworkCore;

namespace Mystikweb.Auth.Demo.Web.Services;

public sealed class MigrationStartupService(IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Database.EnsureCreatedAsync(stoppingToken);
        await context.Database.MigrateAsync(stoppingToken);
    }
}
