using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.DTOs;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.CoreApi.Extensions;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models;
using PokemonTabletopAdventures.Models.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.Controllers.v2;

[ApiController]
[Route("api/v2/pokemon")]
public class SettingsController(
    IUserService userService,
    ITrainerService trainerService,
    IPokemonService pokemonService,
    ISettingService settingService,
    IGameService gameService,
    IDexService dexUtility,
    IEncryptionService encryptionService,
    INpcService npcService,
    IPokedexService pokedexService,
    ILogger<SettingsController> logger) : PtaControllerBase(userService, trainerService, pokemonService, gameService, dexUtility, pokedexService, encryptionService)
{
    private readonly ILogger<SettingsController> _logger = logger;
    private readonly ISettingService _settingService = settingService;
    private readonly INpcService _npcService = npcService;

    private static readonly byte[] Buffer = new byte[36];

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<string>), 200)]
    public IActionResult GetEnvironments()
    {
        return Ok(Enum.GetNames<Environments>().Where(environment => environment != "Default"));
    }

    [HttpGet("{gameId}")]
    [ProducesResponseType(typeof(SettingModel), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetActiveSetting(Guid gameId)
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            await StreamSetting(gameId);
        }

        return BadRequest();
    }

    [HttpGet("{gameId}/{gameMasterId}/all")]
    [ProducesResponseType(typeof(IEnumerable<SettingModel>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetAllSettings(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid gameMasterId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var settings = await _settingService.GetAllSettings(gameId);
        return Ok(settings);
    }

    [HttpPost("{gameId}/{gameMasterId}")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> CreateSetting(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] PostSettingRequest request,
        Guid gameId,
        Guid gameMasterId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var encounter = new SettingModel
        {
            SettingId = Guid.NewGuid(),
            GameId = gameId,
            Name = request.Name,
            Type = request.Type,
            ActiveParticipants = [],
            Environment = [],
            Shops = []
        };

        await _settingService.PostSetting(encounter);
        return Ok();
    }

    [HttpPut("{gameId}/{gameMasterId}/environment")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<ActionResult> SetEnvironment(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid gameMasterId,
        [FromQuery] string[] environments)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var setting = await _settingService.GetActiveSetting(gameId) ?? throw new UnknownEntityException<SettingModel>("GameId", gameId);
        setting.Environment = environments;
        await _settingService.UpdateSetting(setting);
        return Ok();
    }

    [HttpPut("{gameId}/{trainerId}")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> AddToActiveSettingAsync(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] SettingParticipantModel request,
        Guid gameId,
        Guid trainerId)
    {
        await VerifyIdentity(accessToken, sessionAuth, trainerId);
        var encounter = await _settingService.GetActiveSetting(gameId) ?? throw new UnknownEntityException<SettingModel>("GameId", gameId);
        if (encounter.ActiveParticipants.Any(activeParticipant => activeParticipant.ParticipantId == request.ParticipantId))
        {
            return Conflict();
        }
        if (encounter.ActiveParticipants.Any(activeParticipant =>
        {
            if (activeParticipant.Position.X == request.Position.X)
            {
                if (activeParticipant.Position.Y == request.Position.Y)
                {
                    return true;
                }
            }

            return false;
        }))
        {
            return Conflict();
        }
        encounter.ActiveParticipants = encounter.ActiveParticipants.Append(request);
        await _settingService.UpdateSetting(encounter);
        return Ok();
    }

    [HttpPut("{gameId}/{gameMasterId}/{participantId}/remove")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> RemoveFromActiveSetting(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid gameMasterId,
        Guid participantId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        return await RemoveFromParticipants(gameId, participantId);
    }


    [HttpPut("{gameId}/{trainerId}/{pokemonId}/return")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> ReturnToPokeball(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid trainerId,
        Guid pokemonId)
    {
        await VerifyIdentity(accessToken, sessionAuth, trainerId);
        return await RemoveFromParticipants(gameId, pokemonId);
    }

    [HttpPut("{gameId}/{trainerId}/{pokemonId}/catch")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> CatchPokemon(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid trainerId,
        Guid pokemonId,
        [FromQuery] int catchRate,
        [FromQuery] string pokeball,
        [FromQuery] string nickname)
    {
        await VerifyIdentity(accessToken, sessionAuth, trainerId);
        var pokemon = await PokemonService.GetPokemonById(pokemonId);
        var game = await GameService.GetGame(gameId);
        if (pokemon.TrainerId != Guid.Empty)
        {
            throw new PtaException("You cannot catch previously caught pokemon.", "Invalid Catch attempt", HttpStatusCode.BadRequest);
        }

        var encounter = await _settingService.GetActiveSetting(gameId) ?? throw new UnknownEntityException<SettingModel>("GameId", gameId);
        var trainer = await TrainerService.GetTrainerById(trainerId, gameId);
        if (trainer?.IsOnline != true)
        {
            throw new PtaException("Player is not currently online", "Invalid Catch attempt", HttpStatusCode.NotFound);
        }

        if (!Enum.TryParse<Pokeball>(pokeball, true, out var pokebalEnum))
        {
            throw new ItemNotFoundException(pokeball);
        }
        var items = new[]
        {
            new ItemModel
            {
                Name = pokebalEnum.ToString().Replace("_", " "),
                Amount = 1
            }
        };
        await RemoveItemsFromTrainer(trainer, items);

        var pokeballModifier = await GetPokeballModifier(pokemon, trainerId, pokebalEnum, encounter.Environment);
        var random = new Random();
        var check = random.Next(1, 101) + pokeballModifier;
        var log = new LogModel
        (
            user: trainer.TrainerName,
            action: $"failed to catch {pokemon.Nickname}"
        );

        if (check < catchRate)
        {
            encounter.ActiveParticipants = encounter.ActiveParticipants.Where(participant => participant.ParticipantId != pokemonId);
            await _settingService.UpdateSetting(encounter);
            pokemon.Pokeball = pokeball.ToString().Replace("_", " ");
            pokemon.OriginalTrainerId = trainerId;
            pokemon.TrainerId = trainerId;
            var allMons = (await PokemonService.GetPokemonByTrainerId(trainerId, gameId)).Where(pokemon => pokemon.IsOnActiveTeam).Count();
            pokemon.IsOnActiveTeam = allMons < 6;
            if (!string.IsNullOrWhiteSpace(nickname))
            {
                pokemon.Nickname = nickname;
            }
            await PokemonService.UpdatePokemon(pokemon);
            log.Action = $"successfully caught a {pokemon.SpeciesName} named '{pokemon.Nickname}' at {DateTime.UtcNow}";
        }

        await GameService.UpdateGameLogs(game, log);
        return Ok();
    }

    [HttpPut("{gameId}/{gameMasterId}/{participantId}/position")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> UpdatePositionAsync(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] MapPositionModel request,
        Guid gameId,
        Guid gameMasterId,
        Guid participantId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var encounter = await _settingService.GetActiveSetting(gameId) ?? throw new UnknownEntityException<SettingModel>("GameId", gameId);
        if (encounter.ActiveParticipants.Any(activeParticipant =>
        {
            if (activeParticipant.Position.X == request.X)
            {
                if (activeParticipant.Position.Y == request.Y)
                {
                    return true;
                }
            }

            return false;
        }))
        {
            return Conflict();
        }
        var participant = encounter.ActiveParticipants.First(participant => participant.ParticipantId == participantId);
        encounter.ActiveParticipants = encounter.ActiveParticipants.Select(participant =>
        {
            if (participant.ParticipantId == participantId)
            {
                participant.Position = request;
            }

            return participant;
        });

        await _settingService.UpdateSetting(encounter);
        await SendRepositionLog(gameId, participant.Name, request);
        return Ok();
    }

    [HttpPut("{gameId}/{trainerId}/trainer_position")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> UpdateTrainerPositionAsync(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] MapPositionModel request,
        Guid gameId,
        Guid trainerId)
    {
        await VerifyIdentity(accessToken, sessionAuth, trainerId);
        var encounter = await _settingService.GetActiveSetting(gameId) ?? throw new UnknownEntityException<SettingModel>("GameId", gameId);
        var participant = encounter.ActiveParticipants.First(participant => participant.ParticipantId == trainerId);
        if (GetDistance(request, participant.Position) > participant.Speed)
        {
            return StatusCode(411);
        }
        if (encounter.ActiveParticipants.Any(activeParticipant =>
        {
            if (activeParticipant.Position.X == request.X)
            {
                if (activeParticipant.Position.Y == request.Y)
                {
                    return true;
                }
            }

            return false;
        }))
        {
            return Conflict();
        }
        encounter.ActiveParticipants = encounter.ActiveParticipants.Select(participant =>
        {
            if (participant.ParticipantId == trainerId)
            {
                participant.Position = request;
            }

            return participant;
        });

        await _settingService.UpdateSetting(encounter);
        await SendRepositionLog(gameId, participant.Name, request);
        return Ok();
    }

    [HttpPut("{gameId}/{trainerId}/{pokemonId}/pokemon_position")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> UpdateTrainerPokemonPositionAsync(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] MapPositionModel result,
        Guid gameId,
        Guid trainerId,
        Guid pokemonId)
    {
        await VerifyIdentity(accessToken, sessionAuth, trainerId);
        var encounter = await _settingService.GetActiveSetting(gameId) ?? throw new UnknownEntityException<SettingModel>("GameId", gameId);
        var trainer = await TrainerService.GetTrainerById(trainerId, gameId);
        var pokemon = await PokemonService.GetPokemonById(pokemonId);
        if (pokemon.TrainerId != trainerId)
        {
            return Conflict();
        }

        var participant = encounter.ActiveParticipants.First(participant => participant.ParticipantId == pokemonId);
        if (GetDistance(result, participant.Position) > participant.Speed)
        {
            return StatusCode(411);
        }
        if (encounter.ActiveParticipants.Any(activeParticipant =>
        {
            if (activeParticipant.Position.X == result.X)
            {
                if (activeParticipant.Position.Y == result.Y)
                {
                    return true;
                }
            }

            return false;
        }))
        {
            return Conflict();
        }
        encounter.ActiveParticipants = encounter.ActiveParticipants.Select(participant =>
        {
            if (participant.ParticipantId == pokemonId)
            {
                participant.Position = result;
            }

            return participant;
        });

        await _settingService.UpdateSetting(encounter);
        await SendRepositionLog(trainer.GameId, participant.Name, result);
        return Ok();
    }

    [HttpPut("{gameId}/{gameMasterId}/{encounterId}/active")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> SetSettingToActive(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid gameMasterId,
        Guid encounterId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var game = await GameService.GetGame(gameId);
        if (await _settingService.GetActiveSetting(gameId) != null)
        {
            return Conflict();
        }

        var encounter = await _settingService.GetSetting(encounterId);
        if (encounter == null)
        {
            return NotFound(encounterId);
        }

        encounter.IsActive = true;
        await _settingService.UpdateSetting(encounter);

        var newSettingLog = new LogModel(user: "The Game Master", action: $"activated a new encounter ({encounter.Name})");
        await GameService.UpdateGameLogs(game, newSettingLog);
        return Ok();
    }

    [HttpPut("{gameId}/{gameMasterId}/inactive")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> SetSettingToInactive(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid gameMasterId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var encounter = await _settingService.GetActiveSetting(gameId) ?? throw new UnknownEntityException<SettingModel>("GameId", gameId);
        encounter.IsActive = false;
        await _settingService.UpdateSetting(encounter);
        return Ok();
    }

    [HttpPut("{gameId}/{gameMasterId}/hp")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> UpdateParticipantsHp(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid gameMasterId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        var encounter = await _settingService.GetActiveSetting(gameId) ?? throw new UnknownEntityException<SettingModel>("GameId", gameId);
        var participants = await Task.WhenAll(encounter.ActiveParticipants.Select(async participant => await GetWithUpdatedHP(participant, gameId)));
        encounter.ActiveParticipants = participants;
        await _settingService.UpdateSetting(encounter);
        return Ok();
    }

    [HttpDelete("{gameId}/{gameMasterId}/{encounterId}")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> DeleteSetting(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid gameMasterId,
        Guid encounterId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        await _settingService.DeleteSetting(encounterId);
        return Ok();
    }

    [HttpDelete("{gameId}/{gameMasterId}")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> DeleteSettings(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid gameId,
        Guid gameMasterId)
    {
        await IsUserGM(gameMasterId, gameId, accessToken, sessionAuth);
        await _settingService.DeleteSettingsByGameId(gameId);
        return Ok();
    }

    private async Task StreamSetting(Guid gameId)
    {
        using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        var recieved = await RecieveAsync(webSocket);
        while (!recieved.CloseStatus.HasValue)
        {
            await SendAsync(
                webSocket,
                gameId,
                recieved.MessageType,
                recieved.EndOfMessage);

            recieved = await RecieveAsync(webSocket);
        }

        await webSocket.CloseAsync(
            recieved.CloseStatus.Value,
            recieved.CloseStatusDescription,
            CancellationToken.None);
    }

    private async Task<IActionResult> RemoveFromParticipants(Guid gameId, Guid participantId)
    {
        var game = await GameService.GetGame(gameId);
        var encounter = await _settingService.GetActiveSetting(gameId) ?? throw new UnknownEntityException<SettingModel>("GameId", gameId);
        var removedParticipant = encounter.ActiveParticipants.First(participant => participant.ParticipantId == participantId);
        encounter.ActiveParticipants = encounter.ActiveParticipants.Where(participant => participant.ParticipantId != participantId);
        await _settingService.UpdateSetting(encounter);
        var removalLog = new LogModel(
            user: removedParticipant.Name,
            action: $"has been removed from {encounter.Name}");
        await GameService.UpdateGameLogs(game, removalLog);
        return Ok();
    }

    private static async Task<WebSocketReceiveResult> RecieveAsync(WebSocket webSocket)
    {
        return await webSocket.ReceiveAsync(new ArraySegment<byte>(Buffer), CancellationToken.None);
    }

    private async Task SendAsync(
        WebSocket webSocket,
        Guid gameId,
        WebSocketMessageType messageType,
        bool endOfMessage)
    {
        var encounter = await _settingService.GetActiveSetting(gameId) ?? throw new UnknownEntityException<SettingModel>("GameId", gameId);
        var message = JsonSerializer.Serialize(encounter);
        var messageAsBytes = System.Text.Encoding.ASCII.GetBytes(message);
        await webSocket.SendAsync
        (
            new ArraySegment<byte>(messageAsBytes),
            messageType,
            endOfMessage,
            CancellationToken.None
        );
    }

    private async Task<int> GetPokeballModifier(PokemonModel pokemon, Guid trainerId, Pokeball pokeball, string[] environments)
    {
        if (pokemon.CurrentHP < 1 && pokeball != Pokeball.Save_Ball)
        {
            return 100;
        }
        var consideredBasic = 5;
        var consideredGreat = 0;
        var consideredUltra = -5;
        var environment = Environments.Default;
        var types = PokemonTypes.None;
        foreach (var type in pokemon.Type.Split('/'))
        {
            types |= Enum.Parse<PokemonTypes>(type, true);
        }
        if (environments != null)
        {
            foreach (var env in environments)
            {
                environment |= Enum.Parse<Environments>(env, true);
            }
        }

        return pokeball switch
        {
            Pokeball.Park_Ball => environment.HasFlag(Environments.Safari) ? -20 : consideredBasic,
            Pokeball.Cherish_Ball => consideredUltra,
            Pokeball.Premier_Ball => consideredUltra,
            Pokeball.Sport_Ball => environment.HasFlag(Environments.Safari) ? -20 : consideredBasic,
            Pokeball.Heavy_Ball => (pokemon.Weight == Weight.Heavy || pokemon.Weight == Weight.Superweight) ? -15 : consideredBasic,
            Pokeball.Level_Ball => consideredBasic,
            Pokeball.Nest_Ball => consideredBasic,
            Pokeball.Rainforest_Ball => environment.HasFlag(Environments.Rainforest) ? -12 : consideredBasic,
            Pokeball.Great_Ball => consideredGreat,
            Pokeball.Safari_Ball => environment.HasFlag(Environments.Safari) ? -20 : consideredBasic,
            Pokeball.Luxury_Ball => consideredUltra,
            Pokeball.Lure_Ball => environment.HasFlag(Environments.InCombat) ? -10 : consideredBasic,
            Pokeball.Heat_Ball => types.HasFlag(PokemonTypes.Electric) || types.HasFlag(PokemonTypes.Fire) ? -15 : consideredBasic,
            Pokeball.Cave_Ball => environment.HasFlag(Environments.Cave) ? -12 : consideredBasic,
            Pokeball.Earth_Ball => types.HasFlag(PokemonTypes.Grass) || types.HasFlag(PokemonTypes.Ground) ? -15 : consideredBasic,
            Pokeball.Fine_Ball => types.HasFlag(PokemonTypes.Normal) || types.HasFlag(PokemonTypes.Fairy) ? -15 : consideredBasic,
            Pokeball.Taiga_Ball => environment.HasFlag(Environments.Taiga) ? -12 : consideredBasic,
            Pokeball.Save_Ball => pokemon.CurrentHP < 1 ? -10 : consideredBasic,
            Pokeball.Artic_Ball => environment.HasFlag(Environments.Artic) ? -12 : consideredBasic,
            Pokeball.Desert_Ball => environment.HasFlag(Environments.Desert) ? -12 : consideredBasic,
            Pokeball.Haunt_Ball => types.HasFlag(PokemonTypes.Dark) || types.HasFlag(PokemonTypes.Ghost) ? -15 : consideredBasic,
            Pokeball.Urban_Ball => environment.HasFlag(Environments.Urban) ? -12 : consideredBasic,
            Pokeball.Net_Ball => types.HasFlag(PokemonTypes.Water) || types.HasFlag(PokemonTypes.Bug) ? -15 : consideredBasic,
            Pokeball.Freshwater_Ball => environment.HasFlag(Environments.Freshwater) ? -12 : consideredBasic,
            Pokeball.Beach_Ball => environment.HasFlag(Environments.Beach) ? -12 : consideredBasic,
            Pokeball.Timer_Ball => consideredUltra,
            Pokeball.Mystic_Ball => types.HasFlag(PokemonTypes.Dragon) || types.HasFlag(PokemonTypes.Psychic) ? -15 : consideredBasic,
            Pokeball.Air_Ball => types.HasFlag(PokemonTypes.Flying) || types.HasFlag(PokemonTypes.Ice) ? -15 : consideredBasic,
            Pokeball.Fast_Ball => consideredUltra,
            Pokeball.Basic_Ball => consideredUltra,
            Pokeball.Heal_Ball => consideredGreat,
            Pokeball.Master_Ball => -100,
            Pokeball.Tundra_Ball => environment.HasFlag(Environments.Tundra) ? -12 : consideredBasic,
            Pokeball.Friend_Ball => consideredGreat,
            Pokeball.Grassland_Ball => environment.HasFlag(Environments.Grassland) ? -12 : consideredBasic,
            Pokeball.Marsh_Ball => environment.HasFlag(Environments.Marsh) ? -12 : consideredBasic,
            Pokeball.Quick_Ball => consideredBasic,
            Pokeball.Repeat_Ball => (await PokedexService.GetPokedexItem(trainerId, pokemon.GameId, pokemon.DexNo)).IsCaught == true ? -10 : consideredBasic,
            Pokeball.Dream_Ball => Enum.TryParse<Status>(pokemon.PokemonStatus, true, out var result) && result == Status.Asleep ? -10 : consideredBasic,
            Pokeball.Moon_Ball => consideredBasic,
            Pokeball.Dusk_Ball => environment.HasFlag(Environments.NoSunlight) ? -12 : consideredBasic,
            Pokeball.Mold_Ball => types.HasFlag(PokemonTypes.Poison) || types.HasFlag(PokemonTypes.Fighting) ? -15 : consideredBasic,
            Pokeball.Solid_Ball => types.HasFlag(PokemonTypes.Rock) || types.HasFlag(PokemonTypes.Steel) ? -15 : consideredBasic,
            Pokeball.Forest_Ball => environment.HasFlag(Environments.Forest) ? -12 : consideredBasic,
            Pokeball.Love_Ball => consideredBasic,
            Pokeball.Mountain_Ball => environment.HasFlag(Environments.Mountain) ? -12 : consideredBasic,
            _ => 1000
        };
    }

    private async Task<SettingParticipantModel> GetWithUpdatedHP(SettingParticipantModel participant, Guid gameId)
    {
        return participant.Type switch
        {
            SettingParticipantType.Trainer => SettingParticipantModel.FromTrainer(await TrainerService.GetTrainerById(participant.ParticipantId, gameId), participant.Position),
            SettingParticipantType.Pokemon => SettingParticipantModel.FromPokemon(await PokemonService.GetPokemonById(participant.ParticipantId), participant.Position, participant.Type),
            SettingParticipantType.EnemyNpc => SettingParticipantModel.FromNpc(await _npcService.GetNpc(participant.ParticipantId), participant.Position, participant.Type),
            SettingParticipantType.EnemyPokemon => SettingParticipantModel.FromPokemon(await PokemonService.GetPokemonById(participant.ParticipantId), participant.Position, participant.Type),
            SettingParticipantType.NeutralNpc => SettingParticipantModel.FromNpc(await _npcService.GetNpc(participant.ParticipantId), participant.Position, participant.Type),
            SettingParticipantType.NeutralPokemon => SettingParticipantModel.FromPokemon(await PokemonService.GetPokemonById(participant.ParticipantId), participant.Position, participant.Type),
            _ => throw new ArgumentOutOfRangeException(nameof(participant.Type)),
        };
    }

    private async Task SendRepositionLog(Guid gameId, string participantName, MapPositionModel position)
    {
        var game = await GameService.GetGame(gameId);
        var repositionLog = new LogModel
        (
            user: participantName,
            action: $"moved to point ({position.X}, {position.Y})"
        );
        await GameService.UpdateGameLogs(game, repositionLog);
    }

    private static double GetDistance(MapPositionModel start, MapPositionModel end)
    {
        return Math.Sqrt(Math.Pow(start.X - end.X, 2) + Math.Pow(start.Y - end.Y, 2));
    }
}
