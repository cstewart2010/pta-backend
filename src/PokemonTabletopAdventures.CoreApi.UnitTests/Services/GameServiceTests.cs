using Microsoft.Extensions.Logging;
using NSubstitute;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Domain;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.CoreApi.UnitTests.Implementations;
using PokemonTabletopAdventures.Models;
using PokemonTabletopAdventures.Models.Enums;
using PokemonTabletopAdventures.Models.Games;
using PokemonTabletopAdventures.Models.Npcs;
using PokemonTabletopAdventures.Models.Users;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Services;

public class GameServiceTests
{
    private GameService _sut;
    private TrainerCollectionImpl _trainerCollection;
    private PokemonCollectionImpl _pokemonCollection;
    private NpcCollectionImpl _npcCollection;
    private ShopCollectionImpl _shopCollection;
    private SettingCollectionImpl _settingCollection;
    private GameCollectionImpl _gameCollection;
    private UserCollectionImpl _userCollection;
    private PokedexCollectionImpl _pokedexCollection;
    private NpcService _npcService;

    [OneTimeSetUp]
    public void Setup()
    {
        var mockRepository = Substitute.For<IRepositoryService>();
        var mockLogger = Substitute.For<ILogger<GameService>>();
        _trainerCollection = new TrainerCollectionImpl();
        _pokemonCollection = new PokemonCollectionImpl();
        _gameCollection = new GameCollectionImpl();
        _npcCollection = new NpcCollectionImpl();
        _shopCollection = new ShopCollectionImpl();
        _userCollection = new UserCollectionImpl();
        _pokedexCollection = new PokedexCollectionImpl();
        _settingCollection = new SettingCollectionImpl(_trainerCollection, _pokemonCollection, _npcCollection, _shopCollection);
        foreach (var game in _gameCollection.Collection)
        {
            game.NPCs = [.._npcCollection.Collection.Where(x => x.GameId == game.GameId).Select(x => x.NPCId)];
        }
        mockRepository.GetCollection<ShopDto>(MongoCollection.Shops).Returns(_shopCollection);
        mockRepository.GetCollection<SettingDto>(MongoCollection.Settings).Returns(_settingCollection);
        mockRepository.GetCollection<GameDto>(MongoCollection.Games).Returns(_gameCollection);
        mockRepository.GetCollection<UserDto>(MongoCollection.Users).Returns(_userCollection);
        mockRepository.GetCollection<PokeDexItemDto>(MongoCollection.Pokedex).Returns(_pokedexCollection);
        mockRepository.GetCollection<PokemonDto>(MongoCollection.Pokemon).Returns(_pokemonCollection);
        mockRepository.GetCollection<TrainerDto>(MongoCollection.Trainers).Returns(_trainerCollection);
        mockRepository.GetCollection<NpcDto>(MongoCollection.NPCs).Returns(_npcCollection);
        var shopService = new ShopService(mockRepository, Shared.DtoToModelMapper, Shared.ModelToDtoMapper, Substitute.For<ILogger<ShopService>>());
        var pokedexService = new PokedexService(mockRepository, Shared.DtoToModelMapper, Substitute.For<ILogger<PokedexService>>());
        var pokemonService = new PokemonService(mockRepository, pokedexService, Shared.DtoToModelMapper, Shared.ModelToDtoMapper, Substitute.For<ILogger<PokemonService>>());
        var settingService = new SettingService(mockRepository, shopService, Shared.DtoToModelMapper, Shared.ModelToDtoMapper, Substitute.For<ILogger<SettingService>>());
        var trainerService = new TrainerService(mockRepository, pokemonService, pokedexService, settingService, Shared.DtoToModelMapper, Shared.ModelToDtoMapper, Substitute.For<ILogger<TrainerService>>());
        _npcService = new NpcService(mockRepository, pokemonService, Shared.DtoToModelMapper, Shared.ModelToDtoMapper, Substitute.For<ILogger<NpcService>>());
        _sut = new GameService(mockRepository, trainerService, _npcService, settingService, shopService,
            Shared.DtoToModelMapper, Shared.ModelToDtoMapper, mockLogger);
    }

    [Test]
    public async Task GetAllGames_Nickname_ReturnsGames()
    {
        var games = await _sut.GetAllGames(string.Empty);
        Assert.That(games, Is.Not.Null.And.Count.EqualTo(_gameCollection.Collection.Count));
    }

    [Test]
    public async Task GetAllGamesWithUser_Valid_ReturnsGames()
    {
        var trainerDto = _trainerCollection.Collection.First(x => x.GameId == Shared.GameIds.First());
        var userDto = _userCollection.Collection.First(x => x.UserId == trainerDto.TrainerId);
        var user = new User
        {
            Games = userDto.Games,
            Username = userDto.Username,
            Messages = [],
            SiteRole = userDto.SiteRole,
            UserId = userDto.UserId,
            DateCreated = userDto.DateCreated,
            ActivityToken = userDto.ActivityToken,
        };

        var games = await _sut.GetAllGamesWithUser(user);
        Assert.That(games, Is.Not.Null.Or.Empty);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task GetGame_Valid_ReturnsGame(bool isGM)
    {
        var gameId = Shared.GameIds.First();
        
        var game = await _sut.GetGame(gameId, isGM);
        
        Assert.That(game, Is.Not.Null);
        Assert.That(game.Npcs.Count != 0, Is.EqualTo(isGM));
    }

    [Test]
    public async Task GetGameNickname_Valid_ReturnsNickname()
    {
        var expected = _gameCollection.Collection.First();
        
        var actual = await _sut.GetGameNickname(expected.GameId);
        
        Assert.That(actual, Is.EqualTo(expected.Nickname));
    }

    [Test]
    public async Task GetMostRecent20Games_Valid_ReturnsGames()
    {
        var trainerDto = _trainerCollection.Collection.First(x => x.GameId == Shared.GameIds.First());
        var userDto = _userCollection.Collection.First(x => x.UserId == trainerDto.TrainerId);
        foreach (var gameId in Enumerable.Range(1, 30).Select(_ => Guid.NewGuid()))
        {
            _gameCollection.Collection.Add(new GameDto
            {
                GameId = gameId,
                Logs = [],
                Nickname = gameId.ToString(),
                NPCs = []
            });
            userDto.Games.Add(gameId);
        }
        var user = new User
        {
            Games = userDto.Games,
            Username = userDto.Username,
            Messages = [],
            SiteRole = userDto.SiteRole,
            UserId = userDto.UserId,
            DateCreated = userDto.DateCreated,
            ActivityToken = userDto.ActivityToken
        };

        var games = await _sut.GetMostRecent20Games(user);
        Assert.That(games, Is.Not.Null.Or.Empty.And.Count.EqualTo(20));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task HasGM_Test(bool expected)
    {
        var gameId = expected ? Shared.GameIds.First() : Guid.NewGuid();
        
        var actual = await _sut.HasGM(gameId);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public async Task PostGame_Valid_AddsGame()
    {
        var gameId = Guid.NewGuid();
        var game = new Game
        {
            GameId = gameId,
            Nickname = gameId.ToString(),
            IsOnline = false,
            Trainers = [],
            Npcs = [],
            Settings = [],
            Logs = [],
        };
        
        var passwordHash = Guid.NewGuid().ToString();
        
        await _sut.PostGame(game, passwordHash);
        var actual = _gameCollection.Collection.First(x => x.GameId == gameId);
        Assert.That(actual.PasswordHash, Is.EqualTo(passwordHash));
    }

    [Test]
    public async Task UpdateGameLogs_Appends()
    {
        var game = await _sut.GetGame(Shared.GameIds.First(), false);
        var log = new Log
        {
            Action = "test value",
            LogTimestamp = DateTimeOffset.Now,
            User = "test name"
        };

        await _sut.UpdateGameLogs(game, true, log);
        
        var actual = _gameCollection.Collection.First(x => x.GameId == game.GameId).Logs.SingleOrDefault(x => x.Action == log.Action && x.User == log.User && x.LogTimestamp == log.LogTimestamp);
        Assert.That(actual, Is.Not.Null);
    }

    [Test]
    public async Task UpdateGameNpcList_Updates()
    {
        var gameId = Shared.GameIds.First();
        var npcId = Guid.NewGuid();
        var npc = new Npc
        {
            NpcId = npcId,
            GameId = gameId,
            TrainerName = npcId.ToString(),
            Feats = [],
            TrainerClasses = [],
            TrainerStats = new Stats(),
            PokemonTeam = [],
            Level = 10,
            TrainerSkills = [],
            Gender = Gender.Genderless,
            Height = 10,
            Weight = 10,
            Description = string.Empty,
            Personality = string.Empty,
            Background = string.Empty,
            Goals = string.Empty,
            Species = string.Empty,
            Sprite = npcId.ToString(),
        };

        await _npcService.PostNpc(npc);
        var game = await _sut.GetGame(gameId, true);
        var npcList = game.Npcs.Select(x => x.NpcId).ToList();
        npcList.Add(npcId);
        
        await _sut.UpdateGameNpcList(gameId, npcList);
        
        var actual = _gameCollection.Collection.First(x => x.GameId == gameId);
        Assert.That(actual.NPCs, Is.EquivalentTo(npcList));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task UpdateGameOnlineStatus_Valid_Updates(bool isOnline)
    {
        var gameId = Shared.GameIds.First();
        await _sut.UpdateGameOnlineStatus(gameId, isOnline);
        var actual = _gameCollection.Collection.First(x => x.GameId == gameId);
        Assert.That(actual.IsOnline, Is.EqualTo(isOnline));
    }
    
    [Test]
    public async Task DeleteGame_Valid_RemovesGame()
    {
        var gameId = Guid.NewGuid();
        var game = new Game
        {
            GameId = gameId,
            Nickname = gameId.ToString(),
            IsOnline = false,
            Trainers = [],
            Npcs = [],
            Settings = [],
            Logs = [],
        };
        
        var passwordHash = Guid.NewGuid().ToString();
        
        await _sut.PostGame(game, passwordHash);
        await  _sut.DeleteGame(gameId);
        var actual = _gameCollection.Collection.SingleOrDefault(x => x.GameId == gameId);
        Assert.That(actual, Is.Null);
    }
}