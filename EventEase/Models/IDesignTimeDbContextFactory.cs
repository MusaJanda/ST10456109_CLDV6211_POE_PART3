using EventEase.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace EventEase // Make sure this namespace matches your project's root namespace
{
    public class EventEaseDBContextFactory : IDesignTimeDbContextFactory<EventEaseDBContext>
    {
        public EventEaseDBContext CreateDbContext(string[] args)
        {
            // Build configuration to read connection string
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json") // Read from appsettings.json
                .AddJsonFile("appsettings.Development.json", optional: true) // Also read development settings
                .Build();

            // Get the connection string explicitly for your Azure DB
            // Make sure this name matches the one in appsettings.json
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // IMPORTANT: If you want to force it to use your Azure DB for migrations,
            // you can hardcode the Azure connection string here temporarily for Update-Database only.
            // REMEMBER TO REMOVE IT AFTER YOU'RE DONE!
            // Example:
            // connectionString = "Server=tcp:musaserver.database.windows.net,1433;Initial Catalog=EventEaseDB;Persist Security Info=False;User ID=Musa;Password=@Janda123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

            var builder = new DbContextOptionsBuilder<EventEaseDBContext>();
            builder.UseSqlServer(connectionString);

            return new EventEaseDBContext(builder.Options);
        }
    }
}