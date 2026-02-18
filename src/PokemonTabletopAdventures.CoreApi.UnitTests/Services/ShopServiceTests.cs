using System.Net;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Domain;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.CoreApi.UnitTests.Implementations;
using PokemonTabletopAdventures.Models.Enums;
using PokemonTabletopAdventures.Models.Settings;
using PokemonTabletopAdventures.Models.Shops;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Services;

internal class ShopServiceTests
{
    private ShopService _sut;
    private ShopCollectionImpl _shopCollection;
    
    [OneTimeSetUp]
    public void Setup()
    {
        var mockRepository = Substitute.For<IRepositoryService>();
        _shopCollection = new ShopCollectionImpl();
        mockRepository.GetCollection<ShopDto>(MongoCollection.Shops).Returns(_shopCollection);
        var mockLogger = Substitute.For<ILogger<ShopService>>();
        _sut = new ShopService(mockRepository, Shared.DtoToModelMapper, Shared.ModelToDtoMapper, mockLogger);
    }

    [Test]
    public async Task GetShopById_Valid_ReturnsShop()
    {
        var expected = _shopCollection.Collection.First();
        var actual = await _sut.GetShopById(expected.ShopId, expected.GameId);
        
        Assert.That(actual, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(actual.ShopId, Is.EqualTo(expected.ShopId));
            Assert.That(actual.GameId, Is.EqualTo(expected.GameId));
            Assert.That(actual.Name, Is.EqualTo(expected.Name));
            Assert.That(actual.IsActive, Is.EqualTo(expected.IsActive));
            Assert.That(actual.Inventory.Keys, Is.EquivalentTo(expected.Inventory.Keys));
            foreach (var key in expected.Inventory.Keys)
            {
                Assert.That(actual.Inventory[key].Cost, Is.EqualTo(expected.Inventory[key].Cost));
                Assert.That(actual.Inventory[key].Type, Is.EqualTo(expected.Inventory[key].Type));
                Assert.That(actual.Inventory[key].Effects, Is.EqualTo(expected.Inventory[key].Effects));
                Assert.That(actual.Inventory[key].Quantity, Is.EqualTo(expected.Inventory[key].Quantity));
            }
        });
    }

    [Test]
    public async Task GetShopsByGameId_Valid_ReturnsShops()
    {
        var gameId = Shared.GameIds.First();
        var expected = _shopCollection.Collection.Where(x => x.GameId == gameId).ToList();
        var actual = await _sut.GetShopsByGameId(gameId);
        
        Assert.That(actual, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(actual, Has.Count.EqualTo(expected.Count));
            foreach (var actualItem in actual)
            {
                Assert.That(actualItem.GameId, Is.EqualTo(gameId));
            }
        });
    }

    [Test]
    public async Task GetShopsBySetting_NoShops_ReturnsEmpty()
    {
        var setting = new Setting
        {
            Shops = [],
            SettingId = Guid.Empty,
            GameId = Guid.Empty,
            Name = string.Empty,
            IsActive = false,
            Type = (SettingType)0,
            Environment = [],
            Participants = []
        };
        
        var actual = await _sut.GetShopsBySetting(setting);
        
        Assert.That(actual, Is.Empty);
    }

    [Test]
    public async Task GetShopsBySetting_WithShops_ReturnsShops()
    {
        var gameId = Shared.GameIds.First();
        var expected = _shopCollection.Collection.Where(x => x.GameId == gameId).Take(2).ToList();
        var setting = new Setting
        {
            Shops = expected.Select(x => new Shop
            {
                ShopId = x.ShopId,
                GameId = gameId,
                Name = x.Name,
                IsActive = x.IsActive,
                Inventory = x.Inventory.ToDictionary(y => y.Key, y => new Ware
                {
                    Quantity = y.Value.Quantity,
                    Cost = y.Value.Cost,
                    Type = y.Value.Type,
                    Effects = y.Value.Effects,
                })
            }),
            SettingId = Guid.Empty,
            GameId = gameId,
            Name = string.Empty,
            IsActive = false,
            Type = (SettingType)0,
            Environment = [],
            Participants = []
        };
        
        var actual = await _sut.GetShopsBySetting(setting);
        
        Assert.That(actual, Has.Count.EqualTo(expected.Count));
    }

    [Test]
    public async Task PostShop_Success_AddsToCollection()
    {
        var shop = new Shop
        {
            IsActive = true,
            Name = string.Empty,
            GameId = Guid.NewGuid(),
            Inventory = [],
            ShopId = Guid.NewGuid()
        };
        
        await _sut.PostShop(shop);
        
        var actual = await _sut.GetShopById(shop.ShopId, shop.GameId);
        
        Assert.That(actual, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(actual.ShopId, Is.EqualTo(shop.ShopId));
            Assert.That(actual.GameId, Is.EqualTo(shop.GameId));
            Assert.That(actual.Name, Is.EqualTo(shop.Name));
            Assert.That(actual.IsActive, Is.EqualTo(shop.IsActive));
            Assert.That(actual.Inventory.Keys, Is.EquivalentTo(shop.Inventory.Keys));
            foreach (var key in shop.Inventory.Keys)
            {
                Assert.That(actual.Inventory[key].Cost, Is.EqualTo(shop.Inventory[key].Cost));
                Assert.That(actual.Inventory[key].Type, Is.EqualTo(shop.Inventory[key].Type));
                Assert.That(actual.Inventory[key].Effects, Is.EqualTo(shop.Inventory[key].Effects));
                Assert.That(actual.Inventory[key].Quantity, Is.EqualTo(shop.Inventory[key].Quantity));
            }
        });
    }

    [Test]
    public async Task PostShop_Duplicate_ThrowsException()
    {
        var shopDto = _shopCollection.Collection.First();
        var shop = await _sut.GetShopById(shopDto.ShopId, shopDto.GameId);
        var aggregatedException = Assert.Throws<AggregateException>(() =>
        {
            _sut.PostShop(shop).Wait();
        });
        
        Assert.That(aggregatedException.InnerException, Is.InstanceOf<DuplicateEntryException>());
        var exception = (DuplicateEntryException)aggregatedException.InnerException;
        Assert.Multiple(() =>
        {
            Assert.That(exception.Message, Is.EqualTo($"Duplicate entry of type {nameof(ShopDto)}"));
            Assert.That(exception.Title, Is.EqualTo(PtaExceptionParts.DuplicateEntryTitle));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        });
    }

    [Test]
    public async Task UpdateShop_Valid_UpdatesCollection()
    {
        var shopDto = _shopCollection.Collection.First();
        var shop = await _sut.GetShopById(shopDto.ShopId, shopDto.GameId);
        var updatedExcepted = new Shop
        {
            Name = shop.Name + "1",
            GameId = Guid.NewGuid(),
            Inventory = [],
            ShopId = shopDto.ShopId,
            IsActive = !shop.IsActive,
        };
        
        var updatedActual = await _sut.UpdateShop(updatedExcepted);
        Assert.That(updatedActual, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(updatedActual.ShopId, Is.EqualTo(shop.ShopId));
            Assert.That(updatedActual.GameId, Is.EqualTo(updatedExcepted.GameId));
            Assert.That(updatedActual.Name, Is.EqualTo(updatedExcepted.Name));
            Assert.That(updatedActual.IsActive, Is.EqualTo(updatedExcepted.IsActive));
            Assert.That(updatedActual.Inventory.Keys, Is.EquivalentTo(updatedExcepted.Inventory.Keys));
        });
    }

    [Test]
    public async Task UpdateShop_InvalidShopId_DoesNotUpdateCollection()
    {
        var shopDto = _shopCollection.Collection.First();
        var shop = await _sut.GetShopById(shopDto.ShopId, shopDto.GameId);
        var updatedFailure = new Shop
        {
            Name = shop.Name + "1",
            GameId = shop.GameId,
            Inventory = [],
            ShopId = Guid.NewGuid(),
            IsActive = !shop.IsActive,
        };
        
        var expectedCount = _shopCollection.Collection.Count;
        Assert.Throws<AggregateException>(() =>
        {
            _sut.UpdateShop(updatedFailure).Wait();
        });
        
        Assert.That(_shopCollection.Collection, Has.Count.EqualTo(expectedCount));
    }

    [Test]
    public async Task DeleteShop_Valid_RemovesFromCollection()
    {
        var count = _shopCollection.Collection.Count;
        var shop = new Shop
        {
            IsActive = true,
            Name = string.Empty,
            GameId = Guid.NewGuid(),
            Inventory = [],
            ShopId = Guid.NewGuid()
        };
        
        await _sut.PostShop(shop);
        await _sut.DeleteShop(shop.ShopId, shop.GameId);
        Assert.Throws<AggregateException>(_sut.GetShopById(shop.ShopId, shop.GameId).Wait);
        Assert.That(_shopCollection.Collection, Has.Count.EqualTo(count));
    }
    
    [Test]
    public async Task DeleteShopsFromGameId_Valid_RemovesFromCollection()
    {
        var count = _shopCollection.Collection.Count;
        var gameId = Guid.NewGuid();
        var shops = Enumerable.Range(0, 3).Select(x => new Shop
        {
            IsActive = true,
            Name = string.Empty,
            GameId = gameId,
            Inventory = [],
            ShopId = Guid.NewGuid()
        });
        foreach (var shop in shops)
        {
            await _sut.PostShop(shop);
        }

        await _sut.DeleteShopByGameId(gameId);
        Assert.That(_shopCollection.Collection, Has.Count.EqualTo(count));
    }
}