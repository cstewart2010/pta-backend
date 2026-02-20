using Microsoft.Extensions.Logging;
using NSubstitute;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Domain;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.CoreApi.UnitTests.Implementations;

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
        _settingCollection = new SettingCollectionImpl(_trainerCollection,  _pokemonCollection, _npcCollection, _shopCollection);
        mockRepository.GetCollection<ShopDto>(MongoCollection.Shops).Returns(_shopCollection);
        mockRepository.GetCollection<SettingDto>(MongoCollection.Settings).Returns(_settingCollection);
        mockRepository.GetCollection<GameDto>(MongoCollection.Games).Returns(_gameCollection);
        mockRepository.GetCollection<UserDto>(MongoCollection.Users).Returns(_userCollection);
        mockRepository.GetCollection<PokeDexItemDto>(MongoCollection.Pokedex).Returns(_pokedexCollection);
        mockRepository.GetCollection<PokemonDto>(MongoCollection.Pokemon).Returns(_pokemonCollection);
        mockRepository.GetCollection<TrainerDto>(MongoCollection.Trainers).Returns(_trainerCollection);
        mockRepository.GetCollection<NpcDto>(MongoCollection.NPCs).Returns(_npcCollection);
        mockRepository.GetCollection<NpcDto>(MongoCollection.NPCs).Returns(_npcCollection);
        var shopService = new ShopService(mockRepository, Shared.DtoToModelMapper, Shared.ModelToDtoMapper, Substitute.For<ILogger<ShopService>>());
        var pokedexService = new PokedexService(mockRepository, Shared.DtoToModelMapper, Substitute.For<ILogger<PokedexService>>());
        var pokemonService = new PokemonService(mockRepository, pokedexService, Shared.DtoToModelMapper, Shared.ModelToDtoMapper, Substitute.For<ILogger<PokemonService>>());
        var settingService = new SettingService(mockRepository, shopService, Shared.DtoToModelMapper, Shared.ModelToDtoMapper, Substitute.For<ILogger<SettingService>>());
        var trainerService = new TrainerService(mockRepository, pokemonService, pokedexService, settingService, Shared.DtoToModelMapper, Shared.ModelToDtoMapper, Substitute.For<ILogger<TrainerService>>());
        var npcService = new NpcService(mockRepository, pokemonService, Shared.DtoToModelMapper, Shared.ModelToDtoMapper, Substitute.For<ILogger<NpcService>>());
        _sut = new GameService(mockRepository, trainerService, npcService, settingService, shopService,
            Shared.DtoToModelMapper, Shared.ModelToDtoMapper, mockLogger);
    }

    [Test]
    public async Task GetAllGames_Nickname_ReturnsGames()
    {
        var games = await _sut.GetAllGames(string.Empty);
        Assert.That(games, Is.Not.Null.And.Count.EqualTo(_gameCollection.Collection.Count));
    }
}