using Microsoft.Extensions.Logging;
using NSubstitute;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Domain;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.CoreApi.UnitTests.Implementations;
using PokemonTabletopAdventures.Models.Users;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Services;

public class UserMessageThreadServiceTests
{
    private UserMessageThreadService _sut;
    private UserMessageThreadCollectionImpl _userMessageThreadCollection;

    [OneTimeSetUp]
    public void Setup()
    {
        var mockRepository = Substitute.For<IRepositoryService>();
        var mockLogger = Substitute.For<ILogger<UserMessageThreadService>>();
        var trainerCollection = new TrainerCollectionImpl();
        var gameCollection = new GameCollectionImpl();
        var userCollection = new UserCollectionImpl();
        var pokemonCollection = new PokemonCollectionImpl();
        var pokedexCollection = new PokedexCollectionImpl();
        var npcCollection = new NpcCollectionImpl();
        var shopCollection = new ShopCollectionImpl();
        var settingCollection = new SettingCollectionImpl(trainerCollection,  pokemonCollection, npcCollection, shopCollection);
        _userMessageThreadCollection = new UserMessageThreadCollectionImpl();
        mockRepository.GetCollection<ShopDto>(MongoCollection.Shops).Returns(shopCollection);
        mockRepository.GetCollection<SettingDto>(MongoCollection.Settings).Returns(settingCollection);
        mockRepository.GetCollection<GameDto>(MongoCollection.Games).Returns(gameCollection);
        mockRepository.GetCollection<UserDto>(MongoCollection.Users).Returns(userCollection);
        mockRepository.GetCollection<PokeDexItemDto>(MongoCollection.PokeDex).Returns(pokedexCollection);
        mockRepository.GetCollection<PokemonDto>(MongoCollection.Pokemon).Returns(pokemonCollection);
        mockRepository.GetCollection<TrainerDto>(MongoCollection.Trainers).Returns(trainerCollection);
        mockRepository.GetCollection<NpcDto>(MongoCollection.NPCs).Returns(npcCollection);
        mockRepository.GetCollection<UserMessageThreadDto>(MongoCollection.UserMessageThreads).Returns(_userMessageThreadCollection);
        var pokedexService = new PokedexService(mockRepository, Shared.DtoToModelMapper, Substitute.For<ILogger<PokedexService>>());
        var pokemonService = new PokemonService(mockRepository, pokedexService, Shared.DtoToModelMapper, Shared.ModelToDtoMapper, Substitute.For<ILogger<PokemonService>>());
        var shopService = new ShopService(mockRepository, Shared.DtoToModelMapper, Shared.ModelToDtoMapper, Substitute.For<ILogger<ShopService>>());
        var settingService = new SettingService(mockRepository, shopService, Shared.DtoToModelMapper, Shared.ModelToDtoMapper, Substitute.For<ILogger<SettingService>>());
        var trainerService = new TrainerService(mockRepository, pokemonService, pokedexService, settingService, Shared.DtoToModelMapper, Shared.ModelToDtoMapper, Substitute.For<ILogger<TrainerService>>());
        var userService = new UserService(mockRepository, trainerService, Shared.DtoToModelMapper, Shared.ModelToDtoMapper, Substitute.For<ILogger<UserService>>());
        _sut = new UserMessageThreadService(mockRepository, userService, Shared.DtoToModelMapper, Shared.ModelToDtoMapper, mockLogger);
    }

    [Test]
    public async Task PostThread_Valid_ReturnsThread()
    {
        var thread = new UserMessageThread
        {
            MessageId = Guid.NewGuid(),
            Messages = Shared.UserIds.Select(x => new UserMessage
            {
                Message = x.ToString(),
                Timestamp = DateTime.Now,
                User = x
            }).ToList()
        };
        
        await _sut.PostThread(thread);
        
        var actual = await _sut.GetMessageById(thread.MessageId);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(actual.MessageId, Is.EqualTo(thread.MessageId));
            Assert.That(actual.Messages, Has.Count.EqualTo(thread.Messages.Count));
        }
    }

    [Test]
    public async Task UpdateThread_Valid_UpdatesThread()
    {
        var thread = new UserMessageThread
        {
            MessageId = Guid.NewGuid(),
            Messages = Shared.UserIds.Select(x => new UserMessage
            {
                Message = x.ToString(),
                Timestamp = DateTime.Now,
                User = x
            }).ToList()
        };
        
        await _sut.PostThread(thread);
        var updatedThread = new UserMessageThread
        {
            MessageId = thread.MessageId,
            Messages = []
        };
        var actual = await _sut.UpdateThread(updatedThread);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(actual.MessageId, Is.EqualTo(thread.MessageId));
            Assert.That(actual.Messages, Is.Empty);
        }
    }

    [Test]
    public async Task DeleteThread_Valid_DeletesThread()
    {
        var thread = new UserMessageThread
        {
            MessageId = Guid.NewGuid(),
            Messages = Shared.UserIds.Append(Guid.NewGuid()).Select(x => new UserMessage
            {
                Message = x.ToString(),
                Timestamp = DateTime.Now,
                User = x
            }).ToList()
        };
        
        await _sut.PostThread(thread);
        
        await _sut.DeleteThread(thread.MessageId);
        Assert.That(_userMessageThreadCollection.Collection.Where(x => x.MessageId == thread.MessageId), Is.Empty);
    }
}