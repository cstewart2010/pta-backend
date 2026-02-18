using Microsoft.Extensions.Logging;
using NSubstitute;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Domain;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.CoreApi.UnitTests.Implementations;
using PokemonTabletopAdventures.Models;
using PokemonTabletopAdventures.Models.Enums;
using PokemonTabletopAdventures.Models.Trainers;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Services;

public class TrainerServiceTests
{
    private TrainerService _sut;
    private TrainerCollectionImpl _trainerCollection;
    private UserCollectionImpl _userCollection;
    private GameCollectionImpl _gameCollection;
    private PokemonCollectionImpl _pokemonCollection;
    private PokedexCollectionImpl _pokedexCollection;
    private NpcCollectionImpl _npcCollection;
    private ShopCollectionImpl _shopCollection;
    private SettingCollectionImpl _settingCollection;

    [OneTimeSetUp]
    public void Setup()
    {
        var mockRepository = Substitute.For<IRepositoryService>();
        var mockLogger = Substitute.For<ILogger<TrainerService>>();
        _trainerCollection = new TrainerCollectionImpl();
        _pokemonCollection = new PokemonCollectionImpl();
        _pokedexCollection = new PokedexCollectionImpl();
        _npcCollection = new NpcCollectionImpl();
        _shopCollection = new ShopCollectionImpl();
        _settingCollection = new SettingCollectionImpl(_trainerCollection,  _pokemonCollection, _npcCollection, _shopCollection);
        _gameCollection = new GameCollectionImpl();
        _userCollection = new UserCollectionImpl();
        mockRepository.GetCollection<ShopDto>(MongoCollection.Shops).Returns(_shopCollection);
        mockRepository.GetCollection<SettingDto>(MongoCollection.Settings).Returns(_settingCollection);
        mockRepository.GetCollection<GameDto>(MongoCollection.Games).Returns(_gameCollection);
        mockRepository.GetCollection<UserDto>(MongoCollection.Users).Returns(_userCollection);
        mockRepository.GetCollection<PokeDexItemDto>(MongoCollection.Pokedex).Returns(_pokedexCollection);
        mockRepository.GetCollection<PokemonDto>(MongoCollection.Pokemon).Returns(_pokemonCollection);
        mockRepository.GetCollection<TrainerDto>(MongoCollection.Trainers).Returns(_trainerCollection);
        mockRepository.GetCollection<NpcDto>(MongoCollection.NPCs).Returns(_npcCollection);
        var pokedexService = new PokedexService(mockRepository, Shared.DtoToModelMapper, Substitute.For<ILogger<PokedexService>>());
        var pokemonService = new PokemonService(mockRepository, pokedexService, Shared.DtoToModelMapper, Shared.ModelToDtoMapper, Substitute.For<ILogger<PokemonService>>());
        var shopService = new ShopService(mockRepository, Shared.DtoToModelMapper, Shared.ModelToDtoMapper, Substitute.For<ILogger<ShopService>>());
        var settingService = new SettingService(mockRepository, shopService, Shared.DtoToModelMapper, Shared.ModelToDtoMapper, Substitute.For<ILogger<SettingService>>());
        var encryptionService = new EncryptionService(mockRepository, Substitute.For<ILogger<EncryptionService>>());
        _sut = new TrainerService(mockRepository, pokemonService, pokedexService, settingService, encryptionService, Shared.DtoToModelMapper, Shared.ModelToDtoMapper, mockLogger);
    }

    [Test]
    public async Task GetTrainerById_Valid_ReturnsTrainer()
    {
        var expected = _trainerCollection.Collection.First();
        
        var actual = await _sut.GetTrainerById(expected.TrainerId, expected.GameId);
        var actualItems = actual.Items.ToList();
        var actualTrainerSkills = actual.TrainerSkills.ToList();
        var expectedTrainerSkills = expected.TrainerSkills.ToList();
        
        Assert.That(actual, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(actual.TrainerId, Is.EqualTo(expected.TrainerId));
            Assert.That(actual.GameId, Is.EqualTo(expected.GameId));
            Assert.That(actual.Age, Is.EqualTo(expected.Age));
            Assert.That(actual.Background, Is.EqualTo(expected.Background));
            Assert.That(actual.CurrentHP, Is.EqualTo(expected.CurrentHP));
            Assert.That(actual.Description, Is.EqualTo(expected.Description));
            Assert.That(actual.Feats, Is.EquivalentTo(expected.Feats));
            Assert.That(actual.Gender, Is.EqualTo(expected.Gender));
            Assert.That(actual.Goals, Is.EqualTo(expected.Goals));
            Assert.That(actual.Height, Is.EqualTo(expected.Height));
            Assert.That(actual.Weight, Is.EqualTo(expected.Weight));
            Assert.That(actual.Honors, Is.EquivalentTo(expected.Honors));
            Assert.That(actual.IsAllowed, Is.EqualTo(expected.IsAllowed));
            Assert.That(actual.IsComplete, Is.EqualTo(expected.IsComplete));
            Assert.That(actual.IsGM, Is.EqualTo(expected.IsGM));
            Assert.That(actual.IsOnline, Is.EqualTo(expected.IsOnline));
            Assert.That(actualItems, Has.Count.EqualTo(expected.Items.Count));
            Assert.That(actual.Money, Is.EqualTo(expected.Money));
            Assert.That(actual.Origin, Is.EqualTo(expected.Origin));
            Assert.That(actual.Personality, Is.EqualTo(expected.Personality));
            Assert.That(actual.Species, Is.EqualTo(expected.Species));
            Assert.That(actual.TrainerClasses, Is.EquivalentTo(expected.TrainerClasses));
            Assert.That(actual.TrainerName, Is.EqualTo(expected.TrainerName));
            Assert.That(actualTrainerSkills, Has.Count.EqualTo(expectedTrainerSkills.Count));
            Assert.That(actual.TrainerStats, Is.Not.Null);
            Assert.That(actual.CaughtTotal, Is.EqualTo(_pokedexCollection.Collection.Count(x => x.TrainerId == expected.TrainerId && x.GameId == expected.GameId && x.IsCaught)));
            Assert.That(actual.SeenTotal, Is.EqualTo(_pokedexCollection.Collection.Count(x => x.TrainerId == expected.TrainerId && x.GameId == expected.GameId && x.IsSeen)));
            Assert.That(actual.Level, Is.EqualTo(expected.Honors.Count() + 1));
            Assert.That(actual.Species, Is.EqualTo(expected.Species));
            Assert.That(actual.Sprite, Is.EqualTo(expected.Sprite));
        });
        Assert.Multiple(() =>
        {
            for (var i = 0; i < expected.Items.Count; i++)
            {
                Assert.That(actualItems[i].Name, Is.EqualTo(expected.Items[i].Name));
                Assert.That(actualItems[i].Type, Is.EqualTo(expected.Items[i].Type));
                Assert.That(actualItems[i].Amount, Is.EqualTo(expected.Items[i].Amount));
                Assert.That(actualItems[i].Effects, Is.EqualTo(expected.Items[i].Effects));
            }
        });
        Assert.Multiple(() =>
        {
            for (var i = 0; i < expectedTrainerSkills.Count; i++)
            {
                Assert.That(actualTrainerSkills[i].Name, Is.EqualTo(expectedTrainerSkills[i].Name));
                Assert.That(actualTrainerSkills[i].ModifierStat, Is.EqualTo(expectedTrainerSkills[i].ModifierStat));
                Assert.That(actualTrainerSkills[i].Talent1, Is.EqualTo(expectedTrainerSkills[i].Talent1));
                Assert.That(actualTrainerSkills[i].Talent2, Is.EqualTo(expectedTrainerSkills[i].Talent2));
            }
        });
        Assert.Multiple(() =>
        {
            Assert.That(actual.TrainerStats.HP, Is.EqualTo(expected.TrainerStats.HP));
            Assert.That(actual.TrainerStats.Attack, Is.EqualTo(expected.TrainerStats.Attack));
            Assert.That(actual.TrainerStats.SpecialAttack, Is.EqualTo(expected.TrainerStats.SpecialAttack));
            Assert.That(actual.TrainerStats.Defense, Is.EqualTo(expected.TrainerStats.Defense));
            Assert.That(actual.TrainerStats.SpecialDefense, Is.EqualTo(expected.TrainerStats.SpecialDefense));
            Assert.That(actual.TrainerStats.Speed, Is.EqualTo(expected.TrainerStats.Speed));
        });
    }

    [Test]
    public async Task GetTrainerByUsername_Valid_ReturnsTrainer()
    {
        var expected = _trainerCollection.Collection.First();
        var actual = await _sut.GetTrainerByUsername(expected.TrainerName, expected.GameId);
        Assert.That(actual, Is.Not.Null);
    }
    
    [Test]
    public async Task GetTrainersByGameId_Valid_ReturnsTrainers()
    {
        var gameId = Shared.GameIds.First();
        var expected = _trainerCollection.Collection.Where(x => x.GameId == gameId).ToList();
        var actual = await _sut.GetTrainersByGameId(gameId);
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual, Has.Count.EqualTo(expected.Count));
    }

    [Test]
    public async Task GetIncompleteTrainerById_Valid_ReturnsTrainer()
    {
        var trainer = new TrainerDto
        {
            GameId = Guid.NewGuid(),
            TrainerId = Guid.NewGuid(),
        };
        
        _trainerCollection.Collection.Add(trainer);
        
        var actual = await _sut.GetIncompleteTrainerById(trainer.TrainerId, trainer.GameId);
        
        Assert.That(actual, Is.Not.Null);
    }

    [Test]
    public async Task GetAllUserTrainers_Valid_ReturnsTrainers()
    {
        var userId = Shared.UserIds.First();
        var expected = _trainerCollection.Collection.Where(x => x.TrainerId == userId).ToList();
        var actual = await _sut.GetAllUserTrainers(userId);
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual, Has.Count.EqualTo(expected.Count));
        Assert.Multiple(() =>
        {
            foreach (var trainer in actual)
            {
                Assert.That(trainer.TrainerId, Is.EqualTo(userId));
            }
        });
    }

    [Test]
    public async Task PostTrainer_Valid_AddsToCollection()
    {
        var trainer = new Trainer
        {
            GameId = Guid.NewGuid(),
            TrainerId = Guid.NewGuid(),
            TrainerName = null,
            IsGM = false,
            IsOnline = false,
            Feats = [],
            Honors = [],
            Money = 0,
            Origin = null,
            TrainerClasses = [],
            PokemonTeam = [],
            PokemonHome = [],
            PokeDex = [],
            NewPokemon = [],
            TrainerStats = new Stats(),
            IsComplete = false,
            IsAllowed = false,
            SeenTotal = 0,
            CaughtTotal = 0,
            Level = 0,
            Items = [],
            TrainerSkills = [],
            Age = 0,
            Sprite = null,
            Gender = Gender.Genderless,
            Height = 0,
            Weight = 0,
            Description = null,
            Personality = null,
            Background = null,
            Goals = null,
            Species = null,
            CurrentHP = 0,
        };
        
        var count = _trainerCollection.Collection.Count;
        await _sut.PostTrainer(trainer);
        
        Assert.That(_trainerCollection.Collection, Has.Count.EqualTo(count + 1));
    }

    [Test]
    public async Task UpdateTrainer_Valid_UpdatesCollection()
    {
        var trainer = new Trainer
        {
            GameId = Guid.NewGuid(),
            TrainerId = Guid.NewGuid(),
            TrainerName = null,
            IsGM = false,
            IsOnline = false,
            Feats = [],
            Honors = [],
            Money = 0,
            Origin = null,
            TrainerClasses = [],
            PokemonTeam = [],
            PokemonHome = [],
            PokeDex = [],
            NewPokemon = [],
            TrainerStats = new Stats(),
            IsComplete = false,
            IsAllowed = false,
            SeenTotal = 0,
            CaughtTotal = 0,
            Level = 0,
            Items = [],
            TrainerSkills = [],
            Age = 0,
            Sprite = null,
            Gender = Gender.Genderless,
            Height = 0,
            Weight = 0,
            Description = null,
            Personality = null,
            Background = null,
            Goals = null,
            Species = null,
            CurrentHP = 0,
        };
        
        await _sut.PostTrainer(trainer);
        trainer.TrainerName = "UpdatedName";
        await _sut.UpdateTrainer(trainer);
        var updatedTrainer = _trainerCollection.Collection.First(x => x.TrainerId == trainer.TrainerId);
        
        Assert.That(updatedTrainer.TrainerName, Is.EqualTo(trainer.TrainerName));
    }

    [Test]
    public async Task UpdateTrainerHonors_Valid_UpdatesItem()
    {
        var trainer = Random.Shared.GetItems(_trainerCollection.Collection.ToArray(), 1).First();
        string[] honors = ["t1", "t2", "t3"];
        
        await _sut.UpdateTrainerHonors(trainer.TrainerId, trainer.GameId, honors);
        
        var updatedTrainer = _trainerCollection.Collection.First(x => x.TrainerId == trainer.TrainerId && x.GameId == trainer.GameId);
        Assert.That(updatedTrainer.Honors, Is.EquivalentTo(honors));
    }
}