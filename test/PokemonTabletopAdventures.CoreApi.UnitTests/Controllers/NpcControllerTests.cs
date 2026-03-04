using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PokemonTabletopAdventures.CoreApi.Controllers.v2;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.Models;
using PokemonTabletopAdventures.Models.Enums;
using PokemonTabletopAdventures.Models.Npcs;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Controllers;

public class NpcControllerTests : BasePtaControllerTests
{
    private NpcController _sut;

    [OneTimeSetUp]
    public void SetUp()
    {
        _sut = new NpcController(
            UserService,
            TrainerService,
            PokemonService,
            NpcService,
            GameService,
            DexService,
            PokedexService,
            EncryptionService,
            DtoToModelMapper,
            ModelToDtoMapper,
            Substitute.For<ILogger<NpcController>>());
    }

    [Test]
    public async Task GetNpc_Valid_ReturnsNpc()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var gm = trainers.First(x => x.IsGM);
        var npcs = await NpcService.GetNpcsByGameId(gameId);
        var expected = npcs.First();
        var request = new RetrieveNpcRequest
        {
            GameMasterId = gm.TrainerId,
            GameId = gameId,
            NpcId = expected.NpcId
        };
        
        // act
        var response = await _sut.GetNpc(string.Empty, request);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<RetrieveNpcResponse>());
        });
    }

    [Test]
    public async Task GetNpc_InvalidNpc_ThrowsConflictInDataException()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var gm = trainers.First(x => x.IsGM);
        var npcs = await NpcService.GetNpcsByGameId(Shared.GameIds.Last());
        var expected = npcs.First();
        var request = new RetrieveNpcRequest
        {
            GameMasterId = gm.TrainerId,
            GameId = gameId,
            NpcId = expected.NpcId
        };
        
        // act
        var exception = Assert.Throws<AggregateException>(_sut.GetNpc(string.Empty, request).Wait);
        
        // assert
        Assert.That(exception.InnerException, Is.InstanceOf<ConflictInDataException>());
    }

    [Test]
    public async Task GetNpc_NotGM_ThrowsPtaUnauthorizedException()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var gm = trainers.First(x => !x.IsGM);
        var npcs = await NpcService.GetNpcsByGameId(Shared.GameIds.Last());
        var expected = npcs.First();
        var request = new RetrieveNpcRequest
        {
            GameMasterId = gm.TrainerId,
            GameId = gameId,
            NpcId = expected.NpcId
        };
        
        // act
        var exception = Assert.Throws<AggregateException>(_sut.GetNpc(string.Empty, request).Wait);
        
        // assert
        Assert.That(exception.InnerException, Is.TypeOf<PtaUnauthorizedException>());
    }

    [Test]
    public async Task GetNpcsInGame_Valid_ReturnsNpcs()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var gm = trainers.First(x => x.IsGM);
        var request = new RetrieveNpcRequest
        {
            GameMasterId = gm.TrainerId,
            GameId = gameId,
            NpcId = Guid.NewGuid()
        };
        
        // act
        var response = await _sut.GetNpcsInGame(string.Empty, request);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<RetrieveNpcResponse>());
        });
    }

    [Test]
    public async Task CreateNewNpcAsync_Valid_ReturnsNpcs()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var gm = trainers.First(x => x.IsGM);
        var request = new CreateNpcRequest
        {
            Npcs = [
                new Npc
                {
                    NpcId = Guid.Empty,
                    GameId = Guid.Empty,
                    TrainerName = "null",
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
                    Description = "null",
                    Personality = "null",
                    Background = "null",
                    Goals = "null",
                    Species = "null",
                    Sprite = "null",
                    CurrentHP = 0
                }
            ],
            GameMasterId = gm.TrainerId,
            GameId = gameId,
        };
        
        // act
        var response = await _sut.CreateNewNpcAsync(string.Empty, request);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<CreateNpcResponse>());
        });
    }

    [Test]
    public async Task AddNpcStats_Valid_ReturnsNpcs()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var gm = trainers.First(x => x.IsGM);
        var npcs = await NpcService.GetNpcsByGameId(gameId);
        var request = new UpdateNpcRequest
        {
            GameMasterId = gm.TrainerId,
            GameId = gameId,
            Npcs = npcs
        };
        
        // act
        var response = await _sut.AddNpcStats(string.Empty, request);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<UpdateNpcResponse>());
        });
    }

    [Test]
    public async Task DeleteNpc_Valid_RemovesNpc()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var gm = trainers.First(x => x.IsGM);
        var createRequest = new CreateNpcRequest
        {
            Npcs = [
                new Npc
                {
                    NpcId = Guid.Empty,
                    GameId = Guid.Empty,
                    TrainerName = "null",
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
                    Description = "null",
                    Personality = "null",
                    Background = "null",
                    Goals = "null",
                    Species = "null",
                    Sprite = "null",
                    CurrentHP = 0
                }
            ],
            GameMasterId = gm.TrainerId,
            GameId = gameId,
        };
        var createResponse = await _sut.CreateNewNpcAsync(string.Empty, createRequest);
        var createResult = (createResponse as ObjectResult)!;
        var npc = (createResult.Value as CreateNpcResponse)!.Npcs.First();
        var request = new DeleteNpcRequest
        {
            GameMasterId = gm.TrainerId,
            GameId = gameId,
            NpcId = npc.NpcId,
        };

        // act
        var response = await _sut.DeleteNpc(string.Empty, request);
        
        // assert
        var result = response as OkResult;
        Assert.That(result, Is.Not.Null);
    }

    [Test, NonParallelizable]
    public async Task DeleteNpcs_Valid_RemovesNpcs()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var gm = trainers.First(x => x.IsGM);
        var request = new DeleteNpcRequest
        {
            GameMasterId = gm.TrainerId,
            GameId = gameId,
            NpcId = Guid.NewGuid()
        };
        
        // act
        var response = await _sut.DeleteNpcsInGame(string.Empty, request);
        
        // assert
        var result = response as OkResult;
        Assert.That(result, Is.Not.Null);
    }
}