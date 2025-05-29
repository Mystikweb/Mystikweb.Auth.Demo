namespace Mystikweb.Auth.Demo;

public static class ServiceConstants
{
    public const string AUTHORIZATION_POLICY_NAME = "CookieAuthenticationPolicy";
    public const string DATABASE_MANAGEMENT_RESOURCE_NAME = "identity-db-management";
    public const string IDENTITY_URI_ENVIRONMENT_VARIABLE = "IDENTITY_URI";

    public static class CacheService
    {
        public const string RESOURCE_NAME = "cache";
        public const string RESOURCE_CONTAINER_NAME = "cache-demo";
        public const string RESOURCE_DATA_VOLUME = "cache-data";
    }

    public static class IdentityService
    {
        public const string DATABASE_SERVER_PASSWORD_PARAMETER = "IdentityDbPassword";
        public const string DATABASE_SERVER_USERNAME_PARAMETER = "IdentityDbUsername";
        public const string DATABASE_SERVER_RESOURCE_NAME = "identity";
        public const int DATABASE_SERVER_PORT = 5450;
        public const string DATABASE_SERVER_CONTAINER_NAME = "identity-db-demo";
        public const string DATABASE_SERVER_DATA_VOLUME = "identity-data";
        public const string DATABASE_RESOURCE_NAME = "identitydb";
        public const string DATABASE_NAME = "identity_db";
        public const string SERVER_RESOURCE_NAME = "identityserver";
    }

    public static class ApiService
    {
        public const string DATABASE_SERVER_USERNAME_PARAMETER = "ApiDbUsername";
        public const string DATABASE_SERVER_PASSWORD_PARAMETER = "ApiDbPassword";
        public const string DATABASE_SERVER_RESOURCE_NAME = "api";
        public const int DATABASE_SERVER_PORT = 5451;
        public const string DATABASE_SERVER_CONTAINER_NAME = "api-db-demo";
        public const string DATABASE_SERVER_DATA_VOLUME = "api-data";
        public const string DATABASE_RESOURCE_NAME = "apidb";
        public const string DATABASE_NAME = "api_db";
        public const string SERVER_RESOURCE_NAME = "apiservice";
    }

    public static class BlazorService
    {
        public const string DATABASE_SERVER_USERNAME_PARAMETER = "BlazorDbUsername";
        public const string DATABASE_SERVER_PASSWORD_PARAMETER = "BlazorDbPassword";
        public const string DATABASE_SERVER_RESOURCE_NAME = "blazor";
        public const int DATABASE_SERVER_PORT = 5452;
        public const string DATABASE_SERVER_CONTAINER_NAME = "blazor-db-demo";
        public const string DATABASE_SERVER_DATA_VOLUME = "blazor-data";
        public const string DATABASE_RESOURCE_NAME = "blazordb";
        public const string DATABASE_NAME = "blazor_db";
        public const string SERVER_RESOURCE_NAME = "blazorfrontend";
    }
}

