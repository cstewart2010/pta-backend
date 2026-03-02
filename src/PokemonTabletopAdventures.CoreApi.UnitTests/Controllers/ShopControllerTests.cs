using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PokemonTabletopAdventures.CoreApi.Controllers.v2;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.Models.Shops;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Controllers;

public class ShopControllerTests : BasePtaControllerTests
{
    private ShopController _sut;

    [OneTimeSetUp]
    public void SetUp()
    {
        _sut = new ShopController(
            UserService,
            TrainerService,
            PokemonService,
            ShopService,
            GameService,
            DexService,
            SettingService,
            PokedexService,
            EncryptionService,
            DtoToModelMapper,
            ModelToDtoMapper,
            Substitute.For<ILogger<ShopController>>());
    }

    [Test]
    public async Task GetShopGM_Valid_ReturnsShop()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var gm = trainers.First(x => x.IsGM);
        var shops = await ShopService.GetShopsByGameId(gameId);
        var shop = shops.First();
        var request = new RetrieveShopRequest
        {
            GameId = gameId,
            SettingId = Guid.Empty,
            ShopId = shop.ShopId,
            UserId = gm.TrainerId
        };
        
        // act
        var response = await _sut.GetShopGM(string.Empty, request);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<RetrieveShopResponse>());
        });
    }

    [Test]
    public async Task GetShopTrainer_Valid_ReturnsShop()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var gm = trainers.First(x => !x.IsGM);
        var shops = await ShopService.GetShopsByGameId(gameId);
        var shop = shops.First(x => x.IsActive);
        var request = new RetrieveShopRequest
        {
            GameId = gameId,
            SettingId = Guid.Empty,
            ShopId = shop.ShopId,
            UserId = gm.TrainerId
        };
        
        // act
        var response = await _sut.GetShopTrainer(string.Empty, request);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<RetrieveShopResponse>());
        });
    }

    [Test]
    public async Task GetShopTrainer_Inactive_ThrowsUnknownEntityException()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var gm = trainers.First(x => !x.IsGM);
        var shops = await ShopService.GetShopsByGameId(gameId);
        var shop = shops.First(x => !x.IsActive);
        var request = new RetrieveShopRequest
        {
            GameId = gameId,
            SettingId = Guid.Empty,
            ShopId = shop.ShopId,
            UserId = gm.TrainerId
        };
        
        // act
        var exception = Assert.ThrowsAsync<UnknownEntityException<Shop>>(async() => await _sut.GetShopTrainer(string.Empty, request));
    }

    [Test]
    public async Task GetShops_Valid_ReturnsShops()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var gm = trainers.First(x => x.IsGM);
        var request = new RetrieveShopRequest
        {
            GameId = gameId,
            SettingId = Guid.Empty,
            ShopId = Guid.Empty,
            UserId = gm.TrainerId
        };
        
        // act
        var response = await _sut.GetShops(string.Empty, request);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<RetrieveShopResponse>());
        });
    }

    [Test]
    public async Task GetShopsBySettingGM_Valid_ReturnsShops()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var settings = await SettingService.GetAllSettings(gameId);
        var gm = trainers.First(x => x.IsGM);
        var request = new RetrieveShopRequest
        {
            GameId = gameId,
            SettingId = settings.First().SettingId,
            ShopId = Guid.Empty,
            UserId = gm.TrainerId
        };
        
        // act
        var response = await _sut.GetShopsBySettingGM(string.Empty, request);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<RetrieveShopResponse>());
        });
    }

    [Test]
    public async Task GetShopsBySettingTrainer_Valid_ReturnsShops()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var settings = await SettingService.GetAllSettings(gameId);
        var gm = trainers.First(x => !x.IsGM);
        var request = new RetrieveShopRequest
        {
            GameId = gameId,
            SettingId = settings.First(x => x.IsActive).SettingId,
            ShopId = Guid.Empty,
            UserId = gm.TrainerId
        };
        
        // act
        var response = await _sut.GetShopsBySettingTrainer(string.Empty, request);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<RetrieveShopResponse>());
        });
    }

    [Test]
    public async Task GetShopsBySettingTrainer_Invalid_ThrowsUnknownEntityException()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var settings = await SettingService.GetAllSettings(gameId);
        var setting = settings.First(x => !x.IsActive);
        var gm = trainers.First(x => !x.IsGM);
        var request = new RetrieveShopRequest
        {
            GameId = gameId,
            SettingId = setting.SettingId,
            ShopId = Guid.Empty,
            UserId = gm.TrainerId
        };
        
        // act
        var exception = Assert.Throws<AggregateException>(_sut.GetShopsBySettingTrainer(string.Empty, request).Wait);
        
        // assert
        Assert.That(exception.InnerException, Is.TypeOf<UnknownEntityException<SettingDto>>());
    }

    [Test]
    public async Task CreateShop_Valid_AddsShop()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var gm = trainers.First(x => x.IsGM);
        var request = new CreateShopRequest
        {
            Shops = [
                new Shop
                {
                    ShopId = Guid.Empty,
                    GameId = Guid.Empty,
                    Name = "Test Shop",
                    IsActive = false,
                    Inventory = []
                }
            ],
            GameMasterId = gm.TrainerId,
            GameId = gameId,
        };
        
        // act
        var response = await _sut.CreateShop(string.Empty, request);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<CreateShopResponse>());
        });
    }

    [Test]
    public async Task UpdateShop_Valid_UpdatesShop()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var shops = await ShopService.GetShopsByGameId(gameId);
        var gm = trainers.First(x => x.IsGM);
        var request = new UpdateShopRequest
        {
            Shops = shops,
            UserId = gm.TrainerId,
            GameId = gameId,
        };
        
        // act
        var response = await _sut.UpdateShop(string.Empty, request);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<UpdateShopResponse>());
        });
    }
    
    [Test]
    public async Task PurchaseFromShop_Valid_UpdatesTrainerAndShop()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var trainer = trainers.First(x => !x.IsGM);
        var shops = await ShopService.GetShopsByGameId(gameId);
        var shop = shops.First(x => x.IsActive);
        trainer.Money = int.MaxValue;
        await TrainerService.UpdateTrainer(trainer);
        var request = new UpdateShopRequest
        {
            UserId = trainer.TrainerId,
            GameId = gameId,
            Shops = [shop],
            Items =
            [
                ..shop.Inventory.Select(x => new Item
                {
                    Name = x.Key,
                    Effects = x.Value.Effects,
                    Amount = Random.Shared.Next(1, x.Value.Quantity),
                    Type = x.Value.Type
                })
            ]
        };
        
        // act
        var response = await _sut.PurchaseFromShop(string.Empty, request);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<UpdateShopResponse>());
        });
    }
    
    [Test]
    public async Task PurchaseFromShop_TooManyShops_InvalidShopException()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var trainer = trainers.First(x => !x.IsGM);
        var shops = await ShopService.GetShopsByGameId(gameId);
        var request = new UpdateShopRequest
        {
            UserId = trainer.TrainerId,
            GameId = gameId,
            Shops = shops,
            Items = []
        };
        
        // act
        var exception = Assert.Throws<AggregateException>(_sut.PurchaseFromShop(string.Empty, request).Wait);
        
        // assert
        Assert.That(exception.InnerException, Is.TypeOf<InvalidShopException>());
    }
    
    [Test]
    public async Task PurchaseFromShop_NoMoney_ThrowsInvalidShopException()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var trainer = trainers.First(x => !x.IsGM);
        var shops = await ShopService.GetShopsByGameId(gameId);
        var shop = shops.First(x => x.IsActive);
        trainer.Money = 0;
        await TrainerService.UpdateTrainer(trainer);
        var request = new UpdateShopRequest
        {
            UserId = trainer.TrainerId,
            GameId = gameId,
            Shops = [shop],
            Items =
            [
                ..shop.Inventory.Select(x => new Item
                {
                    Name = x.Key,
                    Effects = x.Value.Effects,
                    Amount = Random.Shared.Next(1, x.Value.Quantity),
                    Type = x.Value.Type
                })
            ]
        };
        
        // act
        var exception = Assert.Throws<AggregateException>(_sut.PurchaseFromShop(string.Empty, request).Wait);
        
        // assert
        Assert.That(exception.InnerException, Is.TypeOf<InvalidShopException>());
    }

    [Test]
    public async Task DeleteShop_Valid_DeletesShop()
    {
        // arrange
        var gameId = Shared.GameIds.Last();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var gm = trainers.First(x => x.IsGM);
        var shops = await ShopService.GetShopsByGameId(gameId);
        var shop = shops.First();
        var request = new DeleteShopRequest
        {
            GameId = gameId,
            ShopId = shop.ShopId,
            UserId = gm.TrainerId
        };
        
        // act
        var response = await _sut.DeleteShop(string.Empty, request);
        
        // assert
        Assert.That(response, Is.InstanceOf<OkResult>());
    }

    [Test]
    public async Task DeleteShopsByGameId_Valid_DeletesShops()
    {
        // arrange
        var gameId = Shared.GameIds.Last();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var gm = trainers.First(x => x.IsGM);
        var request = new DeleteShopRequest
        {
            GameId = gameId,
            ShopId = Guid.Empty,
            UserId = gm.TrainerId
        };
        
        // act
        var response = await _sut.DeleteShopsByGameId(string.Empty, request);
        
        // assert
        Assert.That(response, Is.InstanceOf<OkResult>());
    }
}