using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using UrlShortener.Api.Services;
using Xunit;

namespace UrlShortener.Tests.Integration.Services;

[Collection(PostgreSqlCollection.Name)]
public class UrlShorteningServiceTests(IntegrationTestWebAppFactory factory)
{
    [Fact]
    public async Task GenerateUniqueCode_ShouldUseFullBase62Alphabet()
    {
        const string expectedAlphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        using var scope = factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<UrlShorteningService>();
        var observedCharacters = new HashSet<char>();

        for (var sample = 0; sample < 1_000; sample++)
        {
            var code = await service.GenerateUniqueCodeAsync(TestContext.Current.CancellationToken);
            observedCharacters.UnionWith(code);
        }

        new string(observedCharacters.Order().ToArray())
            .ShouldBe(new string(expectedAlphabet.Order().ToArray()));
    }
}
