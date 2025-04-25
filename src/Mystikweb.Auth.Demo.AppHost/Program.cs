using Mystikweb.Auth.Demo;

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis(ServiceConstants.CacheService.RESOURCE_NAME)
	.WithContainerName(ServiceConstants.CacheService.RESOURCE_CONTAINER_NAME)
	.WithDataVolume(ServiceConstants.CacheService.RESOURCE_DATA_VOLUME)
	.WithLifetime(ContainerLifetime.Persistent)
	.WithDbGate(containerName: ServiceConstants.DATABASE_MANAGEMENT_RESOURCE_NAME);

var identityDbUsername = builder.AddParameter(ServiceConstants.IdentityService.DATABASE_SERVER_USERNAME_PARAMETER, true);
var identityDbPassword = builder.AddParameter(ServiceConstants.IdentityService.DATABASE_SERVER_PASSWORD_PARAMETER, true);
var identityDbServer = builder.AddPostgres(ServiceConstants.IdentityService.DATABASE_SERVER_RESOURCE_NAME, identityDbUsername, identityDbPassword, ServiceConstants.IdentityService.DATABASE_SERVER_PORT)
	.WithContainerName(ServiceConstants.IdentityService.DATABASE_SERVER_CONTAINER_NAME)
	.WithDataVolume(ServiceConstants.IdentityService.DATABASE_SERVER_DATA_VOLUME)
	.WithLifetime(ContainerLifetime.Persistent)
	.WithDbGate(containerName: ServiceConstants.DATABASE_MANAGEMENT_RESOURCE_NAME);

var identityDb = identityDbServer
	.AddDatabase(ServiceConstants.IdentityService.DATABASE_RESOURCE_NAME, ServiceConstants.IdentityService.DATABASE_NAME);

var identity = builder.AddProject<Projects.Mystikweb_Auth_Demo_Identity>(ServiceConstants.IdentityService.SERVER_RESOURCE_NAME)
	.WithReference(cache)
	.WaitFor(cache)
    .WithReference(identityDb)
	.WaitFor(identityDb);

var apiDbUsername = builder.AddParameter(ServiceConstants.ApiService.DATABASE_SERVER_USERNAME_PARAMETER, true);
var apiDbPassword = builder.AddParameter(ServiceConstants.ApiService.DATABASE_SERVER_PASSWORD_PARAMETER, true);
var apiDbServer = builder.AddPostgres(ServiceConstants.ApiService.DATABASE_SERVER_RESOURCE_NAME, apiDbUsername, apiDbPassword, 5451)
	.WithContainerName(ServiceConstants.ApiService.DATABASE_SERVER_CONTAINER_NAME)
	.WithDataVolume(ServiceConstants.ApiService.DATABASE_SERVER_DATA_VOLUME)
	.WithLifetime(ContainerLifetime.Persistent)
	.WithDbGate(containerName: ServiceConstants.DATABASE_MANAGEMENT_RESOURCE_NAME);

var apiDb = apiDbServer
	.AddDatabase(ServiceConstants.ApiService.DATABASE_RESOURCE_NAME, ServiceConstants.ApiService.DATABASE_NAME);

var apiService = builder.AddProject<Projects.Mystikweb_Auth_Demo_ApiService>(ServiceConstants.ApiService.SERVER_RESOURCE_NAME)
    .WithReference(identity)
	.WaitFor(identity)
	.WithReference(apiDb)
	.WaitFor(apiDb);

builder.AddProject<Projects.Mystikweb_Auth_Demo_Web>(ServiceConstants.WebService.SERVER_RESOURCE_NAME)
	.WithExternalHttpEndpoints()
	.WithReference(cache)
	.WaitFor(cache)
	.WithReference(apiService)
	.WaitFor(apiService)
	.WithReference(identity)
	.WaitFor(identity);

builder.Build().Run();
