using Microsoft.Extensions.Logging;
using NSubstitute;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Domain;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.CoreApi.UnitTests.Implementations;
using PokemonTabletopAdventures.Models.Enums;
using PokemonTabletopAdventures.Models.Npcs;
using System.Net;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Services;

internal class NpcServiceTests
{
    private NpcService sut;
    private NpcCollectionImpl npcCollection;
    private PokedexService pokedexService;
    private PokedexCollectionImpl pokedexCollection;
    private PokemonService pokemonService;
    private PokemonCollectionImpl pokemonCollection;
    private GameCollectionImpl gameCollection;

    [OneTimeSetUp]
    public void SetUp()
    {
        var mockRepository = Substitute.For<IRepositoryService>();
        pokedexCollection = new PokedexCollectionImpl();
        pokemonCollection = new PokemonCollectionImpl();
        gameCollection = new GameCollectionImpl();
        mockRepository.GetCollection<PokeDexItemDto>(MongoCollection.Pokedex).Returns(pokedexCollection);
        mockRepository.GetCollection<PokemonDto>(MongoCollection.Pokemon).Returns(pokemonCollection);
        pokedexService = new PokedexService(mockRepository, Shared.DtoToModelMapper, Substitute.For<ILogger<PokedexService>>());
        pokemonService = new PokemonService(mockRepository, pokedexService, Shared.DtoToModelMapper, Shared.ModelToDtoMapper, Substitute.For<ILogger<PokemonService>>());
        npcCollection = new NpcCollectionImpl();
        mockRepository.GetCollection<NpcDto>(MongoCollection.NPCs).Returns(npcCollection);
        sut = new NpcService(mockRepository, pokemonService, Shared.DtoToModelMapper, Shared.ModelToDtoMapper, Substitute.For<ILogger<NpcService>>());
    }

    [Test]
    public async Task GetNpc_Valid_ReturnsItem()
    {
        var id = Shared.NpcIds.First();
        var expectedNpc = npcCollection.Collection.First(x => x.NPCId == id);

        var actualNpc = await sut.GetNpc(id);

        Assert.That(actualNpc, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(actualNpc.NpcId, Is.EqualTo(id));
            Assert.That(actualNpc.GameId, Is.EqualTo(expectedNpc.GameId));
            Assert.That(actualNpc.Age, Is.EqualTo(expectedNpc.Age));
            Assert.That(actualNpc.TrainerName, Is.EqualTo(expectedNpc.TrainerName));
        });
    }

    [Test]
    [TestCaseSource(nameof(GetSearchItem))]
    public void GetNpc_Invalid_Throws(SearchItem item)
    {
        var aggregateException = Assert.Throws<AggregateException>(() =>
        {
            var task = sut.GetNpc(item.NpcId!.Value);
            task.Wait();
        });

        Assert.That(aggregateException.InnerException, Is.TypeOf<UnknownEntityException<NpcDto>>());
        var exception = aggregateException.InnerException as UnknownEntityException<NpcDto>;
        Assert.Multiple(() =>
        {
            Assert.That(exception!.Title, Is.EqualTo(PtaExceptionParts.UnknownEntityTitle));
            Assert.That(
                exception.Message,
                Is.EqualTo($"Could not find a {typeof(NpcDto).Name} using {PropertyNames.NpcId}={item.NpcId}"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        });
    }

    [Test]
    public async Task GetNpcs_AllValid_ReturnsCollection()
    {
        var ids = Shared.NpcIds;

        var npcs = await sut.GetNpcs(ids);

        Assert.That(npcs, Is.Not.Null.Or.Empty);
        Assert.That(npcs, Has.Count.EqualTo(npcCollection.Collection.Count));
        Assert.Multiple(() =>
        {
            foreach (var npc in npcCollection.Collection)
            {
                Assert.That(npcs.SingleOrDefault(x => x.NpcId == npc.NPCId), Is.Not.Null);
            }
        });
    }

    [Test]
    public async Task GetNpcs_SomeValid_ReturnsCollection()
    {
        Guid[] ids = [Shared.NpcIds.First(), Guid.NewGuid()];

        var npcs = await sut.GetNpcs(ids);

        Assert.That(npcs, Is.Not.Null.Or.Empty);
        Assert.That(npcs, Has.Count.EqualTo(1));
    }

    [Test]
    public void GetNpcs_NoneValid_Throws()
    {
        Guid[] ids = [Guid.NewGuid()];

        var aggregateException = Assert.Throws<AggregateException>(() =>
        {
            var task = sut.GetNpcs(ids);
            task.Wait();
        });

        Assert.That(aggregateException.InnerException, Is.TypeOf<UnknownEntityException<NpcDto>>());
        var exception = aggregateException.InnerException as UnknownEntityException<NpcDto>;
        Assert.Multiple(() =>
        {
            Assert.That(exception!.Title, Is.EqualTo(PtaExceptionParts.UnknownEntityTitle));
            Assert.That(
                exception.Message,
                Contains.Substring($"Could not find a {typeof(NpcDto).Name} using {PropertyNames.NpcId}"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        });
    }

    [Test]
    public async Task GetNpcsByGameId_Valid_ReturnsCollection()
    {
        var gameId = Shared.GameIds.First();
        var expectedNpcs = npcCollection.Collection.Where(x => x.GameId == gameId).ToArray();

        var actualNpcs = await sut.GetNpcsByGameId(gameId);

        Assert.That(actualNpcs, Is.Not.Null.Or.Empty);
        Assert.That(actualNpcs, Has.Count.EqualTo(expectedNpcs.Length));
        Assert.Multiple(() =>
        {
            foreach (var npc in expectedNpcs)
            {
                Assert.That(actualNpcs.SingleOrDefault(x => x.NpcId == npc.NPCId), Is.Not.Null);
            }
        });
    }

    [Test]
    public void GetNpcsByGameId_Invalid_Throws()
    {
        var gameId = Guid.NewGuid();

        var aggregateException = Assert.Throws<AggregateException>(() =>
        {
            var task = sut.GetNpcsByGameId(gameId);
            task.Wait();
        });

        Assert.That(aggregateException.InnerException, Is.TypeOf<UnknownEntityException<NpcDto>>());
        var exception = aggregateException.InnerException as UnknownEntityException<NpcDto>;
        Assert.Multiple(() =>
        {
            Assert.That(exception!.Title, Is.EqualTo(PtaExceptionParts.UnknownEntityTitle));
            Assert.That(
                exception.Message,
                Is.EqualTo($"Could not find a {typeof(NpcDto).Name} using {PropertyNames.GameId}={gameId}"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        });
    }

    [Test]
    public async Task PostNpc_Valid_AddsToCollection()
    {
        var initialCount = npcCollection.Collection.Count;
        var npc = new Npc
        {
            Background = string.Empty,
            Species = string.Empty,
            Description = string.Empty,
            Feats = [],
            GameId = Guid.NewGuid(),
            Gender = Gender.Genderless,
            Goals = string.Empty,
            Height = 3,
            Level = 5,
            NpcId = Guid.NewGuid(),
            Personality = string.Empty,
            PokemonTeam = [],
            Sprite = string.Empty,
            TrainerClasses = [],
            TrainerSkills = [],
            TrainerStats = new(),
            TrainerName = string.Empty,
            Weight = 7
        };

        await sut.PostNpc(npc);

        Assert.That(npcCollection.Collection, Has.Count.EqualTo(initialCount + 1));
        var newNpc = await sut.GetNpc(npc.NpcId);
        Assert.Multiple(() =>
        {
            Assert.That(newNpc.Background, Is.EqualTo(npc.Background));
            Assert.That(newNpc.GameId, Is.EqualTo(npc.GameId));
            Assert.That(newNpc.Height, Is.EqualTo(npc.Height));
        });
    }

    [Test]
    public void PostNpc_DuplicateItem_Items()
    {
        var initialCount = npcCollection.Collection.Count;
        var dto = npcCollection.Collection.First();
        var npc = new Npc
        {
            Background = string.Empty,
            Species = string.Empty,
            Description = string.Empty,
            Feats = [],
            GameId = dto.GameId,
            Gender = Gender.Genderless,
            Goals = string.Empty,
            Height = 3,
            Level = 5,
            NpcId = dto.NPCId,
            Personality = string.Empty,
            PokemonTeam = [],
            Sprite = string.Empty,
            TrainerClasses = [],
            TrainerSkills = [],
            TrainerStats = new(),
            TrainerName = string.Empty,
            Weight = 7
        };

        var aggregateException = Assert.Throws<AggregateException>(() =>
        {
            var task = sut.PostNpc(npc);
            task.Wait();
        });

        Assert.That(aggregateException.InnerException, Is.TypeOf<DuplicateEntryException>());
        var exception = aggregateException.InnerException as DuplicateEntryException;
        Assert.Multiple(() =>
        {
            Assert.That(exception!.Title, Is.EqualTo(PtaExceptionParts.DuplicateEntryTitle));
            Assert.That(exception.Message, Is.EqualTo($"Duplicate entry of type {typeof(NpcDto).Name}"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(npcCollection.Collection, Has.Count.EqualTo(initialCount));
        });
    }

    [Test]
    public async Task UpdateNpc_UpdatesAllProperties()
    {
        await PostNpc_Valid_AddsToCollection();
        var count = npcCollection.Collection.Count;
        var dto = npcCollection.Collection.Last();
        var npc = await sut.GetNpc(dto.NPCId);
        var oldGameId = npc.GameId;
        var oldAge = npc.Age;
        var oldName = npc.TrainerName;
        npc.GameId = Guid.NewGuid();
        npc.Age = Random.Shared.Next(10, 100);
        npc.TrainerName = nameof(UpdateNpc_UpdatesAllProperties);

        var updatedNpc = await sut.UpdateNpc(npc);

        Assert.Multiple(() =>
        {
            Assert.That(updatedNpc, Is.Not.Null);
            Assert.That(npcCollection.Collection, Has.Count.EqualTo(count));
        });
        Assert.Multiple(() =>
        {
            Assert.That(updatedNpc.GameId, Is.Not.EqualTo(oldGameId));
            Assert.That(updatedNpc.Age, Is.Not.EqualTo(oldAge));
            Assert.That(updatedNpc.TrainerName, Is.Not.EqualTo(oldName));
            Assert.That(updatedNpc.GameId, Is.EqualTo(npc.GameId));
            Assert.That(updatedNpc.Age, Is.EqualTo(npc.Age));
            Assert.That(updatedNpc.TrainerName, Is.EqualTo(npc.TrainerName));
        });
    }

    [Test]
    public async Task DeleteNpc_Valid_UpdatesCollection()
    {
        var count = npcCollection.Collection.Count;
        await PostNpc_Valid_AddsToCollection();
        var dto = npcCollection.Collection.Last();

        await sut.DeleteNpc(dto.NPCId);

        Assert.That(npcCollection.Collection, Has.Count.EqualTo(count));
        GetNpc_Invalid_Throws(new SearchItem
        {
            NpcId = dto.NPCId
        });
    }

    [Test]
    public void DeleteNpc_NotFound_Throws()
    {
        var id = Guid.NewGuid();
        var aggregateException = Assert.Throws<AggregateException>(() =>
        {
            var task = sut.DeleteNpc(id);
            task.Wait();
        });

        Assert.That(aggregateException.InnerException, Is.TypeOf<UnknownEntityException<NpcDto>>());
        var exception = aggregateException.InnerException as UnknownEntityException<NpcDto>;
        Assert.Multiple(() =>
        {
            Assert.That(exception!.Title, Is.EqualTo(PtaExceptionParts.UnknownEntityTitle));
            Assert.That(
                exception.Message,
                Is.EqualTo($"Could not find a {typeof(NpcDto).Name} using {PropertyNames.NpcId}={id}"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        });
    }

    private static SearchItem[] GetSearchItem()
    {
        return [
            new SearchItem{
                NpcId = Guid.NewGuid(),
            }
        ];
    }
}
