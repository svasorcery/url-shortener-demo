using Xunit;
using System.Data.Common;
using Testcontainers.PostgreSql;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Api.Persistence;

namespace UrlShortener.Tests.Integration;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer;
    private DbConnection _connection = null!;
    public ApplicationDbContext DbContext { get; private set; } = null!;

    public IntegrationTestWebAppFactory()
    {
        _postgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:latest")
            .WithCleanUp(true)
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveDbContext<ApplicationDbContext>();
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(_postgreSqlContainer.GetConnectionString());
            });
            services.EnsureDbCreated<ApplicationDbContext>();
        });
    }

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();

        DbContext = Services.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>();
        _connection = DbContext.Database.GetDbConnection();
        await _connection.OpenAsync();
    }

    public new async Task DisposeAsync()
    {
        await _connection.CloseAsync();
        await _postgreSqlContainer.DisposeAsync();
    }
}
