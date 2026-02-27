using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PokemonTabletopAdventures.CoreApi.Controllers.v2;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.Models.Enums;
using PokemonTabletopAdventures.Models.Pokedex;
using PokemonTabletopAdventures.Models.Pokemons;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Controllers;

public class PokemonControllerTests : BasePtaControllerTests
{
    private PokemonController _sut;

    [OneTimeSetUp]
    public void Setup()
    {
        _sut = new PokemonController(
            UserService,
            TrainerService,
            PokemonService,
            PokedexService,
            GameService,
            DexService,
            EncryptionService,
            DtoToModelMapper,
            ModelToDtoMapper,
            Substitute.For<ILogger<PokemonController>>());
    }

    [Test]
    public async Task GetPokemon_Valid_ReturnsPokemon()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var trainer = trainers.First(x => !x.IsGM);
        var pokemon = trainer.PokemonTeam.First();
        var request = new RetrievePokemonRequest
        {
            GameMasterId = Guid.Empty,
            GameId = gameId,
            TrainerId = trainer.TrainerId,
            PokemonId = pokemon.PokemonId
        };

        // act
        var response = await _sut.GetPokemon(string.Empty, request);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<RetrievePokemonResponse>());
        });
    }

    [Test]
    public async Task GetTrainerMon_Valid_ReturnsPokemon()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var trainer = trainers.First(x => !x.IsGM);
        var request = new RetrievePokemonRequest
        {
            GameMasterId = Guid.Empty,
            GameId = gameId,
            TrainerId = trainer.TrainerId,
            PokemonId = Guid.Empty
        };

        // act
        var response = await _sut.GetTrainerMon(request);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<RetrievePokemonResponse>());
        });
    }

    [Test]
    public async Task GetNpcMon_Valid_ReturnsPokemon()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var trainer = trainers.First(x => !x.IsGM);
        var gm = trainers.First(x => x.IsGM);
        var request = new RetrievePokemonRequest
        {
            GameMasterId = gm.TrainerId,
            GameId = gameId,
            TrainerId = trainer.TrainerId,
            PokemonId = Guid.Empty
        };

        // act
        var response = await _sut.GetNpcMon(string.Empty, request);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<RetrievePokemonResponse>());
        });
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task GetPossibleEvolutions_Valid_ReturnsPokemon(bool isGm)
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var trainer = trainers.First(x => !x.IsGM);
        var gm = trainers.First(x => x.IsGM);
        var request = new RetrievePokemonRequest
        {
            GameMasterId = Guid.Empty,
            GameId = gameId,
            TrainerId = isGm ? gm.TrainerId : trainer.TrainerId,
            PokemonId = trainer.PokemonTeam.First().PokemonId
        };

        // act
        var response = await _sut.GetPossibleEvolutions(string.Empty, request);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<RetrievePokemonResponse>());
        });
    }
    
    [Test]
    public async Task GetPossibleEvolutions_Invalid_ThrowsPtaUnauthorizedException()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var trainer = trainers.First(x => !x.IsGM);
        var gm = trainers.First(x => x.IsGM);
        var request = new RetrievePokemonRequest
        {
            GameMasterId = Guid.Empty,
            GameId = gameId,
            TrainerId = trainers.Last(x => !x.IsGM).TrainerId,
            PokemonId = trainer.PokemonTeam.First().PokemonId
        };

        // act
        var exception = Assert.Throws<AggregateException>( _sut.GetPossibleEvolutions(string.Empty, request).Wait);
        
        // assert
        Assert.That(exception.InnerException, Is.TypeOf<PtaUnauthorizedException>());
    }

    [Test]
    public async Task TradePokemon_Valid_TradesPokemon()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var leftTrainer = trainers.First(x => !x.IsGM);
        var leftPokemon = leftTrainer.PokemonTeam.First();
        var rightTrainer = trainers.Last(x => !x.IsGM);
        var rightPokemon = rightTrainer.PokemonTeam.First();
        var gm = trainers.First(x => x.IsGM);
        var request = new TradePokemonRequest
        {
            GameMasterId = gm.TrainerId,
            GameId = gameId,
            LeftPokemonId = leftPokemon.PokemonId,
            RightPokemonId = rightPokemon.PokemonId,
        };

        // act
        var response = await _sut.TradePokemon(string.Empty, request);
        
        // assert
        Assert.That(response, Is.InstanceOf<OkResult>());
    }

    [Test]
    public async Task TradePokemon_SameTrainer_ThrowsInvalidTradeException()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var leftTrainer = trainers.First(x => !x.IsGM);
        var leftPokemon = leftTrainer.PokemonTeam.First();
        var rightPokemon = leftTrainer.PokemonHome.First();
        var gm = trainers.First(x => x.IsGM);
        var request = new TradePokemonRequest
        {
            GameMasterId = gm.TrainerId,
            GameId = gameId,
            LeftPokemonId = leftPokemon.PokemonId,
            RightPokemonId = rightPokemon.PokemonId,
        };

        // act
        var exception = Assert.Throws<AggregateException>(_sut.TradePokemon(string.Empty, request).Wait);
        
        // assert
        Assert.That(exception.InnerException, Is.InstanceOf<InvalidTradeException>());
    }

    [Test]
    public async Task CapturePokemon_DefinedEnums_AddPokemon()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var trainer = trainers.First(x => !x.IsGM);
        var gm = trainers.First(x => x.IsGM);
        var request = new CapturePokemonRequest
        {
            GameMasterId = gm.TrainerId,
            GameId = gameId,
            TrainerId = trainer.TrainerId,
            Pokemon = new WildPokemon
            {
                Form = "base",
                Gender = Gender.Male,
                Nature = Nature.Adamant,
                Status = Status.Normal,
                Pokemon = "Ivysaur",
                ForceShiny = true
            }
        };

        // act
        var response = await _sut.CapturePokemon(string.Empty, request);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<CapturePokemonResponse>());
        });
    }

    [Test]
    public async Task CapturePokemon_UndefinedEnums_AddPokemon()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var trainer = trainers.First(x => !x.IsGM);
        var gm = trainers.First(x => x.IsGM);
        var request = new CapturePokemonRequest
        {
            GameMasterId = gm.TrainerId,
            GameId = gameId,
            TrainerId = trainer.TrainerId,
            Pokemon = new WildPokemon
            {
                Form = "base",
                Gender = (Gender)(-1),
                Nature = (Nature)(-1),
                Status = (Status)(-1),
                Pokemon = "Ivysaur",
                ForceShiny = false
            }
        };

        // act
        var response = await _sut.CapturePokemon(string.Empty, request);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<CapturePokemonResponse>());
        });
    }

    [Test]
    [TestCase("Ivysaur")]
    [TestCase("IvysaurIvysaurIvysaur")]
    public async Task CreateNewNpcMonAsync_Valid_AddsToCollection(string nickname)
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var trainer = trainers.First(x => !x.IsGM);
        var gm = trainers.First(x => x.IsGM);
        var request = new CreatePokemonRequest
        {
            GameMasterId = gm.TrainerId,
            GameId = gameId,
            TrainerId = trainer.TrainerId,
            Pokemon = [new NewPokemon
                {
                    Form = "base",
                    SpeciesName = "Ivysaur",
                    Nickname = nickname,
                    IsOnActiveTeam = false
                }
            ]
        };

        // act
        var response = await _sut.CreateNewNpcMonAsync(string.Empty, request);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<CreatePokemonResponse>());
        });
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task UpdateHP_Valid_UpdatesHealth(bool isGm)
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var trainer = trainers.First(x => !x.IsGM);
        var gm = trainers.First(x => x.IsGM);
        var pokemon = trainer.PokemonTeam.First();
        var request = new UpdatePokemonRequest
        {
            GameMasterId = Guid.Empty,
            GameId = gameId,
            TrainerId = isGm ? gm.TrainerId : trainer.TrainerId,
            PokemonId = pokemon.PokemonId,
            HP = pokemon.PokemonStats.HP / 2
        };

        // act
        var response = await _sut.UpdateHP(string.Empty, request);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<UpdatePokemonResponse>());
        });
    }

    [Test]
    [TestCase(2)]
    [TestCase(-2)]
    public async Task UpdateHP_InvalidHealth_ThrowsOutofRangeException(int multiplier)
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var trainer = trainers.First(x => !x.IsGM);
        var pokemon = trainer.PokemonTeam.First();
        var request = new UpdatePokemonRequest
        {
            GameMasterId = Guid.Empty,
            GameId = gameId,
            TrainerId = trainer.TrainerId,
            PokemonId = pokemon.PokemonId,
            HP = pokemon.PokemonStats.HP * multiplier
        };

        // act
        var exception = Assert.Throws<AggregateException>(_sut.UpdateHP(string.Empty, request).Wait);
        
        // assert
        Assert.That(exception.InnerException, Is.InstanceOf<OutofRangeException>());
    }

    [Test]
    public async Task SwitchForm_Valid_Updates()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var trainer = trainers.First(x => !x.IsGM);
        var pokemon = trainer.PokemonHome.First(x => x.SpeciesName.Equals("Venusaur", StringComparison.InvariantCultureIgnoreCase) && x.Form.Equals("base", StringComparison.InvariantCultureIgnoreCase));
        var request = new UpdatePokemonRequest
        {
            GameMasterId = Guid.Empty,
            GameId = gameId,
            TrainerId = trainer.TrainerId,
            PokemonId = pokemon.PokemonId,
            Form = "Mega"
        };

        // act
        var response = await _sut.SwitchForm(string.Empty, request);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<UpdatePokemonResponse>());
        });
    }

    [Test]
    public async Task SwitchForm_invalid_ThrowsPtaException()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var trainer = trainers.First(x => !x.IsGM);
        var pokemon = trainer.PokemonHome.First(x => x.SpeciesName.Equals("Venusaur", StringComparison.InvariantCultureIgnoreCase) && x.Form.Equals("base", StringComparison.InvariantCultureIgnoreCase));
        var request = new UpdatePokemonRequest
        {
            GameMasterId = Guid.Empty,
            GameId = gameId,
            TrainerId = trainer.TrainerId,
            PokemonId = pokemon.PokemonId,
            Form = "Mega1"
        };

        // act
        var exception = Assert.Throws<AggregateException>(_sut.SwitchForm(string.Empty, request).Wait);
        
        // assert
        Assert.That(exception.InnerException, Is.InstanceOf<PtaException>());
    }

    [Test]
    public async Task MarkPokemonAsEvolvable_Valid_Updates()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var trainer = trainers.First(x => !x.IsGM);
        var gm = trainers.First(x => x.IsGM);
        var pokemon = trainer.PokemonTeam.First(x => x.SpeciesName.Equals("Ivysaur", StringComparison.InvariantCultureIgnoreCase));
        var request = new UpdatePokemonRequest
        {
            GameMasterId = gm.TrainerId,
            GameId = gameId,
            TrainerId = Guid.Empty,
            PokemonId = pokemon.PokemonId,
        };

        // act
        var response = await _sut.MarkPokemonAsEvolvable(string.Empty, request);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<UpdatePokemonResponse>());
        });
    }

    [Test]
    public async Task EvolvePokemonAsync_Valid_Updates()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var trainer = trainers.First(x => !x.IsGM);
        var pokemon = trainer.PokemonHome.First(x => x.SpeciesName.Equals("Bulbasaur", StringComparison.InvariantCultureIgnoreCase));
        var request = new UpdatePokemonRequest
        {
            GameMasterId = Guid.Empty,
            GameId = gameId,
            TrainerId = trainer.TrainerId,
            PokemonId = pokemon.PokemonId,
            EvolvePokemonData = new EvolvePokemonData
            {
                KeptMoves = pokemon.Moves,
                NewMoves = ["Move 2"],
                NextForm = "Ivysaur"
            }
        };

        // act
        var response = await _sut.EvolvePokemonAsync(string.Empty, request);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<UpdatePokemonResponse>());
        });
    }

    [Test]
    public async Task EvolvePokemonAsync_InvalidMove_ThrowsInvalidEvolutionException()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var trainer = trainers.First(x => !x.IsGM);
        var gm = trainers.First(x => x.IsGM);
        var pokemon = trainer.PokemonHome.First(x => x.SpeciesName.Equals("Bulbasaur", StringComparison.InvariantCultureIgnoreCase));
        var request = new UpdatePokemonRequest
        {
            GameMasterId = Guid.Empty,
            GameId = gameId,
            TrainerId = trainer.TrainerId,
            PokemonId = pokemon.PokemonId,
            EvolvePokemonData = new EvolvePokemonData
            {
                KeptMoves = ["Move 2"],
                NewMoves = ["Move 2"],
                NextForm = "Ivysaur"
            }
        };

        // act
        var exception = Assert.Throws<AggregateException>(_sut.EvolvePokemonAsync(string.Empty, request).Wait);
        
        // assert
        Assert.That(exception.InnerException, Is.InstanceOf<InvalidEvolutionException>());
    }

    [Test]
    public async Task EvolvePokemonAsync_NullData_ThrowsInvalidEvolutionException()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var trainer = trainers.First(x => !x.IsGM);
        var gm = trainers.First(x => x.IsGM);
        var pokemon = trainer.PokemonHome.First(x => x.SpeciesName.Equals("Bulbasaur", StringComparison.InvariantCultureIgnoreCase));
        var request = new UpdatePokemonRequest
        {
            GameMasterId = gm.TrainerId,
            GameId = gameId,
            TrainerId = Guid.Empty,
            PokemonId = pokemon.PokemonId,
        };

        // act
        var exception = Assert.Throws<AggregateException>(_sut.EvolvePokemonAsync(string.Empty, request).Wait);
        
        // assert
        Assert.That(exception.InnerException, Is.InstanceOf<InvalidEvolutionException>());
    }

    [Test]
    public async Task UpdateDexItemIsSeen_Adds()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var trainer = trainers.First(x => !x.IsGM);
        var gm = trainers.First(x => x.IsGM);
        var request = new UpdatePokedexRequest
        {
            GameMasterId = gm.TrainerId,
            GameId = gameId,
            TrainerId = trainer.TrainerId,
            PokedexItems = [
                new PokedexItem
                {
                    DexNo = 3,
                    GameId = gameId,
                    IsSeen = true,
                    IsCaught = false,
                    TrainerId = trainer.TrainerId,
                },
                new PokedexItem
                {
                    DexNo = 5,
                    GameId = gameId,
                    IsSeen = true,
                    IsCaught = false,
                    TrainerId = trainer.TrainerId,
                }
            ]
        };

        // act
        var response = await _sut.UpdateDexItemIsSeen(string.Empty, request);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<UpdatePokedexResponse>());
        });
    }

    [Test]
    public async Task UpdateDexItemIsCaught_Adds()
    {
        // arrange
        var gameId = Shared.GameIds.First();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var trainer = trainers.First(x => !x.IsGM);
        var gm = trainers.First(x => x.IsGM);
        var request = new UpdatePokedexRequest
        {
            GameMasterId = gm.TrainerId,
            GameId = gameId,
            TrainerId = trainer.TrainerId,
            PokedexItems = [
                new PokedexItem
                {
                    DexNo = 3,
                    GameId = gameId,
                    IsSeen = true,
                    IsCaught = true,
                    TrainerId = trainer.TrainerId,
                },
                new PokedexItem
                {
                    DexNo = 4,
                    GameId = gameId,
                    IsSeen = true,
                    IsCaught = true,
                    TrainerId = trainer.TrainerId,
                }
            ]
        };

        // act
        var response = await _sut.UpdateDexItemIsCaught(string.Empty, request);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<UpdatePokedexResponse>());
        });
    }

    [Test]
    public async Task DeletePokemon_Valid_Updates()
    {
        // arrange
        var gameId = Shared.GameIds.Last();
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var trainer = trainers.First(x => !x.IsGM);
        var gm = trainers.First(x => x.IsGM);
        var pokemon = trainer.PokemonHome.First();
        var request = new DeletePokemonRequest
        {
            GameMasterId = gm.TrainerId,
            GameId = gameId,
            PokemonId = pokemon.PokemonId
        };

        // act
        var response = await _sut.DeletePokemon(string.Empty, request);
        
        // assert
        Assert.That(response, Is.InstanceOf<OkResult>());
    }
}