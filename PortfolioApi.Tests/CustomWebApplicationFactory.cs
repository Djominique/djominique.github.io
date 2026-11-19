
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PortfolioApi.Data;

namespace PortfolioApi.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string TestConnection =
        @"Server=DESKTOP-UC9ROU3\SQLEXPRESS;Database=PortfolioTestDb;Trusted_Connection=True;TrustServerCertificate=True";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureTestServices(services =>
        {
            // Verwijder de normale databaseconfiguratie.
            services.RemoveAll<
                IDbContextOptionsConfiguration<PortfolioDbContext>>();

            services.RemoveAll<
                DbContextOptions<PortfolioDbContext>>();

            // Gebruik uitsluitend de testdatabase.
            services.AddDbContext<PortfolioDbContext>(options =>
                options.UseSqlServer(TestConnection));
        });
    }
}
