using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PokemonTabletopAdventures.CoreApi.Controllers.v2;
using PokemonTabletopAdventures.Models;
using PokemonTabletopAdventures.Models.Enums;
using PokemonTabletopAdventures.Models.Games;
using PokemonTabletopAdventures.Models.Npcs;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Controllers;

public class GameControllerTests : BasePtaControllerTests
{
    private GameController _sut;
    
    [OneTimeSetUp]
    public void Setup()
    {
        _sut = new GameController(
            UserService,
            TrainerService,
            PokemonService,
            NpcService,
            GameService,
            DexService,
            SpriteService,
            PokedexService,
            EncryptionService,
            DtoToModelMapper,
            ModelToDtoMapper,
            Substitute.For<ILogger<GameController>>())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Test]
    public async Task GetAllGames_Nickname_ReturnsAllMatching()
    {
        // arrange
        var userId = Shared.UserIds.First();
        
        // act
        var response = await _sut.GetAllGames(string.Empty, userId, Shared.GameIds.First().ToString());
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<RetrieveGameResponse>());
        });
    }

    [Test]
    public async Task GetAllGames_NoNickname_ReturnsUserGames()
    {
        // arrange
        var userId = Shared.UserIds.First();
        
        // act
        var response = await _sut.GetAllGames(string.Empty, userId, string.Empty);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<RetrieveGameResponse>());
        });
    }

    [Test]
    public async Task GetAllUserGames_User_ReturnsGames()
    {
        // arrange
        var userId = Shared.UserIds.First();
        
        // act
        var response = await _sut.GetAllUserGames(string.Empty, userId);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<RetrieveGameResponse>());
        });
    }

    [Test]
    public async Task GetAllSprites_ReturnsSprites()
    {
        // act
        var response = await _sut.GetAllSprites();
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.InstanceOf<ICollection<Sprite>>());
        });
    }

    [Test]
    public async Task GetGame_Valid_ReturnsGame()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        
        // act
        var response = await _sut.GetGame(gameId);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<RetrieveGameResponse>());
        });
    }
    
    [Test]
    public async Task GetLogs_Valid_ReturnsLogs()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        
        // act
        var response = await _sut.GetLogs(1, gameId);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<RetrieveLogsResponse>());
        });
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task RefreshInGame_Updates(bool isGm)
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var user = trainers.First(x => x.IsGM == isGm);
        
        // act
        var response = await _sut.RefreshInGame(string.Empty, user.TrainerId, gameId, isGm);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<RetrieveGameResponse>());
        });
    }

    [Test]
    [TestCase("test-game")]
    [TestCase(null)]
    public async Task CreateNewGame_Valid_AddsGameToCollection(string? nickname)
    {
        // arrange
        var user = await UserService.GetUserById(Shared.UserIds.First());
        var request = new CreateGameRequest
        {
            UserId = user.UserId,
            Username = user.Username,
            GameSessionPassword = "test",
            GameNickname = nickname!
        };
        
        // act
        var response = await _sut.CreateNewGame(request);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<CreateGameResponse>());
        });
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task AddLogsAsync_Updates(bool isGm)
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var user = trainers.First(x => x.IsGM == isGm);
        var request = new UpdateGameRequest
        {
            UserId = user.TrainerId,
            GameMasterId = Guid.Empty,
            GameSessionPassword = null,
            Game = new Game
            {
                Logs =
                [
                    new Log
                    {
                        Action = "test-log",
                        LogTimestamp = DateTime.Now,
                        User = user.TrainerName
                    }
                ],
                Nickname = null!,
                GameId = Guid.Empty,
                IsOnline = false,
                Trainers = null!,
                Npcs = null!,
                Settings = null!
            }
        };

        // act
        var response = await _sut.AddLogsAsync(string.Empty, request, gameId);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<UpdateGameResponse>());
        });
    }

    [Test]
    public async Task StartGame_Valid_Updates()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var user = trainers.First(x => x.IsGM);
        var request = new UpdateGameRequest
        {
            UserId = Guid.Empty,
            GameMasterId = user.TrainerId,
            GameSessionPassword = string.Empty,
            Game = new Game
            {
                Logs = [],
                Nickname = null!,
                GameId = Guid.Empty,
                IsOnline = false,
                Trainers = null!,
                Npcs = null!,
                Settings = null!
            }
        };

        // act
        var response = await _sut.StartGame(string.Empty, request, gameId);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<UpdateGameResponse>());
        });
    }

    [Test]
    public async Task EndGame_Valid_Updates()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var user = trainers.First(x => x.IsGM);
        var request = new UpdateGameRequest
        {
            UserId = Guid.Empty,
            GameMasterId = user.TrainerId,
            GameSessionPassword = string.Empty,
            Game = new Game
            {
                Logs = [],
                Nickname = null!,
                GameId = Guid.Empty,
                IsOnline = false,
                Trainers = null!,
                Npcs = null!,
                Settings = null!
            }
        };

        // act
        var response = await _sut.EndGame(string.Empty, request, gameId);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<UpdateGameResponse>());
        });
    }

    [Test]
    public async Task AddNPCsToGame_Valid_Updates()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var user = trainers.First(x => x.IsGM);
        ICollection<Npc> npcs = [..Enumerable.Range(0, 3).Select(x => new Npc
        {
            NpcId = Guid.NewGuid(),
            GameId = gameId,
            TrainerName = x.ToString(),
            Feats = [],
            TrainerClasses = [],
            TrainerStats = new Stats(),
            PokemonTeam = [],
            Level = 0,
            TrainerSkills = [],
            Age = 0,
            Gender = Gender.Genderless,
            Height = 0,
            Weight = 0,
            Description = string.Empty,
            Personality = string.Empty,
            Background = string.Empty,
            Goals = string.Empty,
            Species = "null",
            Sprite = ";",
            CurrentHP = 0
        })];
        foreach (var npc in npcs)
        {
            await NpcService.PostNpc(npc);
        }
        var request = new UpdateGameRequest
        {
            UserId = Guid.Empty,
            GameMasterId = user.TrainerId,
            GameSessionPassword = string.Empty,
            Game = new Game
            {
                Logs = [],
                Nickname = null!,
                GameId = Guid.Empty,
                IsOnline = false,
                Trainers = null!,
                Npcs = npcs,
                Settings = null!
            }
        };

        // act
        var response = await _sut.AddNPCsToGame(string.Empty, request, gameId);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<UpdateGameResponse>());
        });
    }

    [Test]
    public async Task RemovesNPCsFromGame_Valid_Updates()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var user = trainers.First(x => x.IsGM);
        ICollection<Npc> npcs = [..Enumerable.Range(0, 3).Select(x => new Npc
        {
            NpcId = Guid.NewGuid(),
            GameId = gameId,
            TrainerName = x.ToString(),
            Feats = [],
            TrainerClasses = [],
            TrainerStats = new Stats(),
            PokemonTeam = [],
            Level = 0,
            TrainerSkills = [],
            Age = 0,
            Gender = Gender.Genderless,
            Height = 0,
            Weight = 0,
            Description = string.Empty,
            Personality = string.Empty,
            Background = string.Empty,
            Goals = string.Empty,
            Species = "null",
            Sprite = ";",
            CurrentHP = 0
        })];
        foreach (var npc in npcs)
        {
            await NpcService.PostNpc(npc);
        }
        var request = new UpdateGameRequest
        {
            UserId = Guid.Empty,
            GameMasterId = user.TrainerId,
            GameSessionPassword = string.Empty,
            Game = new Game
            {
                Logs = [],
                Nickname = null!,
                GameId = Guid.Empty,
                IsOnline = false,
                Trainers = null!,
                Npcs = npcs,
                Settings = null!
            }
        };
        await _sut.AddNPCsToGame(string.Empty, request, gameId);

        // act
        var response = await _sut.RemovesNPCsFromGame(string.Empty, request, gameId);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<UpdateGameResponse>());
        });
    }

    [Test]
    public async Task DeleteGame_Valid_DeletesGame()
    {
        // arrange
        var gameId = Shared.GameIds.Last();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var user = trainers.First(x => x.IsGM);

        // act
        var response = await _sut.DeleteGame(string.Empty, gameId, user.TrainerId, string.Empty);
        
        // assert
        Assert.That(response, Is.InstanceOf<OkResult>());
    }
}