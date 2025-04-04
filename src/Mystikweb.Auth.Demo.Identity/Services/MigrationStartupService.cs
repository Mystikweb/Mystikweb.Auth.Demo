using Microsoft.EntityFrameworkCore;
using Mystikweb.Auth.Demo.Identity.Data;

namespace Mystikweb.Auth.Demo.Identity.Services;

public sealed class MigrationStartupService(IServiceProvider serviceProvider) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		using var scope = serviceProvider.CreateScope();
		var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

		await context.Database.EnsureCreatedAsync(stoppingToken);
		await context.Database.MigrateAsync(stoppingToken);
	}
}
