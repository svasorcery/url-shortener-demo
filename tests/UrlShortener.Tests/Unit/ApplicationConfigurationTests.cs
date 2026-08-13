using Microsoft.Extensions.Configuration;
using Shouldly;
using Xunit;

namespace UrlShortener.Tests.Unit;

public class ApplicationConfigurationTests
{
    [Fact]
    public void Appsettings_ShouldProvidePostgresConnectionString()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

        configuration.GetConnectionString("postgres")
            .ShouldBe("Host=localhost;Port=5432;Database=urlshortener;Username=postgres;Password=postgres");
    }
}
