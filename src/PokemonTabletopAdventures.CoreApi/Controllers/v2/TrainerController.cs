using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.DTOs;
using PokemonTabletopAdventures.CoreApi.DTOs.Enums;
using PokemonTabletopAdventures.CoreApi.DTOs.Pokemons;
using PokemonTabletopAdventures.CoreApi.DTOs.Trainers;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models;
using PokemonTabletopAdventures.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.Controllers.v2;

[ApiController]
[Route(Routes.TrainerRoute)]
public class TrainerController(
    IUserService userService,
    ITrainerService trainerService,
    IPokemonService pokemonService,
    IGameService gameService,
    IDexService dexUtility,
    IPokedexService pokedexService,
    IEncryptionService encryptionService,
    ILogger<TrainerController> logger) : PtaControllerBase(userService, trainerService, pokemonService, gameService, dexUtility, pokedexService, encryptionService)
{
    private readonly ILogger<TrainerController> _logger = logger;

    [HttpGet("{gameId}/trainers")]
    [ProducesResponseType(typeof(IEnumerable<Trainer>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> FindTrainers(Guid gameId)
    {
        var trainer = await GetTrainers(gameId);
        return Ok(trainer);
    }

    [HttpGet("trainers/{trainerName}")]
    [ProducesResponseType(typeof(FoundTrainerResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> FindTrainer(string trainerName, [FromQuery] Guid gameId)
    {
        var trainer = await TrainerService.GetTrainerByUsername(trainerName, gameId);
        var user = await UserService.GetUserById(trainer.TrainerId);
        return Ok(await FoundTrainerResponse.ParseFromModel(trainer, user, PokemonService, PokedexService, GameService));
    }

    [HttpGet("{gameId}/trainer/{trainerId}", Name = nameof(GetTrainerInGame))]
    [ProducesResponseType(typeof(FoundTrainerResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetTrainerInGame(Guid gameId, Guid trainerId)
    {
        var trainer = await TrainerService.GetTrainerById(trainerId, gameId);
        var user = await UserService.GetUserById(trainer.TrainerId);
        return Ok(await FoundTrainerResponse.ParseFromModel(trainer, user, PokemonService, PokedexService, GameService));
    }

    [HttpGet("{gameId}/{trainerId}/{pokemonId}")]
    [ProducesResponseType(typeof(PokemonModel), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> FindTrainerMon(
        Guid gameId,
        Guid trainerId,
        Guid pokemonId)
    {
        var pokemon = await PokemonService.GetPokemonById(pokemonId);
        if (pokemon.TrainerId != trainerId)
        {
            throw new InvalidPokemonException($"This pokemon is not associate with trainer {trainerId}");
        }
        if (pokemon.GameId != gameId)
        {
            throw new InvalidPokemonException($"This pokemon is not associate with game {gameId}");
        }

        return Ok(pokemon);
    }

    [HttpPost("create")]
    public async Task<IActionResult> PostNewPlayer(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth)
    {
        return await Task.FromResult(NotFound());
    }

    [HttpPost("{gameId}/{gameMasterId}/{trainerId}")]
    [ProducesResponseType(typeof(PokemonModel), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> AddPokemon(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid gameMasterId,
        Guid trainerId,
        [FromQuery] WildPokemon wild)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var pokemon = await BuildPokemon(trainerId, gameId, wild);
        await PokemonService.PostPokemon(pokemon);
        await RefreshToken(gameMasterId);
        return Ok(pokemon);
    }

    [HttpPut("{gameId}/{userId}/newUser")]
    [ProducesResponseType(typeof(FoundTrainerResponse), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> AddPlayerToGame(
        Guid gameId,
        Guid tempId,
        Guid userId,
        [FromQuery] string username)
    {
        if (!await GameService.HasGM(gameId))
        {
            return BadRequest();
        }

        //var trainer = await BuildTrainer(gameId, userId, username);
        var trainer = await TrainerService.GetTrainerById(userId, tempId);
        trainer.GameId = gameId;
        await TrainerService.UpdateTrainer(trainer);
        
        await RefreshToken(userId);
        var user = await UserService.GetUserById(trainer.TrainerId);
        return Ok(await FoundTrainerResponse.ParseFromModel(trainer, user, PokemonService, PokedexService, GameService));
    }

    [HttpPut("{gameId}/{gameMasterId}/{trainerId}/allow")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> AllowUser(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid gameMasterId,
        Guid trainerId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var trainer = await TrainerService.GetTrainerById(trainerId, gameId);
        var game = await GameService.GetGame(gameId);
        trainer.IsAllowed = true;
        await TrainerService.UpdateTrainer(trainer);
        var log = new LogModel
        (
            user: trainer.TrainerName,
            action: GameLogMessages.JoinedTheGameLog
        );
        await GameService.UpdateGameLogs(game, log);
        await RefreshToken(gameMasterId);
        return Ok();
    }

    [HttpPut("{gameId}/{gameMasterId}/{trainerId}/disallow")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> DisallowUser(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid gameMasterId,
        Guid trainerId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var trainer = await TrainerService.GetTrainerById(trainerId, gameId);
        var game = await GameService.GetGame(gameId);
        trainer.IsAllowed = false;
        await TrainerService.UpdateTrainer(trainer);
        var log = new LogModel
        (
            user: trainer.TrainerName,
            action: GameLogMessages.RemovedFromGameLog
        );
        await GameService.UpdateGameLogs(game, log);
        await RefreshToken(gameMasterId);
        return Ok();
    }

    [HttpPut("{gameId}/{trainerId}/addStats")]
    [ProducesResponseType(typeof(FoundTrainerResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> AddTrainerStats(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] Trainer trainerDto,
        Guid gameId,
        Guid trainerId)
    {
        await VerifyIdentity(accessToken, sessionAuth, trainerId);
        await CompleteTrainer(trainerId, gameId, trainerDto);
        var trainer = await TrainerService.GetTrainerById(trainerId, gameId);
        var user = await UserService.GetUserById(trainerId);
        return Ok(await FoundTrainerResponse.ParseFromModel(trainer, user, PokemonService, PokedexService, GameService));
    }

    [HttpPut("{gameId}/{gameMasterId}/groupHonor")]
    [ProducesResponseType(typeof(GenericResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> AddGroupHonor(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] string honor,
        Guid gameId,
        Guid gameMasterId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var game = await GameService.GetGame(gameId);
        if (string.IsNullOrEmpty(honor))
        {
            return BadRequest(nameof(honor));
        }
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        foreach (var trainer in trainers)
        {
            await TrainerService.UpdateTrainerHonors(trainer.TrainerId, trainer.GameId, trainer.Honors.Append(honor));
        }

        var updatedHonorsLog = new LogModel
        (
            user: GameLogMessages.PartyUser,
            action: $"has earned a new honor: {honor}"
        );

        await GameService.UpdateGameLogs(game, updatedHonorsLog);
        await RefreshToken(gameMasterId);
        return Ok(new GenericResponse($"Granted the party honor: {honor}"));
    }

    [HttpPut("{gameId}/{gameMasterId}/honor")]
    [ProducesResponseType(typeof(GenericResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> AddSingleHonor(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        PutSingleHonorRequest request,
        Guid gameId,
        Guid gameMasterId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var trainer = await TrainerService.GetTrainerById(request.TrainerId, gameId);
        var gm = await TrainerService.GetTrainerById(gameMasterId, gameId);
        var game = await GameService.GetGame(gameId);
        await TrainerService.UpdateTrainerHonors(trainer.TrainerId, trainer.GameId, trainer.Honors.Append(request.Honor));
        var updatedHonorsLog = new LogModel(
            user: gm.TrainerName,
            action: $"has granted {trainer.TrainerName} a new honor");
        await GameService.UpdateGameLogs(game, updatedHonorsLog);
        await RefreshToken(gameMasterId);
        return Ok(new GenericResponse($"Granted {gameMasterId} honor: {request.Honor}"));
    }

    [HttpPut("{gameId}/{gameMasterId}/{trainerId}/addItems")]
    [ProducesResponseType(typeof(FoundTrainerResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> AddItemsToTrainerAsync(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] IEnumerable<ItemModel> items,
        Guid gameId,
        Guid gameMasterId,
        Guid trainerId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var trainer = await TrainerService.GetTrainerById(trainerId, gameId);
        var game = await GameService.GetGame(gameId);
        var addedItemsLogs = await AddItemsToTrainer(trainer, items);
        await GameService.UpdateGameLogs(game, [.. addedItemsLogs]);
        await RefreshToken(gameMasterId);
        var user = await UserService.GetUserById(trainerId);
        return Ok(await FoundTrainerResponse.ParseFromModel(trainer, user, PokemonService, PokedexService, GameService));
    }

    [HttpPut("{gameId}/{gameMasterId}/addItems/all")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> AddItemsToAllTrainersAsync(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] IEnumerable<ItemModel> items,
        Guid gameId,
        Guid gameMasterId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        var game = await GameService.GetGame(gameId);
        if (trainers.Any())
        {
            foreach (var trainer in trainers)
            {
                var addedItemsLogs = await AddItemsToTrainer(trainer, items);
                await GameService.UpdateGameLogs(game, [.. addedItemsLogs]);
            }
        }

        await RefreshToken(gameMasterId);
        return Ok();
    }

    [HttpPut("{gameId}/{trainerId}/removeItems")]
    [ProducesResponseType(typeof(FoundTrainerResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> RemoveItemsFromTrainerAsync(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] IEnumerable<ItemModel> items,
        Guid gameId,
        Guid trainerId)
    {
        await VerifyIdentity(accessToken, sessionAuth, trainerId);
        var trainer = await TrainerService.GetTrainerById(trainerId, gameId);
        var game = await GameService.GetGame(gameId);
        var removedItemsLogs = await RemoveItemsFromTrainer(trainer, items);
        await GameService.UpdateGameLogs(game, [.. removedItemsLogs]);
        await RefreshToken(trainerId);
        var user = await UserService.GetUserById(trainerId);
        return Ok(await FoundTrainerResponse.ParseFromModel(trainer, user, PokemonService, PokedexService, GameService));
    }

    [HttpPut("{gameId}/{gameMasterId}/{trainerId}/removeItems")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> RemoveItemsFromTrainerGMAsync(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] IEnumerable<ItemModel> items,
        Guid gameId,
        Guid gameMasterId,
        Guid trainerId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var game = await GameService.GetGame(gameId);
        var trainer = await TrainerService.GetTrainerById(trainerId, gameId);
        var removedItemsLogs = await RemoveItemsFromTrainer(trainer, items);
        await GameService.UpdateGameLogs(game, [.. removedItemsLogs]);
        await RefreshToken(gameMasterId);
        var user = await UserService.GetUserById(trainerId);
        return Ok(await FoundTrainerResponse.ParseFromModel(trainer, user, PokemonService, PokedexService, GameService));
    }

    [HttpPut("{gameId}/{gameMasterId}/removeItems/all")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> RemoveItemsFromAllTrainersAsync(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] IEnumerable<ItemModel> items,
        Guid gameId, 
        Guid gameMasterId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var game = await GameService.GetGame(gameId);
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        if (trainers.Any())
        {
            foreach (var trainer in trainers)
            {
                var removedItemsLogs = await RemoveItemsFromTrainer(trainer, items);
                await GameService.UpdateGameLogs(game, [.. removedItemsLogs]);
            }
        }

        await RefreshToken(gameMasterId);
        return Ok();
    }

    [HttpPut("{gameId}/{gameMasterId}/{trainerId}/money")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> UpdateTrainerMoney(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid gameMasterId,
        Guid trainerId,
        [FromQuery] int addition)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var trainer = await TrainerService.GetTrainerById(trainerId, gameId);
        trainer.Money += addition;
        await TrainerService.UpdateTrainer(trainer);
        return Ok();
    }

    [HttpPut("{gameId}/{gameMasterId}/money/all")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> UpdateAllTrainersMoney(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid gameMasterId,
        [FromQuery] int addition)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var trainers = await TrainerService.GetTrainersByGameId(gameId);
        foreach (var trainer in trainers.Where(trainer => !trainer.IsGM))
        {
            trainer.Money += addition;
            await TrainerService.UpdateTrainer(trainer);
        }

        return Ok();
    }

    [HttpDelete("{gameId}/{gameMasterId}/{trainerId}")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> DeleteTrainer(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid trainerId,
        Guid gameMasterId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var game = await GameService.GetGame(gameId);
        foreach (var pokemon in await PokemonService.GetPokemonByTrainerId(trainerId))
        {
            await PokemonService.DeletePokemon(pokemon.PokemonId);
        }
        await TrainerService.DeleteTrainer(gameId, trainerId);
        if (await TrainerService.GetTrainerById(trainerId, gameId) == null)
        {
            throw new DeletionException($"Failed to delete trainer {trainerId}");
        }

        var gm = await TrainerService.GetTrainerById(gameMasterId, gameId);
        var deleteTrainerLog = new LogModel(
            user: gm.TrainerName,
            action: $"removed a trainer and all of their pokemon from the game");
        await GameService.UpdateGameLogs(game, deleteTrainerLog);
        await RefreshToken(gameMasterId);
        return Ok();
    }

    private async Task<TrainerModel> BuildTrainer(
        Guid gameId,
        Guid userId,
        string username)
    {
        var trainer = await BuildTrainer(
            gameId,
            userId,
            username,
            false);

        trainer.Sprite = Sprites.AceTrainer;
        return trainer;
    }

    private async Task CompleteTrainer(
        Guid trainerId,
        Guid gameId,
        Trainer trainerDto)
    {
        var user = await UserService.GetUserById(trainerId);
        var trainer = await TrainerService.GetTrainerById(trainerId, gameId);
        if (CheckRequestingTrainer(trainerDto, trainer, user))
        {
            await AddTrainerPokemon(trainerDto.NewPokemon, trainer);
            await SetStartingEquipmentWithOrigin(trainer);
            await TrainerService.UpdateTrainer(trainer);
            var game = await GameService.GetGame(trainer.GameId);
            var statsAddedLog = new LogModel(trainer.TrainerName, GameLogMessages.UpdatedStatsLog);
            await GameService.UpdateGameLogs(game, statsAddedLog);
        }
    }

    private async Task AddTrainerPokemon(
        IEnumerable<NewPokemon> pokemon,
        TrainerModel trainer)
    {
        foreach (var data in pokemon.Where(data => data != null))
        {
            var nickname = data.Nickname.Length > 18 ? data.Nickname[..18] : data.Nickname;
            var pokemonModel = await DexService.GetNewPokemon(data.SpeciesName, nickname, data.Form);
            pokemonModel.IsOnActiveTeam = data.IsOnActiveTeam;
            pokemonModel.OriginalTrainerId = trainer.TrainerId;
            pokemonModel.TrainerId = trainer.TrainerId;
            pokemonModel.GameId = trainer.GameId;
            pokemonModel.Pokeball = Pokeball.Basic_Ball.ToString().Replace("_", "");
            await PokemonService.PostPokemon(pokemonModel);
            var game = await GameService.GetGame(trainer.GameId);
            var caughtPokemonLog = new LogModel(trainer.TrainerName, $"caught a {pokemonModel.SpeciesName} named {pokemonModel.Nickname}");
            await GameService.UpdateGameLogs(game, caughtPokemonLog);
            if (await PokedexService.GetPokedexItem(trainer.TrainerId, trainer.GameId, pokemonModel.DexNo) == null)
            {
                await PokedexService.PostDexItem(trainer.TrainerId, trainer.GameId, pokemonModel.DexNo, true, true);
            }
            else
            {
                await PokedexService.UpdateDexItemIsCaught(trainer.TrainerId, trainer.GameId, pokemonModel.DexNo);
            }
        }
    }

    private async Task SetStartingEquipmentWithOrigin(TrainerModel trainer)
    {
        var origin = await DexService.GetDexEntry<OriginModel>(DexType.Origins, trainer.Origin);
        var collection = await Task.WhenAll(origin.Data.StartingEquipmentList.Select(ConvertStartingEquipment));
        trainer.Items = [.. collection];
    }

    private async Task<List<Trainer>> GetTrainers(Guid gameId)
    {
        var models = await TrainerService.GetTrainersByGameId(gameId);
        var trainers = await Task.WhenAll(models.Select(async trainer => await Trainer.ParseFromModel(trainer, PokemonService, PokedexService)));
        return [.. trainers];
    }

    private async Task<ItemModel> ConvertStartingEquipment(StartingEquipment s)
    {
        var baseItem = s.Type switch
        {
            StartingEquipmentType.Trainer => await DexService.GetDexEntry<BaseItemModel>(DexType.TrainerEquipment, s.Name),
            StartingEquipmentType.Pokeball => await DexService.GetDexEntry<BaseItemModel>(DexType.Pokeballs, s.Name),
            StartingEquipmentType.Medical => await DexService.GetDexEntry<BaseItemModel>(DexType.MedicalItems, s.Name),
            StartingEquipmentType.Berry => await DexService.GetDexEntry<BaseItemModel>(DexType.Berries, s.Name),
            StartingEquipmentType.Pokemon => await DexService.GetDexEntry<BaseItemModel>(DexType.PokemonItems, s.Name),
            _ => throw new ArgumentOutOfRangeException(nameof(s.Type)),
        };
        var item = new ItemModel
        {
            Name = baseItem.Data.Name,
            Effects = baseItem.Data.Effects,
            Amount = s.Amount,
            Type = s.Type
        };
        return item;
    }

    #region Helper methods
    private static bool CheckRequestingTrainer(
        Trainer trainerDto,
        TrainerModel requestingTrainer,
        UserModel user)
    {
        if (user.SiteRole == UserRoleOnSite.SiteAdmin)
        {
            return true;
        }

        if (trainerDto.GameId != requestingTrainer.GameId)
        {
            return false;
        }

        return trainerDto.TrainerId == requestingTrainer.TrainerId || requestingTrainer.IsGM;
    }
    #endregion
}
