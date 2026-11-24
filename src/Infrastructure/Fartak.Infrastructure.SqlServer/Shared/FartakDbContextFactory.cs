using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Fartak.Infrastructure.SqlServer.Shared
{
    internal class FartakDbContextFactory : IDesignTimeDbContextFactory<FartakDbContext>
    {
        public FartakDbContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? "Server=.;Database=FartakDb;Trusted_Connection=True;TrustServerCertificate=True;";

            var optionsBuilder = new DbContextOptionsBuilder<FartakDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new FartakDbContext(optionsBuilder.Options);
        }
    }
}
