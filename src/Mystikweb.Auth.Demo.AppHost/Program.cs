var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache")
	.WithContainerName("cache-demo")
	.WithDataVolume("cache-data", false)
	.WithLifetime(ContainerLifetime.Persistent);

var identityDbUsername = builder.AddParameter("IdentityDbUsername", true);
var identityDbPassword = builder.AddParameter("IdentityDbPassword", true);
var identityDbServer = builder.AddPostgres("identity", identityDbUsername, identityDbPassword, 5450)
	.WithContainerName("identity-db-demo")
	.WithDataVolume("identity-data", false)
	.WithLifetime(ContainerLifetime.Persistent)
	.WithPgAdmin(configure =>
	{
		configure.WithContainerName("pgadmin-identity-demo");
		configure.WithHostPort(5455);
		configure.WithImage("dpage/pgadmin4", "latest");
	});

var identityDb = identityDbServer
	.AddDatabase("identitydb", "identity_db");

var identity = builder.AddProject<Projects.Mystikweb_Auth_Demo_Identity>("identityserver")
	.WithReference(cache)
	.WaitFor(cache)
    .WithReference(identityDb)
	.WaitFor(identityDb);

var apiDbUsername = builder.AddParameter("ApiDbUsername", true);
var apiDbPassword = builder.AddParameter("ApiDbPassword", true);
var apiDbServer = builder.AddPostgres("api", apiDbUsername, apiDbPassword, 5451)
	.WithContainerName("api-db-demo")
	.WithDataVolume("api-data", false)
	.WithLifetime(ContainerLifetime.Persistent)
	.WithPgAdmin(configure =>
	{
		configure.WithContainerName("pgadmin-identity-demo");
		configure.WithHostPort(5455);
		configure.WithImage("dpage/pgadmin4", "latest");
	});

var apiDb = apiDbServer
	.AddDatabase("apidb", "api_db");

var apiService = builder.AddProject<Projects.Mystikweb_Auth_Demo_ApiService>("apiservice")
    .WithReference(identity)
	.WaitFor(identity)
	.WithReference(apiDb)
	.WaitFor(apiDb);

builder.AddProject<Projects.Mystikweb_Auth_Demo_Web>("webfrontend")
	.WithExternalHttpEndpoints()
	.WithReference(cache)
	.WaitFor(cache)
	.WithReference(apiService)
	.WaitFor(apiService)
	.WithReference(identity)
	.WaitFor(identity);

builder.Build().Run();
