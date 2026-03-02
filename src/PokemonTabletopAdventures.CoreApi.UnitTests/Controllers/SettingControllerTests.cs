using System.Net.WebSockets;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PokemonTabletopAdventures.CoreApi.Controllers.v2;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.Models.Enums;
using PokemonTabletopAdventures.Models.Pokemons;
using PokemonTabletopAdventures.Models.Settings;
using PokemonTabletopAdventures.Models.Shops;
using PokemonTabletopAdventures.Models.Trainers;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Controllers;

public class SettingControllerTests : BasePtaControllerTests
{
    private SettingController _sut;

    [OneTimeSetUp]
    public void SetUp()
    {
        _sut = new SettingController(
            UserService,
            TrainerService,
            PokemonService,
            SettingService,
            GameService,
            DexService,
            EncryptionService,
            NpcService,
            PokedexService,
            DtoToModelMapper,
            ModelToDtoMapper,
            Substitute.For<ILogger<SettingController>>())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Test]
    public void GetEnvironments_ReturnsEnvironment()
    {
        // act
        var response = _sut.GetEnvironments();
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.InstanceOf<ICollection<Environments>>());
        });
    }

    [Test]
    public async Task GetAllSettings_Valid_ReturnsRetrieveSettingResponse()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var request = new RetrieveSettingRequest
        {
            GameId = gameId,
            GameMasterId = trainers.First(x => x.IsGM).TrainerId
        };
        
        // act
        var response = await _sut.GetAllSettings(string.Empty, request);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.InstanceOf<RetrieveSettingResponse>());
        });
    }

    [Test]
    public async Task CreateSetting_Valid_CreatesSetting()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var request = new CreateSettingRequest
        {
            GameId = gameId,
            GameMasterId = trainers.First(x => x.IsGM)
                .TrainerId,
            Name = "new setting",
            Type = SettingType.NonHostile
        };
        
        // act
        var response = await _sut.CreateSetting(string.Empty, request);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.InstanceOf<CreateSettingResponse>());
        });
    }

    [Test]
    public async Task SetEnvironment_Valid_UpdatesSetting()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var request = new UpdateSettingRequest
        {
            GameId = gameId,
            TrainerId = trainers.First(x => x.IsGM)
                .TrainerId,
            Setting = new Setting
            {
                Type = SettingType.NonHostile,
                SettingId = Guid.Empty,
                GameId = Guid.Empty,
                Name = null!,
                IsActive = false,
                Environment = [],
                Shops = null!,
                Participants = null!
            }
        };
        
        // act
        var response = await _sut.SetEnvironment(string.Empty, request);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.InstanceOf<UpdateSettingResponse>());
        });
    }

    [Test]
    public async Task AddToActiveSettingAsync_Valid_ReturnsOk()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var activeSetting = await SettingService.GetActiveSetting(gameId, true);
        var participant = activeSetting!.Participants.First(x => x.Type == SettingParticipantType.Trainer);
        var newParticipant = new SettingParticipant
        {
            ParticipantId = Guid.NewGuid(),
            Position = new MapPosition
            {
                X = participant.Position.X,
                Y = participant.Position.Y - 1,
            }
        };
        var request = new UpdateSettingRequest
        {
            GameId = gameId,
            TrainerId = trainers.First(x => x.IsGM)
                .TrainerId,
            Setting = new Setting
            {
                Type = SettingType.NonHostile,
                SettingId = default,
                GameId = default,
                Name = null!,
                IsActive = false,
                Environment = [],
                Shops = null!,
                Participants = [newParticipant]
            }
        };
        
        // act
        var response = await _sut.AddToActiveSettingAsync(string.Empty, request);
        
        // assert
        var result = response as OkResult;
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task AddToActiveSettingAsync_TooManyParticipants_ThrowsInvalidSettingException()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var request = new UpdateSettingRequest
        {
            GameId = gameId,
            TrainerId = trainers.First(x => x.IsGM)
                .TrainerId,
            Setting = new Setting
            {
                Type = SettingType.NonHostile,
                SettingId = default,
                GameId = default,
                Name = null!,
                IsActive = false,
                Environment = [],
                Shops = null!,
                Participants = [new SettingParticipant(), new SettingParticipant()]
            }
        };
        
        // act
        Assert.ThrowsAsync<InvalidSettingException>(() => _sut.AddToActiveSettingAsync(string.Empty, request));
    }

    [Test]
    public async Task AddToActiveSettingAsync_CurrentPariticipant_ReturnConflict()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var activeSetting = await SettingService.GetActiveSetting(gameId, true);
        var participant = activeSetting!.Participants.First(x => x.Type == SettingParticipantType.Trainer);
        var newParticipant = new SettingParticipant
        {
            ParticipantId = participant.ParticipantId,
            Position = new MapPosition
            {
                X = participant.Position.X,
                Y = participant.Position.Y - 1,
            }
        };
        participant.ParticipantId = Guid.NewGuid();
        participant.Position.X = participant.Position.Y - 1;
        var request = new UpdateSettingRequest
        {
            GameId = gameId,
            TrainerId = trainers.First(x => x.IsGM)
                .TrainerId,
            Setting = new Setting
            {
                Type = SettingType.NonHostile,
                SettingId = default,
                GameId = default,
                Name = null!,
                IsActive = false,
                Environment = [],
                Shops = null!,
                Participants = [newParticipant]
            }
        };
        
        // act
        var response = await _sut.AddToActiveSettingAsync(string.Empty, request);
        
        // assert
        var result = response as ConflictResult;
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task RemoveFromParticipants_Valid_ReturnOk()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var activeSetting = await SettingService.GetActiveSetting(gameId, true);
        var participant = activeSetting!.Participants.First(x => x.Type == SettingParticipantType.Trainer);
        var request = new UpdateSettingRequest
        {
            GameId = gameId,
            TrainerId = trainers.First(x => x.IsGM)
                .TrainerId,
            Setting = new Setting
            {
                Type = SettingType.NonHostile,
                SettingId = default,
                GameId = default,
                Name = null!,
                IsActive = false,
                Environment = [],
                Shops = null!,
                Participants = [participant]
            }
        };
        
        // act
        var response = await _sut.RemoveFromActiveSetting(string.Empty, request);
        
        // assert
        var result = response as OkResult;
        Assert.That(result, Is.Not.Null);
    }
    
    [Test]
    public async Task ReturnToPokeball_Valid_ReturnsOk()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var trainer = trainers.First(x => !x.IsGM);
        var pokemon = trainer.PokemonHome.First();
        var request = new UpdateSettingRequest
        {
            GameId = gameId,
            TrainerId = trainer.TrainerId,
            Setting = Activator.CreateInstance<Setting>()
        };
        
        // act
        var response = await _sut.ReturnToPokeball(string.Empty, request, pokemon.PokemonId);
        
        // assert
        var result = response as OkResult;
        Assert.That(result, Is.Not.Null);
    }
    
    [Test]
    public async Task ReturnToPokeball_InvalidPokemonId_ThrowsUnknownEntityException()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var trainer = trainers.First(x => !x.IsGM);
        var request = new UpdateSettingRequest
        {
            GameId = gameId,
            TrainerId = trainer.TrainerId,
            Setting = Activator.CreateInstance<Setting>()
        };
        
        // act
        Assert.ThrowsAsync<UnknownEntityException<Pokemon>>(() => _sut.ReturnToPokeball(string.Empty, request, Guid.NewGuid()));
    }
    
    [Test]
    [TestCase(nameof(nickname))]
    [TestCase("")]
    public async Task CatchPokemon_Valid_ReturnsOk(string nickname)
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var pokemon = Activator.CreateInstance<Pokemon>();
        pokemon.GameId = gameId;
        pokemon.OriginalTrainerId = Guid.Empty;
        pokemon.TrainerId = Guid.Empty;
        pokemon.CatchRate = 200;
        pokemon.PokemonId = Guid.NewGuid();
        await PokemonService.PostPokemon(pokemon);
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var trainer = trainers.First(x => !x.IsGM);
        ICollection<Item> items =
        [
            ..trainer.Items, new Item
            {
                Amount = 1,
                Name = "Basic Ball",
            }
        ];
        await TrainerService.UpdateTrainerItemList(trainer.TrainerId, gameId, items);
        var request = new UpdateSettingRequest
        {
            GameId = gameId,
            TrainerId = trainer.TrainerId,
            Setting = Activator.CreateInstance<Setting>()
        };
        
        // act
        var response = await _sut.CatchPokemon(string.Empty, request, pokemon.PokemonId, "Basic Ball", nickname);
        
        // assert
        var result = response as OkResult;
        Assert.That(result, Is.Not.Null);
    }
    
    [Test]
    public async Task CatchPokemon_NonEmptyTrainerId_ThrowInvalidCatch()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var pokemon = Activator.CreateInstance<Pokemon>();
        pokemon.GameId = gameId;
        pokemon.OriginalTrainerId = Guid.NewGuid();
        pokemon.TrainerId = Guid.NewGuid();
        pokemon.CatchRate = 200;
        pokemon.PokemonId = Guid.NewGuid();
        await PokemonService.PostPokemon(pokemon);
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var trainer = trainers.First(x => !x.IsGM);
        ICollection<Item> items =
        [
            ..trainer.Items, new Item
            {
                Amount = 1,
                Name = "Basic Ball",
            }
        ];
        await TrainerService.UpdateTrainerItemList(trainer.TrainerId, gameId, items);
        var request = new UpdateSettingRequest
        {
            GameId = gameId,
            TrainerId = trainer.TrainerId,
            Setting = Activator.CreateInstance<Setting>()
        };
        
        // act
        Assert.ThrowsAsync<InvalidCatchException>(() => _sut.CatchPokemon(string.Empty, request, pokemon.PokemonId, "Basic Ball", "nickname"));
    }
    
    [Test]
    public async Task CatchPokemon_OfflineTrainer_ThrowInvalidCatch()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var pokemon = Activator.CreateInstance<Pokemon>();
        pokemon.GameId = gameId;
        pokemon.OriginalTrainerId = Guid.Empty;
        pokemon.TrainerId = Guid.Empty;
        pokemon.CatchRate = 200;
        pokemon.PokemonId = Guid.NewGuid();
        await PokemonService.PostPokemon(pokemon);
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var trainer = trainers.Last(x => !x.IsGM);
        ICollection<Item> items =
        [
            ..trainer.Items, new Item
            {
                Amount = 1,
                Name = "Basic Ball",
            }
        ];
        await TrainerService.UpdateTrainerItemList(trainer.TrainerId, gameId, items);
        await TrainerService.UpdateTrainerOnlineStatus(trainer.TrainerId, gameId, false);
        var request = new UpdateSettingRequest
        {
            GameId = gameId,
            TrainerId = trainer.TrainerId,
            Setting = Activator.CreateInstance<Setting>()
        };
        
        // act
        Assert.ThrowsAsync<InvalidCatchException>(() => _sut.CatchPokemon(string.Empty, request, pokemon.PokemonId, "Basic Ball", "nickname"));
    }
    
    [Test]
    public async Task CatchPokemon_Invalid_ThrowsItemNotFoundException()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var pokemon = Activator.CreateInstance<Pokemon>();
        pokemon.GameId = gameId;
        pokemon.OriginalTrainerId = Guid.Empty;
        pokemon.TrainerId = Guid.Empty;
        pokemon.CatchRate = 200;
        pokemon.PokemonId = Guid.NewGuid();
        await PokemonService.PostPokemon(pokemon);
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var trainer = trainers.First(x => !x.IsGM);
        ICollection<Item> items =
        [
            ..trainer.Items, new Item
            {
                Amount = 1,
                Name = "Basic Ball",
            }
        ];
        await TrainerService.UpdateTrainerItemList(trainer.TrainerId, gameId, items);
        var request = new UpdateSettingRequest
        {
            GameId = gameId,
            TrainerId = trainer.TrainerId,
            Setting = Activator.CreateInstance<Setting>()
        };
        
        // act
        Assert.ThrowsAsync<ItemNotFoundException>(() => _sut.CatchPokemon(string.Empty, request, pokemon.PokemonId, "Great Ball", "nickname"));
    }

    [Test]
    public async Task UpdatePositionAsync_Valid_ReturnsOk()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var trainer = trainers.Last(x => !x.IsGM);
        var activeSetting = await SettingService.GetActiveSetting(gameId, true);
        var participant = activeSetting!.Participants.First(x => x.ParticipantId == trainer.TrainerId);
        var updatedParticipant = new SettingParticipant
        {
            Health = participant.Health,
            Name = participant.Name,
            Position = new MapPosition
            {
                X = participant.Position.X,
                Y = participant.Position.Y + 1,
            },
            ParticipantId = participant.ParticipantId,
            Speed = participant.Speed,
            Type = participant.Type,
        };
        var request = new UpdateSettingRequest
        {
            GameId = gameId,
            TrainerId = trainers.First(x => x.IsGM).TrainerId,
            Setting = new Setting
            {
                Type = SettingType.NonHostile,
                SettingId = default,
                GameId = default,
                Name = null!,
                IsActive = false,
                Environment = [],
                Shops = null!,
                Participants = [updatedParticipant]
            }
        };
        
        // act
        var response = await _sut.UpdatePositionAsync(string.Empty, request);
        
        // assert
        var result = response as OkResult;
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task UpdatePositionAsync_IsPositionInConflict_ConflictInDataException()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var trainer = trainers.Last(x => !x.IsGM);
        var activeSetting = await SettingService.GetActiveSetting(gameId, true);
        var participant = activeSetting!.Participants.First(x => x.ParticipantId == trainer.TrainerId);
        var updatedParticipant = new SettingParticipant
        {
            Health = participant.Health,
            Name = participant.Name,
            Position = new MapPosition
            {
                X = participant.Position.X + 1,
                Y = participant.Position.Y + 1,
            },
            ParticipantId = participant.ParticipantId,
            Speed = participant.Speed,
            Type = participant.Type,
        };
        var request = new UpdateSettingRequest
        {
            GameId = gameId,
            TrainerId = trainers.First(x => x.IsGM).TrainerId,
            Setting = new Setting
            {
                Type = SettingType.NonHostile,
                SettingId = default,
                GameId = default,
                Name = null!,
                IsActive = false,
                Environment = [],
                Shops = null!,
                Participants = [updatedParticipant]
            }
        };
        
        // act
        Assert.ThrowsAsync<ConflictInDataException>(() => _sut.UpdatePositionAsync(string.Empty, request));
    }

    [Test]
    public async Task UpdateTrainerPositionAsync_Valid_ReturnsOk()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var trainer = trainers.Last(x => !x.IsGM);
        var activeSetting = await SettingService.GetActiveSetting(gameId, true);
        var participant = activeSetting!.Participants.First(x => x.ParticipantId == trainer.TrainerId);
        var updatedParticipant = new SettingParticipant
        {
            Health = participant.Health,
            Name = participant.Name,
            Position = new MapPosition
            {
                X = participant.Position.X,
                Y = participant.Position.Y + 2,
            },
            ParticipantId = participant.ParticipantId,
            Speed = participant.Speed,
            Type = participant.Type,
        };
        var request = new UpdateSettingRequest
        {
            GameId = gameId,
            TrainerId = trainer.TrainerId,
            Setting = new Setting
            {
                Type = SettingType.NonHostile,
                SettingId = default,
                GameId = default,
                Name = null!,
                IsActive = false,
                Environment = [],
                Shops = null!,
                Participants = [updatedParticipant]
            }
        };

        // act
        var response = await _sut.UpdateTrainerPositionAsync(string.Empty, request);

        // assert
        var result = response as OkResult;
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task UpdateTrainerPositionAsync_TooFar_ThrowsInvalidSettingException()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var trainer = trainers.Last(x => !x.IsGM);
        var activeSetting = await SettingService.GetActiveSetting(gameId, true);
        var participant = activeSetting!.Participants.First(x => x.ParticipantId == trainer.TrainerId);
        var updatedParticipant = new SettingParticipant
        {
            Health = participant.Health,
            Name = participant.Name,
            Position = new MapPosition
            {
                X = participant.Position.X,
                Y = participant.Position.Y + 10,
            },
            ParticipantId = participant.ParticipantId,
            Speed = participant.Speed,
            Type = participant.Type,
        };
        var request = new UpdateSettingRequest
        {
            GameId = gameId,
            TrainerId = trainers.First(x => x.IsGM).TrainerId,
            Setting = new Setting
            {
                Type = SettingType.NonHostile,
                SettingId = default,
                GameId = default,
                Name = null!,
                IsActive = false,
                Environment = [],
                Shops = null!,
                Participants = [updatedParticipant, updatedParticipant]
            }
        };
        
        // act
        Assert.ThrowsAsync<InvalidSettingException>(() => _sut.UpdateTrainerPositionAsync(string.Empty, request));
    }

    [Test]
    public async Task UpdateTrainerPokemonPositionAsync_Valid_ReturnsOk()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var trainer = trainers.First(x => !x.IsGM);
        var pokemon = await PokemonService.GetPokemonByTrainerId(trainer.TrainerId, gameId);
        var lastPokemon = pokemon.Last();
        var activeSetting = await SettingService.GetActiveSetting(gameId, true);
        var participant = activeSetting!.Participants.First(x => x.ParticipantId == lastPokemon.PokemonId);
        var updatedParticipant = new SettingParticipant
        {
            Health = participant.Health,
            Name = participant.Name,
            Position = new MapPosition
            {
                X = participant.Position.X,
                Y = participant.Position.Y + 2,
            },
            ParticipantId = participant.ParticipantId,
            Speed = participant.Speed,
            Type = participant.Type,
        };
        var request = new UpdateSettingRequest
        {
            GameId = gameId,
            TrainerId = trainer.TrainerId,
            Setting = new Setting
            {
                Type = SettingType.NonHostile,
                SettingId = default,
                GameId = default,
                Name = null!,
                IsActive = false,
                Environment = [],
                Shops = null!,
                Participants = [updatedParticipant]
            }
        };

        // act
        var response = await _sut.UpdateTrainerPokemonPositionAsync(string.Empty, request, lastPokemon.PokemonId);

        // assert
        var result = response as OkResult;
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task SetSettingToActive_Valid_ActivatesSetting()
    {
        // arrange
        var gameId = Shared.GameIds.Last();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var setting = await SettingService.GetActiveSetting(gameId, true);
        setting!.IsActive = false;
        await SettingService.UpdateSetting(setting, true);
        var request = new UpdateSettingRequest
        {
            GameId = gameId,
            TrainerId = trainers.First(x => x.IsGM).TrainerId,
            Setting = setting
        };
        
        // act
        var response = await _sut.SetSettingToActive(string.Empty, request);
        
        // assert
        var result = response as OkResult;
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task SetSettingToActive_Invalid_ReturnsConflict()
    {
        // arrange
        var gameId = Shared.GameIds.Last();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var setting = await SettingService.GetActiveSetting(gameId, true);
        var request = new UpdateSettingRequest
        {
            GameId = gameId,
            TrainerId = trainers.First(x => x.IsGM).TrainerId,
            Setting = setting!
        };
        
        // act
        var response = await _sut.SetSettingToActive(string.Empty, request);
        
        // assert
        var result = response as ConflictResult;
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task SetSettingToInactive_Valid_DeactivatesSetting()
    {
        // arrange
        var gameId = Shared.GameIds.Last();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var setting = await SettingService.GetActiveSetting(gameId, true);
        var request = new UpdateSettingRequest
        {
            GameId = gameId,
            TrainerId = trainers.First(x => x.IsGM).TrainerId,
            Setting = setting!
        };
        
        // act
        var response = await _sut.SetSettingToInactive(string.Empty, request);
        
        // assert
        var result = response as OkResult;
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task UpdateParticipantsHp_Valid_UpdatesHealth()
    {
        // arrange
        var gameId = Shared.GameIds.ElementAt(1);
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var setting = await SettingService.GetActiveSetting(gameId, true);
        var request = new UpdateSettingRequest
        {
            GameId = gameId,
            TrainerId = trainers.First(x => x.IsGM).TrainerId,
            Setting = setting!
        };
        
        // act
        var response = await _sut.SetSettingToInactive(string.Empty, request);
        
        // assert
        var result = response as OkResult;
        Assert.That(result, Is.Not.Null);
    }

    [Test, NonParallelizable]
    public async Task DeleteSetting_Valid_RemovesSetting()
    {
        // arrange
        var gameId = Shared.GameIds.ElementAt(1);
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var gameMaster = trainers.First(x => x.IsGM);
        var settings = await SettingService.GetAllSettings(gameId);
        
        // act
        var response = await _sut.DeleteSetting(string.Empty, gameId, gameMaster.TrainerId, settings.First().SettingId);
        
        // assert
        var result = response as OkResult;
        Assert.That(result, Is.Not.Null);
    }

    [Test, NonParallelizable]
    public async Task DeleteSettings_Valid_RemovesSettings()
    {
        // arrange
        var gameId = Shared.GameIds.ElementAt(1);
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var gameMaster = trainers.First(x => x.IsGM);
        
        // act
        var response = await _sut.DeleteSettings(string.Empty, gameId, gameMaster.TrainerId);
        
        // assert
        var result = response as OkResult;
        Assert.That(result, Is.Not.Null);
    }
}