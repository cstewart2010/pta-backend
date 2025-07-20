using Microsoft.Extensions.Logging;
using NSubstitute;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Domain;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.CoreApi.UnitTests.Implementations;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Services;

internal class PokemonServiceTests
{
    private PokedexService pokedexService;
    private PokedexCollectionImpl pokedexCollection;
    private PokemonService sut;
    private PokemonCollectionImpl pokemonCollection;

    [OneTimeSetUp]
    public void SetUp()
    {
        var mockRepository = Substitute.For<IRepositoryService>();
        pokedexCollection = new PokedexCollectionImpl();
        pokemonCollection = new PokemonCollectionImpl();
        mockRepository.GetCollection<PokeDexItemDto>(MongoCollection.Pokedex).Returns(pokedexCollection);
        mockRepository.GetCollection<PokemonDto>(MongoCollection.Pokemon).Returns(pokemonCollection);
        pokedexService = new PokedexService(mockRepository, Shared.DtoToModelMapper, Substitute.For<ILogger<PokedexService>>());
        sut = new PokemonService(mockRepository, pokedexService, Shared.DtoToModelMapper, Shared.ModelToDtoMapper, Substitute.For<ILogger<PokemonService>>());
    }

    [Test]
    public async Task GetPokemonById_Valid_ReturnsPokemon()
    {
        var expectedPokemon = pokemonCollection.Collection.First();
        var actualPokemon = await sut.GetPokemonById(expectedPokemon.PokemonId);
        Assert.That(actualPokemon, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(actualPokemon.GameId, Is.EqualTo(expectedPokemon.GameId));
            Assert.That(actualPokemon.TrainerId, Is.EqualTo(expectedPokemon.TrainerId));
            Assert.That(actualPokemon.PokemonId, Is.EqualTo(expectedPokemon.PokemonId));
            Assert.That(actualPokemon.DexNo, Is.EqualTo(expectedPokemon.DexNo));
            Assert.That(actualPokemon.Nickname, Is.EqualTo(expectedPokemon.Nickname));
        });
    }
}
