using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Mystikweb.Auth.Demo.Web.Data;

public class ApplicationDbContext(DbContextOptions options) : DbContext(options)
{
}

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=Blazor;Username=postgres;Password=yourpassword");

        optionsBuilder.UseOpenIddict();

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
