using Newtonsoft.Json.Linq;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.Models.Indicies;

namespace PokemonTabletopAdventures.CoreApi.IntegrationTests;

[TestFixture]
internal class PokedexControllerTests
{
    private static readonly HttpClient HttpClient = new HttpClient();

    [OneTimeSetUp]
    public static void OneTimeSetup()
    {
        HttpClient.BaseAddress = new Uri(Constants.ApiRootUrl);
    }

    [Test]
    public async Task BulbasaurTest()
    {
        var message = new HttpRequestMessageBuilder(HttpMethod.Get, HttpClient.BaseAddress!, "api/v2/pokedex/bulbasaur").Build();
        var responseMessage = await HttpClient.SendAsync(message);
        var content = await responseMessage.Content.ReadAsStringAsync();
        var response = new Response<IndexResponse<BasePokemonDto>>(responseMessage, content);
        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccessful, Is.True);
            Assert.That(response.Data, Is.Not.Null);
        });

        Assert.That(response.Data.Data, Is.Not.Null);
        var data = response.Data.Data;

        Assert.Multiple(() =>
        {
            Assert.That(data.Name.ToLower(), Is.EqualTo("bulbasaur"));
            Assert.That(data.DexNo, Is.EqualTo(1));
            Assert.That(data.PokemonStats.Defense, Is.EqualTo(5));
        });
    }

    [OneTimeTearDown]
    public static void OneTimeTearDown()
    {
        HttpClient.Dispose();
    }
}
