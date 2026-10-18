using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace foodiya.app.Identity.Infrastructure.Persistence;

public sealed class IdentityDataContextFactory : IDesignTimeDbContextFactory<IdentityDataContext>
{
    public IdentityDataContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__IdentityDatabase")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=FOODIYA_APP_IDENTITY;Trusted_Connection=True;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<IdentityDataContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new IdentityDataContext(options);
    }
}
