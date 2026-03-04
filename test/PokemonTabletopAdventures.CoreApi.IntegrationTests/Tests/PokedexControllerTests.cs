using Allure.NUnit.Attributes;
using PokemonTabletopAdventures.Models.Indicies;

namespace PokemonTabletopAdventures.CoreApi.IntegrationTests.Tests;

[TestFixture]
[AllureFeature("Pokedex")]
internal class PokedexControllerTests : BaseTest
{
    private HttpClientHelper _client;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _client = new HttpClientHelper();
    }

    [Test]
    public async Task BulbasaurTest()
    {
        var request = AllureUtility.Arrange(
            "Build request",
            () => new HttpRequestMessageBuilder(HttpMethod.Get, "api/v2/pokedex/bulbasaur").Build());
        var response = await AllureUtility.Act(
            "Send request",
            () => _client.SendRequestAsync<PokemonAndForms>(request));
        AllureUtility.Assert(
            "Validate response",
            () =>
            {
                Assert.Multiple(() =>
                {
                    Assert.That(response.IsSuccessful, Is.True);
                    Assert.That(response.Data, Is.Not.Null);
                });

                Assert.That(response.Data.Pokemon, Is.Not.Null);
                var data = response.Data.Pokemon;

                Assert.Multiple(() =>
                {
                    Assert.That(data.Name.ToLower(), Is.EqualTo("bulbasaur"));
                    Assert.That(data.DexNo, Is.EqualTo(1));
                    Assert.That(data.PokemonStats.Defense, Is.EqualTo(5));
                });
            });
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client.Dispose();
    }
}
