using Microsoft.Extensions.Logging;
using NSubstitute;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Domain;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.CoreApi.UnitTests.Implementations;
using PokemonTabletopAdventures.Models.Enums;
using PokemonTabletopAdventures.Models.Settings;
using PokemonTabletopAdventures.Models.Shops;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Services;

public class SettingServiceTests
{
    private SettingService _sut;
    private TrainerCollectionImpl _trainerCollection;
    private PokemonCollectionImpl _pokemonCollection;
    private NpcCollectionImpl _npcCollection;
    private ShopCollectionImpl _shopCollection;
    private SettingCollectionImpl _settingCollection;

    [OneTimeSetUp]
    public void Setup()
    {
        var mockRepository = Substitute.For<IRepositoryService>();
        var mockLogger = Substitute.For<ILogger<SettingService>>();
        _trainerCollection = new TrainerCollectionImpl();
        _pokemonCollection = new PokemonCollectionImpl();
        _npcCollection = new NpcCollectionImpl();
        _shopCollection = new ShopCollectionImpl();
        _settingCollection = new SettingCollectionImpl(_trainerCollection,  _pokemonCollection, _npcCollection, _shopCollection);
        mockRepository.GetCollection<ShopDto>(MongoCollection.Shops).Returns(_shopCollection);
        mockRepository.GetCollection<SettingDto>(MongoCollection.Settings).Returns(_settingCollection);
        var shopService = new ShopService(mockRepository, Shared.DtoToModelMapper, Shared.ModelToDtoMapper, Substitute.For<ILogger<ShopService>>());
        _sut = new SettingService(mockRepository, shopService, Shared.DtoToModelMapper, Shared.ModelToDtoMapper, mockLogger);
    }

    [Test]
    public async Task GetSetting_IsGm_Valid_ReturnsSetting()
    {
        var expected = _settingCollection.Collection.First();
        var actual = await _sut.GetSetting(expected.SettingId, expected.GameId, true);
        
        Assert.That(actual, Is.Not.Null);
        var expectedParticipants = expected.ActiveParticipants.ToList();
        var actualParticipants = actual.Participants.ToList();
        Assert.Multiple(() =>
        {
            Assert.That(actual.SettingId, Is.EqualTo(expected.SettingId));
            Assert.That(actual.Name, Is.EqualTo(expected.Name));
            Assert.That(actual.GameId, Is.EqualTo(expected.GameId));
            Assert.That(actual.IsActive, Is.EqualTo(expected.IsActive));
            Assert.That(actual.Environment, Is.EquivalentTo(expected.Environment));
            Assert.That(actual.Shops.Select(x => x.ShopId), Is.EquivalentTo(expected.Shops));
            Assert.That(actual.Type, Is.EqualTo(expected.Type));
            Assert.That(actual.Participants, Has.Count.EqualTo(expectedParticipants.Count));
            for (var i = 0; i < expectedParticipants.Count; i++)
            {
                Assert.That(actualParticipants[i].Name, Is.EqualTo(expectedParticipants[i].Name));
                Assert.That(actualParticipants[i].Type, Is.EqualTo(expectedParticipants[i].Type));
                Assert.That(actualParticipants[i].Health, Is.EqualTo(expectedParticipants[i].Health));
                Assert.That(actualParticipants[i].ParticipantId, Is.EqualTo(expectedParticipants[i].ParticipantId));
                Assert.That(actualParticipants[i].Speed, Is.EqualTo(expectedParticipants[i].Speed));
                Assert.That(actualParticipants[i].Position.X, Is.EqualTo(expectedParticipants[i].Position.X));
                Assert.That(actualParticipants[i].Position.Y, Is.EqualTo(expectedParticipants[i].Position.Y));
            }
        });
    }

    [Test]
    public async Task GetSetting_NotIsGm_Valid_ReturnsSetting()
    {
        var expected = _settingCollection.Collection.First();
        var actual = await _sut.GetSetting(expected.SettingId, expected.GameId, false);
        
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.Shops.ToList(), Has.Count.EqualTo(1));
    }

    [Test]
    public async Task GetAllSettings_Valid_ReturnsAllSettings()
    {
        var gameId = Shared.GameIds.First();
        var settingIds = _settingCollection.Collection.Where(x => x.GameId == gameId).Select(x => x.SettingId).ToList();
        var expected = await _sut.GetAllSettings(gameId);
        
        Assert.That(expected, Has.Count.EqualTo(settingIds.Count));
        Assert.That(expected.Select(x => x.SettingId), Is.EquivalentTo(settingIds));
    }
    
    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task GetActiveSetting_Valid_ReturnsAllSettings(bool isGm)
    {
        var gameId = Shared.GameIds.First();
        var actual = (await _sut.GetActiveSetting(gameId, isGm))!;
        var shops = (await _sut.GetSetting(actual.SettingId, actual.GameId, true)).Shops;
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.Shops.ToList(), Has.Count.EqualTo(shops.Count(x => isGm ? x.GameId == gameId : x.GameId == gameId && x.IsActive)));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task GetActiveSetting_InvalidGameId_ReturnsNull(bool isGm)
    {
        var actual = await _sut.GetActiveSetting(Guid.NewGuid(), isGm);
        Assert.That(actual, Is.Null);
    }
    
    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task GetActiveSetting_NoActiveSetting_ReturnsNull(bool isGm)
    {
        var dto = new SettingDto
        {
            SettingId = Guid.NewGuid(),
            GameId = Guid.NewGuid(),
        };
        _settingCollection.Collection.Add(dto);
        var actual =  await _sut.GetActiveSetting(dto.GameId, isGm);
        Assert.That(actual, Is.Null);
    }

    [Test]
    public async Task PostSetting_Valid_AddsToCollection()
    {
        var model = new Setting
        {
            SettingId = Guid.NewGuid(),
            GameId = Guid.NewGuid(),
            Participants =
            [
                new SettingParticipant
                {
                    Health = "test 2",
                    ParticipantId = Guid.NewGuid(),
                    Name = "test 2",
                    Position = new MapPosition
                    {
                        X = 1,
                        Y = 10,
                    },
                    Speed = 10,
                    Type = SettingParticipantType.Pokemon
                }
            ],
            Environment = ["test 1"],
            IsActive = true,
            Name = "Test",
            Shops = [new Shop
            {
                GameId = Guid.NewGuid(),
                Inventory = [],
                IsActive = false,
                Name = "Test",
                ShopId = Guid.NewGuid(),
            }],
            Type = SettingType.Hybrid
        };

        var count = _settingCollection.Collection.Count;
        await _sut.PostSetting(model);
        
        Assert.That(_settingCollection.Collection, Has.Count.EqualTo(count + 1));
        var actual = _settingCollection.Collection.First(x => x.SettingId == model.SettingId);
        Assert.That(actual, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(actual.SettingId, Is.EqualTo(model.SettingId));
            Assert.That(actual.GameId, Is.EqualTo(model.GameId));
            Assert.That(actual.Name, Is.EqualTo(model.Name));
            Assert.That(actual.Environment, Is.EquivalentTo(model.Environment));
            Assert.That(actual.Shops, Is.EquivalentTo(model.Shops.Select(x => x.ShopId)));
            Assert.That(actual.Type, Is.EqualTo(model.Type));
            Assert.That(actual.ActiveParticipants.ToList(), Has.Count.EqualTo(model.Participants.Count));
        });
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task UpdateSetting_Valid_UpdatesCollection(bool isGm)
    {
        var gameId = Shared.GameIds.First();
        var model = (await _sut.GetActiveSetting(gameId, isGm))!;
        model.Environment = [..model.Environment, "additional"];
        var updatedModel = await _sut.UpdateSetting(model, isGm);
        Assert.That(updatedModel, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(updatedModel.SettingId, Is.EqualTo(model.SettingId));
            Assert.That(updatedModel.Environment, Is.EquivalentTo(model.Environment));
        });
    }

    [Test]
    public async Task DeleteSetting_Valid_RemoveFromCollection()
    {
        var model = new Setting
        {
            SettingId = Guid.NewGuid(),
            GameId = Guid.NewGuid(),
            Participants =
            [
                new SettingParticipant
                {
                    Health = "test 2",
                    ParticipantId = Guid.NewGuid(),
                    Name = "test 2",
                    Position = new MapPosition
                    {
                        X = 1,
                        Y = 10,
                    },
                    Speed = 10,
                    Type = SettingParticipantType.Pokemon
                }
            ],
            Environment = ["test 1"],
            IsActive = true,
            Name = "Test",
            Shops = [new Shop
            {
                GameId = Guid.NewGuid(),
                Inventory = [],
                IsActive = false,
                Name = "Test",
                ShopId = Guid.NewGuid(),
            }],
            Type = SettingType.Hybrid
        };

        var count = _settingCollection.Collection.Count;
        await _sut.PostSetting(model);
        await _sut.DeleteSetting(model.SettingId, model.GameId);
        
        Assert.That(_settingCollection.Collection, Has.Count.EqualTo(count));
    }

    [Test]
    public async Task DeleteSettingsByGameId_Valid_RemoveFromCollection()
    {
        var gameId = Guid.NewGuid();
        ICollection<Setting> models =
        [
            ..Enumerable.Range(0, 3).Select(x => new Setting
            {
                SettingId = Guid.NewGuid(),
                GameId = gameId,
                Participants =
                [
                    new SettingParticipant
                    {
                        Health = "test 2",
                        ParticipantId = Guid.NewGuid(),
                        Name = "test 2",
                        Position = new MapPosition
                        {
                            X = 1,
                            Y = 10,
                        },
                        Speed = 10,
                        Type = SettingParticipantType.Pokemon
                    }
                ],
                Environment = ["test 1"],
                IsActive = true,
                Name = "Test",
                Shops =
                [
                    new Shop
                    {
                        GameId = Guid.NewGuid(),
                        Inventory = [],
                        IsActive = false,
                        Name = "Test",
                        ShopId = Guid.NewGuid(),
                    }
                ],
                Type = SettingType.Hybrid
            })
        ];

        var count = _settingCollection.Collection.Count;
        foreach (var model in models)
        {
            await _sut.PostSetting(model);
        }
        await _sut.DeleteSettingsByGameId(gameId);
        
        Assert.That(_settingCollection.Collection, Has.Count.EqualTo(count));
    }
}