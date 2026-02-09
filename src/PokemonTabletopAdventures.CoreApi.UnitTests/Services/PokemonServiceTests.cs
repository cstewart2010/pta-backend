using Microsoft.Extensions.Logging;
using NSubstitute;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Domain;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.CoreApi.UnitTests.Implementations;
using PokemonTabletopAdventures.Models.Enums;
using PokemonTabletopAdventures.Models.Pokemons;
using System.Net;

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

    [Test]
    public void GetPokemonById_Invalid_Throws()
    {
        var pokemonId = Guid.NewGuid();
        var expectItem = pokemonCollection.Collection.First();
        var aggregateException = Assert.Throws<AggregateException>(() =>
        {
            var task = sut.GetPokemonById(pokemonId);
            task.Wait();
        });

        Assert.That(aggregateException.InnerException, Is.TypeOf<UnknownEntityException<PokemonDto>>());
        var exception = aggregateException.InnerException as UnknownEntityException<PokemonDto>;
        Assert.Multiple(() =>
        {
            Assert.That(exception!.Title, Is.EqualTo(PtaExceptionParts.UnknownEntityTitle));
            Assert.That(
                exception.Message,
                Is.EqualTo($"Could not find a {typeof(PokemonDto).Name} using {PropertyNames.PokemonId}={pokemonId}"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        });
    }

    [Test]
    public async Task GetPokemonByTrainerId_Valid_ReturnsPokemonCollection()
    {
        var trainerId = Shared.UserIds.First();
        var gameId = Shared.GameIds.First();
        var expectedList = pokemonCollection.Collection.Where(x => x.TrainerId == trainerId && x.GameId == gameId).ToList();
        ICollection<Pokemon> actualList = [.. await sut.GetPokemonByTrainerId(trainerId, gameId, false)];
        Assert.That(actualList, Has.Count.EqualTo(expectedList.Count));
        Assert.Multiple(() =>
        {
            foreach (var item in expectedList)
            {
                Assert.That(actualList.SingleOrDefault(x => x.PokemonId == item.PokemonId), Is.Not.Null);
            }
        });
    }

    [Test]
    public void GetPokemonByTrainerId_Invalid_Throws()
    {
        var trainerId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var expectItem = pokemonCollection.Collection.First();
        var aggregateException = Assert.Throws<AggregateException>(() =>
        {
            var task = sut.GetPokemonByTrainerId(trainerId, gameId, false);
            task.Wait();
        });

        Assert.That(aggregateException.InnerException, Is.TypeOf<UnknownEntityException<PokemonDto>>());
        var exception = aggregateException.InnerException as UnknownEntityException<PokemonDto>;
        Assert.Multiple(() =>
        {
            Assert.That(exception!.Title, Is.EqualTo(PtaExceptionParts.UnknownEntityTitle));
            Assert.That(
                exception.Message,
                Is.EqualTo($"Could not find a {typeof(PokemonDto).Name} using {PropertyNames.TrainerId} {PropertyNames.GameId}={(trainerId, gameId)}"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        });
    }

    [Test]
    public async Task PostPokemon_Valid_UpdatesCollection()
    {
        var count = pokemonCollection.Collection.Count;
        var trainerId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var pokemonId = Guid.NewGuid();
        await sut.PostPokemon(GetPokemon(pokemonId, trainerId, gameId, nameof(PostPokemon_Valid_UpdatesCollection)));

        Assert.That(pokemonCollection.Collection, Has.Count.EqualTo(count + 1));
    }

    [Test]
    public async Task PostPokemon_Duplicate_Throws()
    {
        var count = pokemonCollection.Collection.Count;
        var item = pokemonCollection.Collection.First();
        var model = await Shared.DtoToModelMapper.ParseFromDto(item);
        var aggregateException = Assert.Throws<AggregateException>(() =>
        {
            var task = sut.PostPokemon(model);
            task.Wait();
        });

        Assert.That(aggregateException.InnerException, Is.TypeOf<DuplicateEntryException>());
        var exception = aggregateException.InnerException as DuplicateEntryException;
        Assert.Multiple(() =>
        {
            Assert.That(exception!.Title, Is.EqualTo(PtaExceptionParts.DuplicateEntryTitle));
            Assert.That(exception.Message, Is.EqualTo($"Duplicate entry of type {typeof(PokemonDto).Name}"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(pokemonCollection.Collection, Has.Count.EqualTo(count));
        });
    }

    [Test]
    public async Task UpdatePokemon_Valid_UpdatesAllProperties()
    {
        var testPokemon = await Shared.DtoToModelMapper.ParseFromDto(pokemonCollection.Collection.Last());
        var originalName = new string(testPokemon.SpeciesName.ToCharArray());
        var updatedPokemon = GetPokemon(testPokemon.PokemonId, testPokemon.TrainerId, testPokemon.GameId, nameof(UpdatePokemon_Valid_UpdatesAllProperties));
        var actualUpdate = await sut.UpdatePokemon(updatedPokemon);
        Assert.That(actualUpdate, Is.Not.Null);
        var retrievedUpdate = await sut.GetPokemonById(testPokemon.PokemonId);
        Assert.Multiple(() =>
        {
            Assert.That(actualUpdate.PokemonId, Is.EqualTo(testPokemon.PokemonId));
            Assert.That(retrievedUpdate.PokemonId, Is.EqualTo(testPokemon.PokemonId));
            Assert.That(actualUpdate.TrainerId, Is.EqualTo(testPokemon.TrainerId));
            Assert.That(retrievedUpdate.TrainerId, Is.EqualTo(testPokemon.TrainerId));
            Assert.That(actualUpdate.GameId, Is.EqualTo(testPokemon.GameId));
            Assert.That(retrievedUpdate.GameId, Is.EqualTo(testPokemon.GameId));
            Assert.That(actualUpdate.SpeciesName, Is.Not.EqualTo(originalName));
            Assert.That(actualUpdate.SpeciesName, Is.EqualTo(retrievedUpdate.SpeciesName));
        });
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task UpdatePokemonEvolvability_Valid_UpdatesItemIncollection(bool isEvolvable)
    {
        var testPokemon = pokemonCollection.Collection.First();
        var updatedPokemon = await sut.UpdatePokemonEvolvability(testPokemon.PokemonId, isEvolvable);
        Assert.That(updatedPokemon, Is.Not.Null);
        var retrievedPokemon = await sut.GetPokemonById(testPokemon.PokemonId);
        Assert.Multiple(() =>
        {
            Assert.That(updatedPokemon.CanEvolve, Is.EqualTo(isEvolvable));
            Assert.That(retrievedPokemon.CanEvolve, Is.EqualTo(updatedPokemon.CanEvolve));
        });
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void UpdatePokemonEvolvability_NotFound_Throws(bool isEvolvable)
    {
        var pokemonId = Guid.NewGuid();
        var aggregateException = Assert.Throws<AggregateException>(() =>
        {
            var task = sut.UpdatePokemonEvolvability(pokemonId, isEvolvable);
            task.Wait();
        });

        Assert.That(aggregateException.InnerException, Is.TypeOf<UpdateException>());
        var exception = aggregateException.InnerException as UpdateException;
        Assert.Multiple(() =>
        {
            Assert.That(exception!.Title, Is.EqualTo(PtaExceptionParts.UpdateErrorTitle));
            Assert.That(exception.Message, Is.EqualTo($"Failed to update {typeof(PokemonDto).Name} {pokemonId}"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        });
    }

    [Test]
    [TestCase(10)]
    [TestCase(100)]
    public async Task UpdatePokemonHP_Valid_UpdatesItemIncollection(int hp)
    {
        var testPokemon = pokemonCollection.Collection.First();
        var updatedPokemon = await sut.UpdatePokemonHP(testPokemon.PokemonId, hp);
        Assert.That(updatedPokemon, Is.Not.Null);
        var retrievedPokemon = await sut.GetPokemonById(testPokemon.PokemonId);
        Assert.Multiple(() =>
        {
            Assert.That(updatedPokemon.CurrentHP, Is.EqualTo(hp));
            Assert.That(retrievedPokemon.CurrentHP, Is.EqualTo(updatedPokemon.CurrentHP));
        });
    }

    [Test]
    [TestCase(10)]
    [TestCase(100)]
    public void UpdatePokemonHP_NotFound_Throws(int hp)
    {
        var pokemonId = Guid.NewGuid();
        var aggregateException = Assert.Throws<AggregateException>(() =>
        {
            var task = sut.UpdatePokemonHP(pokemonId, hp);
            task.Wait();
        });

        Assert.That(aggregateException.InnerException, Is.TypeOf<UpdateException>());
        var exception = aggregateException.InnerException as UpdateException;
        Assert.Multiple(() =>
        {
            Assert.That(exception!.Title, Is.EqualTo(PtaExceptionParts.UpdateErrorTitle));
            Assert.That(exception.Message, Is.EqualTo($"Failed to update {typeof(PokemonDto).Name} {pokemonId}"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        });
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task UpdatePokemonLocation_Valid_UpdatesItemIncollection(bool isOnActiveTeam)
    {
        var testPokemon = pokemonCollection.Collection.First();
        var updatedPokemon = await sut.UpdatePokemonLocation(testPokemon.PokemonId, isOnActiveTeam);
        Assert.That(updatedPokemon, Is.Not.Null);
        var retrievedPokemon = await sut.GetPokemonById(testPokemon.PokemonId);
        Assert.Multiple(() =>
        {
            Assert.That(updatedPokemon.IsOnActiveTeam, Is.EqualTo(isOnActiveTeam));
            Assert.That(retrievedPokemon.IsOnActiveTeam, Is.EqualTo(updatedPokemon.IsOnActiveTeam));
        });
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void UpdatePokemonLocation_NotFound_Throws(bool isOnActiveTeam)
    {
        var pokemonId = Guid.NewGuid();
        var aggregateException = Assert.Throws<AggregateException>(() =>
        {
            var task = sut.UpdatePokemonLocation(pokemonId, isOnActiveTeam);
            task.Wait();
        });

        Assert.That(aggregateException.InnerException, Is.TypeOf<UpdateException>());
        var exception = aggregateException.InnerException as UpdateException;
        Assert.Multiple(() =>
        {
            Assert.That(exception!.Title, Is.EqualTo(PtaExceptionParts.UpdateErrorTitle));
            Assert.That(exception.Message, Is.EqualTo($"Failed to update {typeof(PokemonDto).Name} {pokemonId}"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        });
    }

    [Test]
    public async Task UpdatePokemonTrainerId_Valid_UpdatesItemIncollection()
    {
        var trainerId = Guid.NewGuid();
        var testPokemon = pokemonCollection.Collection.First();
        var updatedPokemon = await sut.UpdatePokemonTrainerId(testPokemon.PokemonId, trainerId);
        Assert.That(updatedPokemon, Is.Not.Null);
        var retrievedPokemon = await sut.GetPokemonById(testPokemon.PokemonId);
        Assert.Multiple(() =>
        {
            Assert.That(updatedPokemon.TrainerId, Is.EqualTo(trainerId));
            Assert.That(retrievedPokemon.TrainerId, Is.EqualTo(updatedPokemon.TrainerId));
        });
    }

    [Test]
    public void UpdatePokemonTrainerId_NotFound_Throws()
    {
        var trainerId = Guid.NewGuid();
        var pokemonId = Guid.NewGuid();
        var aggregateException = Assert.Throws<AggregateException>(() =>
        {
            var task = sut.UpdatePokemonTrainerId(pokemonId, trainerId);
            task.Wait();
        });

        Assert.That(aggregateException.InnerException, Is.TypeOf<UpdateException>());
        var exception = aggregateException.InnerException as UpdateException;
        Assert.Multiple(() =>
        {
            Assert.That(exception!.Title, Is.EqualTo(PtaExceptionParts.UpdateErrorTitle));
            Assert.That(exception.Message, Is.EqualTo($"Failed to update {typeof(PokemonDto).Name} {pokemonId}"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        });
    }

    [Test]
    public async Task DeletePokemon_Valid_UpdatesCollection()
    {
        var count = pokemonCollection.Collection.Count;
        var trainerId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var pokemonId = Guid.NewGuid();
        await sut.PostPokemon(GetPokemon(pokemonId, trainerId, gameId, nameof(DeletePokemon_Valid_UpdatesCollection)));
        await sut.DeletePokemon(pokemonId);
        Assert.That(pokemonCollection.Collection, Has.Count.EqualTo(count));
    }

    [Test]
    public void DeletePokemon_NotFound_Throws()
    {
        var pokemonId = Guid.NewGuid();
        var aggregateException = Assert.Throws<AggregateException>(() =>
        {
            var task = sut.DeletePokemon(pokemonId);
            task.Wait();
        });

        Assert.That(aggregateException.InnerException, Is.TypeOf<UnknownEntityException<PokemonDto>>());
        var exception = aggregateException.InnerException as UnknownEntityException<PokemonDto>;
        Assert.Multiple(() =>
        {
            Assert.That(exception!.Title, Is.EqualTo(PtaExceptionParts.UnknownEntityTitle));
            Assert.That(
                exception.Message,
                Is.EqualTo($"Could not find a {typeof(PokemonDto).Name} using {PropertyNames.PokemonId}={pokemonId}"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        });
    }

    [Test]
    public async Task DeletePokemonByTrainerId_Valid_UpdatesCollection()
    {
        var count = pokemonCollection.Collection.Count;
        var trainerId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        foreach (var x in Enumerable.Range(0, 5))
        {
            var pokemonId = Guid.NewGuid();
            await sut.PostPokemon(GetPokemon(pokemonId, trainerId, gameId, nameof(DeletePokemonByTrainerId_Valid_UpdatesCollection)));
        }
        await sut.DeletePokemonByTrainerId(gameId, trainerId);
        Assert.That(pokemonCollection.Collection, Has.Count.EqualTo(count));
    }

    [Test]
    [TestCaseSource(nameof(GetSearchItems))]
    public async Task DeletePokemonByTrainerId_NotFound_Throws(SearchItem item)
    {
        var count = pokemonCollection.Collection.Count;
        var testPokemon = pokemonCollection.Collection.First();
        await sut.DeletePokemonByTrainerId(item.GameId ?? testPokemon.GameId, item.TrainerId ?? testPokemon.TrainerId);
        Assert.That(pokemonCollection.Collection, Has.Count.EqualTo(count));
    }

    private static Pokemon GetPokemon(Guid pokemonId, Guid trainerId, Guid gameId, string name)
    {
        return new Pokemon
        {
            AlternateForms = [],
            IsOnActiveTeam = true,
            CanEvolve = true,
            CurrentHP = 0,
            DexNo = 4,
            Form = "",
            Diet = "",
            EggGroups = [],
            EggHatchRate = "",
            EvolvedFrom = "",
            GameId = gameId,
            Gender = Gender.Male,
            GMaxMove = "",
            Habitats = [],
            IsShiny = false,
            LegendaryStats = null,
            Moves = [],
            Nature = Nature.Lonely,
            Nickname = pokemonId.ToString(),
            NormalPortrait = "",
            OriginalTrainerId = trainerId,
            Passives = [],
            Pokeball = "",
            PokemonId = pokemonId,
            PokemonStats = new(),
            PokemonStatus = Status.Normal,
            Proficiencies = [],
            Rarity = "",
            ShinyPortrait = "",
            Size = Size.Small,
            Skills = [],
            SpeciesName = name,
            TrainerId = trainerId,
            Type = "",
            Weight = Weight.Medium,
        };
    }

    private static SearchItem[] GetSearchItems()
    {
        return [
            new SearchItem{ TrainerId = Guid.NewGuid()},
            new SearchItem{ GameId = Guid.NewGuid()}
        ];
    }
}
