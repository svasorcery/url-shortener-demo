using Xunit;

namespace UrlShortener.Tests.Integration;

[CollectionDefinition(Name)]
public sealed class PostgreSqlCollection : ICollectionFixture<IntegrationTestWebAppFactory>
{
    public const string Name = "PostgreSQL integration tests";
}
