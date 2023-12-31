using Microsoft.EntityFrameworkCore.Design;

namespace SKD.Domain;
public class DbContextFactory : IDesignTimeDbContextFactory<SkdContext> {
    public SkdContext CreateDbContext(string[] args) {

        var Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = Configuration.GetConnectionString("DefaultConnection");

        if (connectionString == null) {
            throw new Exception($"Default connection string not found for Development");
        }

        var optionsBuilder = new DbContextOptionsBuilder<SkdContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new SkdContext(optionsBuilder.Options);
    }
}
