using System.Diagnostics;

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

using RandomDataGenerator.FieldOptions;
using RandomDataGenerator.Randomizers;

namespace Mystikweb.Auth.Demo.ApiService.Services;

public sealed class MigrationStartupService(IServiceProvider serviceProvider) : BackgroundService
{
    public const string ActivitySourceName = "API Service Database Migrations";

    public bool IsCompleted { get; private set; } = false;

    private static readonly ActivitySource _activitySource = new(ActivitySourceName);

    private static readonly IRandomizerNumber<int> _recordCountGenerator = RandomizerFactory.GetRandomizer(new FieldOptionsInteger
    {
        Min = 10,
        Max = 100
    });
    private static readonly IRandomizerNumber<int> _addressCountGenerator = RandomizerFactory.GetRandomizer(new FieldOptionsInteger
    {
        Min = 1,
        Max = 2
    });

    private static readonly IRandomizerString _firstNameGenerator = RandomizerFactory.GetRandomizer(new FieldOptionsFirstName());
    private static readonly IRandomizerString _lastNameGenerator = RandomizerFactory.GetRandomizer(new FieldOptionsLastName());
    private static readonly IRandomizerDateTime _birthDateGenerator = RandomizerFactory.GetRandomizer(new FieldOptionsDateTime());
    private static readonly IRandomizerString _emailGenerator = RandomizerFactory.GetRandomizer(new FieldOptionsEmailAddress());
    private static readonly IRandomizerString _addressLineGenerator = RandomizerFactory.GetRandomizer(new FieldOptionsText
    {
        Min = 10,
        Max = 50,
        UseSpecial = false
    });
    private static readonly IRandomizerString _cityGenerator = RandomizerFactory.GetRandomizer(new FieldOptionsCity());
    private static readonly IRandomizerString _stateGenerator = RandomizerFactory.GetRandomizer(new FieldOptionsCountry());
    private static readonly IRandomizerString _countryGenerator = RandomizerFactory.GetRandomizer(new FieldOptionsCountry());
    private static readonly IRandomizerString _postalCodeGenerator = RandomizerFactory.GetRandomizer(new FieldOptionsTextRegex
    {
        Pattern = @"^\d{5}(-\d{4})?$", // US ZIP code format
    });


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
            await SeedDataAsync(dbContext, activity, stoppingToken);

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

    private static async Task SeedDataAsync(ApplicationDbContext dbContext, Activity? activity, CancellationToken cancellationToken)
    {
        activity?.AddEvent(new ActivityEvent("Seeding initial data"));

        var personCount = _recordCountGenerator.Generate() ?? 10;

        var insertUser = nameof(MigrationStartupService);
        var insertAt = Instant.FromDateTimeOffset(DateTimeOffset.UtcNow);

        foreach (var i in Enumerable.Range(1, personCount))
        {
            activity?.AddEvent(new ActivityEvent("Inserting person record.", tags: [
                new KeyValuePair<string, object?>("PersonIndex", i)
            ]));

            try
            {
                var person = new Person
                {
                    FirstName = _firstNameGenerator.Generate() ?? $"FirstName{i}",
                    LastName = _lastNameGenerator.Generate() ?? $"LastName{i}",
                    BirthDate = LocalDateTime.FromDateTime(_birthDateGenerator.Generate() ?? DateTime.UtcNow.AddYears(i)),
                    Email = _emailGenerator.Generate() ?? "something went wrong",
                    InsertBy = insertUser,
                    InsertAt = insertAt
                };

                await dbContext.People.AddAsync(person, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);

                var addressCount = _addressCountGenerator.Generate() ?? 1;

                foreach (var a in Enumerable.Range(1, addressCount))
                {
                    activity?.AddEvent(new ActivityEvent("Inserting person address record.", tags: [
                        new KeyValuePair<string, object?>("PersonIndex", i),
                    new KeyValuePair<string, object?>("AddressIndex", a)
                    ]));

                    var address = new Address
                    {
                        PersonId = person.Id,
                        Line1 = _addressLineGenerator.Generate() ?? $"Address Line {a} for Person {i}",
                        City = _cityGenerator.Generate() ?? "City",
                        State = _stateGenerator.Generate() ?? "State",
                        Country = _countryGenerator.Generate() ?? "Country",
                        PostalCode = _postalCodeGenerator.Generate() ?? "Postal Code",
                        InsertBy = insertUser,
                        InsertAt = insertAt
                    };

                    await dbContext.Addresses.AddAsync(address, cancellationToken);
                }

                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                activity?.AddException(ex, [
                    new KeyValuePair<string, object?>("PersonIndex", i)
                ]);
            }
        }
    }
}
