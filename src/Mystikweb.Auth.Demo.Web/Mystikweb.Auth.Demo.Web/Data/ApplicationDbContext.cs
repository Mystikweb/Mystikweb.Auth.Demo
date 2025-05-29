using Microsoft.EntityFrameworkCore;

namespace Mystikweb.Auth.Demo.Web.Data;

public class ApplicationDbContext(DbContextOptions options) : DbContext(options)
{
}
