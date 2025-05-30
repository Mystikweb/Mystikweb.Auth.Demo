using Mystikweb.Auth.Demo;

using OpenIddict.Validation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<ApplicationDbContext>(ServiceConstants.ApiService.DATABASE_RESOURCE_NAME);
builder.AddRedisOutputCache(ServiceConstants.CacheService.RESOURCE_NAME);

builder.Services.AddScoped<IAddressBookLogic, AddressBookLogic>();

builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddOpenIddict()
    .AddValidation(options =>
    {
        // Add the client registraiton matching the client application for the identity server.
        var issuerString = builder.Configuration.GetValue<string>(ServiceConstants.IDENTITY_URI_ENVIRONMENT_VARIABLE);
        if (string.IsNullOrEmpty(issuerString))
            throw new InvalidOperationException($"The configuration value for '{ServiceConstants.IDENTITY_URI_ENVIRONMENT_VARIABLE}' is missing or empty.");

        options.SetIssuer(new Uri(issuerString, UriKind.Absolute));
        options.AddAudiences(DevelopmentApplications.ApiResource.ClientId!);

        // Configure the validation handler to use introspection and register the client
        // credentials used when communicating with the remote introspection endpoint.
        options.UseIntrospection()
               .SetClientId(DevelopmentApplications.ApiResource.ClientId!)
               .SetClientSecret(DevelopmentApplications.ApiResource.ClientSecret!);

        options.UseSystemNetHttp();

        options.UseAspNetCore();
    });

builder.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
builder.Services.AddAuthorization();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
        tracing.AddSource(MigrationStartupService.ActivitySourceName));

builder.Services.AddHostedService<MigrationStartupService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "v1"));
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();

app.MapGroup("/api")
    .MapAddressBookEndpoints();

app.Run();
