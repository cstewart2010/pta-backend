using Allure.Net.Commons;
using Allure.NUnit.Attributes;
using PokemonTabletopAdventures.CoreApi.Controllers.v2;
using PokemonTabletopAdventures.Models.Indicies;

namespace PokemonTabletopAdventures.CoreApi.IntegrationTests.Tests;

[TestFixture]
[AllureFeature(nameof(BasePokemonController))]
[AllureSuite(nameof(BasePokemonController))]
internal class BasePokemonControllerTests : BaseTest
{
    private HttpClientHelper _client;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _client = new HttpClientHelper();
    }

    [Test]
    [AllureSubSuite(nameof(BasePokemonController.GetPokemonByName))]
    [AllureName("/{name}")]
    public async Task GetPokemonByName_Valid_ReturnsPokemonAndForms()
    {
        var builder = AllureUtility.Arrange(
            "Build HTTP GET request for api/v2/pokedex/bulbasaur",
            () => new HttpRequestMessageBuilder("api/v2/pokedex/bulbasaur"));
        var response = await AllureUtility.Act(
            "Send request",
            () => _client.SendRequestAsync<PokemonAndForms>(builder));
        AllureUtility.Assert(
            "Validate response is type PokemonAndForms and contains expected data",
            () =>
            {
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(response.IsSuccessful, Is.True);
                    Assert.That(response.Data, Is.Not.Null);
                }

                Assert.That(response.Data.Pokemon, Is.Not.Null);
                var data = response.Data.Pokemon;

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(data.Name.ToLower(), Is.EqualTo("bulbasaur"));
                    Assert.That(data.DexNo, Is.EqualTo(1));
                    Assert.That(data.PokemonStats.Defense, Is.EqualTo(5));
                    Assert.That(response.Data.AlternateForms, Is.Empty);
                }
            });
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client.Dispose();
    }
}
