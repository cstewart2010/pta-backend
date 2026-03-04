using Microsoft.Extensions.Logging;
using NSubstitute;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Domain;
using PokemonTabletopAdventures.CoreApi.Domain.Mappers;
using PokemonTabletopAdventures.CoreApi.DTOs;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.CoreApi.UnitTests.Implementations;
using PokemonTabletopAdventures.Models.Enums;
using PokemonTabletopAdventures.Models.Interfaces;

namespace PokemonTabletopAdventures.CoreApi.UnitTests;

public abstract class BasePtaControllerTests
{
    private readonly IRepositoryService _mockRepository;
    
    protected BasePtaControllerTests()
    {
        _mockRepository = Substitute.For<IRepositoryService>();
        var basePokemonCollection = new BasePokemonCollectionImpl();
        _mockRepository.GetCollection<BasePokemonDto>(MongoCollection.BasePokemon).Returns(basePokemonCollection);
        AddCollection<BerryDto>(DexType.Berries);
        AddCollection<FeatureDto>(DexType.Features);
        AddCollection<BaseItemDto>(DexType.KeyItems);
        AddCollection<MoveDto>(DexType.Moves);
        AddCollection<OriginDto>(DexType.Origins);
        AddCollection<BaseItemDto>(DexType.Pokeballs);
        AddCollection<BaseItemDto>(DexType.PokemonItems);
        AddCollection<TrainerClassDto>(DexType.TrainerClasses);
        AddCollection<BaseItemDto>(DexType.TrainerEquipment);
        AddCollection<BaseItemDto>(DexType.MedicalItems);
        var gameCollection = new GameCollectionImpl();
        var userCollection = new UserCollectionImpl();
        var trainerCollection = new TrainerCollectionImpl();
        var pokemonCollection = new PokemonCollectionImpl();
        var npcCollection = new NpcCollectionImpl();
        var shopCollection = new ShopCollectionImpl();
        var pokedexCollection = new PokedexCollectionImpl();
        var settingCollection = new SettingCollectionImpl(trainerCollection, pokemonCollection, npcCollection, shopCollection);
        foreach (var game in gameCollection.Collection)
        {
            game.NPCs = [..npcCollection.Collection.Where(x => x.GameId == game.GameId).Select(x => x.NPCId)];
        }

        var userMessageThreadCollection = new UserMessageThreadCollectionImpl();
        foreach (var user in userCollection.Collection)
        {
            var messageId = Guid.NewGuid();
            var thread = new UserMessageThreadDto
            {
                MessageId = messageId,
                Messages = new List<UserMessageDto>
                {
                    new UserMessageDto
                    {
                        Message = messageId.ToString(),
                        Timestamp = DateTimeOffset.Now,
                        User = user.UserId
                    }
                }
            };
            userMessageThreadCollection.Collection.Add(thread);
            user.Messages = [messageId];
        }
        var spriteCollection = new SpriteCollectionImpl();
        _mockRepository.GetCollection<GameDto>(MongoCollection.Games).Returns(gameCollection);
        _mockRepository.GetCollection<UserDto>(MongoCollection.Users).Returns(userCollection);
        _mockRepository.GetCollection<ShopDto>(MongoCollection.Shops).Returns(shopCollection);
        _mockRepository.GetCollection<SettingDto>(MongoCollection.Settings).Returns(settingCollection);
        _mockRepository.GetCollection<PokeDexItemDto>(MongoCollection.PokeDex).Returns(pokedexCollection);
        _mockRepository.GetCollection<PokemonDto>(MongoCollection.Pokemon).Returns(pokemonCollection);
        _mockRepository.GetCollection<TrainerDto>(MongoCollection.Trainers).Returns(trainerCollection);
        _mockRepository.GetCollection<NpcDto>(MongoCollection.NPCs).Returns(npcCollection);
        _mockRepository.GetCollection<UserMessageThreadDto>(MongoCollection.UserMessageThreads).Returns(userMessageThreadCollection);
        _mockRepository.GetCollection<SpriteDto>(MongoCollection.Sprites).Returns(spriteCollection);
        DexService = new DexService(_mockRepository, Shared.DtoToModelMapper, Shared.ModelToDtoMapper, Substitute.For<ILogger<DexService>>());
        EncryptionService = Substitute.For<IEncryptionService>();
        EncryptionService.GenerateToken(Arg.Any<DateTime>()).Returns(string.Empty);
        EncryptionService.HashSecret(Arg.Any<string>()).Returns(string.Empty);
        ShopService = new ShopService(_mockRepository, Shared.DtoToModelMapper, Shared.ModelToDtoMapper, Substitute.For<ILogger<ShopService>>());
        PokedexService = new PokedexService(_mockRepository, Shared.DtoToModelMapper, Substitute.For<ILogger<PokedexService>>());
        PokemonService = new PokemonService(_mockRepository, PokedexService, Shared.DtoToModelMapper, Shared.ModelToDtoMapper, Substitute.For<ILogger<PokemonService>>());
        SettingService = new SettingService(_mockRepository, ShopService, Shared.DtoToModelMapper, Shared.ModelToDtoMapper, Substitute.For<ILogger<SettingService>>());
        TrainerService = new TrainerService(_mockRepository, PokemonService, PokedexService, SettingService, Shared.DtoToModelMapper, Shared.ModelToDtoMapper, Substitute.For<ILogger<TrainerService>>());
        NpcService = new NpcService(_mockRepository, PokemonService, Shared.DtoToModelMapper, Shared.ModelToDtoMapper, Substitute.For<ILogger<NpcService>>());
        GameService = new GameService(_mockRepository, TrainerService, NpcService, SettingService, ShopService, Shared.DtoToModelMapper, Shared.ModelToDtoMapper, Substitute.For<ILogger<GameService>>());
        UserService = new UserService(_mockRepository, TrainerService, Shared.DtoToModelMapper, Shared.ModelToDtoMapper, Substitute.For<ILogger<UserService>>());
        UserMessageThreadService = new UserMessageThreadService(_mockRepository, UserService, DtoToModelMapper, ModelToDtoMapper, Substitute.For<ILogger<UserMessageThreadService>>());
        SpriteService = new SpriteService(_mockRepository, Substitute.For<ILogger<SpriteService>>());
    }
    
    protected SpriteService SpriteService { get; }

    protected UserMessageThreadService UserMessageThreadService { get; }

    protected UserService UserService { get; }

    protected GameService GameService { get; }

    protected NpcService NpcService { get; }

    protected TrainerService TrainerService { get; }

    protected SettingService SettingService { get; }

    protected PokemonService PokemonService { get; }

    protected PokedexService PokedexService { get; }

    protected ShopService ShopService { get; }
    
    protected DexService DexService { get; }
    
    protected IEncryptionService EncryptionService { get; }
    
    protected DtoToModelMapper DtoToModelMapper => Shared.DtoToModelMapper;
    
    protected ModelToDtoMapper ModelToDtoMapper => Shared.ModelToDtoMapper;

    private void AddCollection<TIndex>(DexType dexType)
        where TIndex : class, IDocument, IDexDocument, new()
    {
        var collection = new IndexCollectionImpl<TIndex>(GetCollection<TIndex>());
        _mockRepository.GetCollection<TIndex>(dexType.ToString()).Returns(collection);
    }

    private static ICollection<TIndex> GetCollection<TIndex>()
        where TIndex : class, IDexDocument, new()
    {
        return
        [
            new TIndex
            {
                Name = "Item 1"
            },
            new TIndex
            {
                Name = "Item 2"
            }
        ];
    }
}