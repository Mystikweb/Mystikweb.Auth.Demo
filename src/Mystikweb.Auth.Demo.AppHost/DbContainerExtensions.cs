using Mystikweb.Auth.Demo;

namespace Aspire.Hosting;

internal static class DbContainerExtensions
{
    public static IResourceBuilder<RedisResource> AddDbGateReference(this IResourceBuilder<RedisResource> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithDbGate(configure =>
            configure.WithHostPort(ServiceConstants.DATABASE_MANAGEMENT_RESOURCE_PORT), ServiceConstants.DATABASE_MANAGEMENT_RESOURCE_NAME);
    }

    public static IResourceBuilder<PostgresServerResource> AddDbGateReference(this IResourceBuilder<PostgresServerResource> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithDbGate(configure =>
            configure.WithHostPort(ServiceConstants.DATABASE_MANAGEMENT_RESOURCE_PORT), ServiceConstants.DATABASE_MANAGEMENT_RESOURCE_NAME);
    }
}
