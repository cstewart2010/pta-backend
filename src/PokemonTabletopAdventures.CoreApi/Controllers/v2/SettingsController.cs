using Microsoft.AspNetCore.Mvc;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Domain.Handlers;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Enums;
using PokemonTabletopAdventures.Models.Games;
using PokemonTabletopAdventures.Models.Pokemons;
using PokemonTabletopAdventures.Models.Settings;
using PokemonTabletopAdventures.Models.Shops;
using System.Net;
using System.Net.WebSockets;
using System.Text.Json;
using Environments = PokemonTabletopAdventures.Models.Enums.Environments;

namespace PokemonTabletopAdventures.CoreApi.Controllers.v2;

[ApiController]
[Route(Routes.SettingRoute)]
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
    [ProducesResponseType(typeof(IEnumerable<Environments>), 200)]
    public IActionResult GetEnvironments()
    {
        return Ok(Enum.GetValues<Environments>());
    }

    [HttpGet("{gameId}")]
    [ProducesResponseType(typeof(SettingDto), 200)]
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

    [HttpPost("all")]
    [ProducesResponseType(typeof(RetrieveSettingResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetAllSettings(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] RetrieveSettingRequest request)
    {
        await IsUserGM(request.GameMasterId, request.GameId, accessToken, sessionAuth);
        var settings = await _settingService.GetAllSettings(request.GameId);
        return Ok(new RetrieveSettingResponse { Settings = [..settings] });
    }

    [HttpPost("create")]
    [ProducesResponseType(typeof(CreateSettingResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> CreateSetting(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] CreateSettingRequest request)
    {
        await IsUserGM(request.GameMasterId, request.GameId, accessToken, sessionAuth);
        var setting = new Setting
        {
            SettingId = Guid.NewGuid(),
            GameId = request.GameId,
            Name = request.Name,
            Type = request.Type,
            Participants = [],
            Environment = [],
            Shops = [],
            IsActive = false,
        };

        await _settingService.PostSetting(setting);
        return Ok(new CreateSettingResponse { Setting = setting });
    }

    [HttpPut("environment")]
    [ProducesResponseType(typeof(UpdateSettingResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<ActionResult> SetEnvironment(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdateSettingRequest request)
    {
        await IsUserGM(request.TrainerId, request.GameId, accessToken, sessionAuth);
        var setting = await _settingService.GetActiveSetting(request.GameId, true) ?? throw new UnknownEntityException<SettingDto>(PropertyNames.GameId, request.GameId);
        setting.Environment = request.Setting.Environment;
        await _settingService.UpdateSetting(setting, true);
        return Ok();
    }

    [HttpPut("participant/add")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> AddToActiveSettingAsync(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdateSettingRequest request)
    {
        await IsUserGM(request.TrainerId, request.GameId, accessToken, sessionAuth);
        var setting = await _settingService.GetActiveSetting(request.GameId, true) ?? throw new UnknownEntityException<SettingDto>(PropertyNames.GameId, request.GameId);
        var participant = GetSingleParticipant(request);
        if (setting.Participants.Any(activeParticipant => activeParticipant.ParticipantId == participant.ParticipantId))
        {
            return Conflict();
        }
        if (setting.Participants.Any(activeParticipant =>
        {
            if (activeParticipant.Position.X == participant.Position.X)
            {
                if (activeParticipant.Position.Y == participant.Position.Y)
                {
                    return true;
                }
            }

            return false;
        }))
        {
            return Conflict();
        }
        setting.Participants.Add(participant);
        await _settingService.UpdateSetting(setting, true);
        return Ok();
    }

    [HttpPut("participant/remove")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> RemoveFromActiveSetting(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdateSettingRequest request)
    {
        await IsUserGM(request.TrainerId, request.GameId, accessToken, sessionAuth);
        var participant = GetSingleParticipant(request);
        return await RemoveFromParticipants(request.GameId, participant.ParticipantId, true);
    }


    [HttpPut("{pokemonId}/return")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> ReturnToPokeball(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdateSettingRequest request,
        Guid pokemonId)
    {
        await VerifyIdentity(accessToken, sessionAuth, request.TrainerId);
        return await RemoveFromParticipants(request.GameId, pokemonId, true);
    }

    [HttpPut("{pokemonId}/catch")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> CatchPokemon(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdateSettingRequest request,
        Guid pokemonId,
        [FromQuery] int catchRate,
        [FromQuery] string pokeball,
        [FromQuery] string nickname)
    {
        await VerifyIdentity(accessToken, sessionAuth, request.TrainerId);
        var pokemon = await PokemonService.GetPokemonById(pokemonId);
        var game = await GameService.GetGame(request.GameId, false);
        if (pokemon.TrainerId != Guid.Empty)
        {
            throw new InvalidCatchException(PtaExceptionParts.AlreadyCaughtPokemonMessage, HttpStatusCode.BadRequest);
        }

        var setting = await _settingService.GetActiveSetting(request.GameId, true) ?? throw new UnknownEntityException<SettingDto>(PropertyNames.GameId, request.GameId);
        var trainer = await TrainerService.GetTrainerById(request.TrainerId, request.GameId);
        if (trainer?.IsOnline != true)
        {
            throw new InvalidCatchException(PtaExceptionParts.PlayNotOnlineMessage, HttpStatusCode.NotFound);
        }

        if (!Enum.TryParse<Pokeball>(pokeball, true, out var pokebalEnum))
        {
            throw new ItemNotFoundException(pokeball);
        }
        var items = new[]
        {
            new Item
            {
                Name = pokebalEnum.ToString().Replace("_", " "),
                Amount = 1
            }
        };
        await RemoveItemsFromTrainer(trainer, items);

        var pokeballModifier = await GetPokeballModifier(pokemon, request.TrainerId, pokebalEnum, setting.Environment);
        var random = new Random();
        var check = random.Next(1, 101) + pokeballModifier;
        var log = new Log
        {
            User = trainer.TrainerName,
            Action = $"failed to catch {pokemon.Nickname}",
            LogTimestamp = DateTimeOffset.Now
        };

        if (check < catchRate)
        {
            setting.Participants = [..setting.Participants.Where(participant => participant.ParticipantId != pokemonId)];
            await _settingService.UpdateSetting(setting, false);
            pokemon.Pokeball = pokeball.ToString().Replace("_", " ");
            pokemon.OriginalTrainerId = request.TrainerId;
            pokemon.TrainerId = request.TrainerId;
            var allMons = (await PokemonService.GetPokemonByTrainerId(request.TrainerId, request.GameId)).Where(pokemon => pokemon.IsOnActiveTeam).Count();
            pokemon.IsOnActiveTeam = allMons < 6;
            if (!string.IsNullOrWhiteSpace(nickname))
            {
                pokemon.Nickname = nickname;
            }
            await PokemonService.UpdatePokemon(pokemon);
            log.Action = $"successfully caught a {pokemon.SpeciesName} named '{pokemon.Nickname}' at {DateTime.UtcNow}";
        }

        await GameService.UpdateGameLogs(game, false, log);
        return Ok();
    }

    [HttpPut("position/gm")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> UpdatePositionAsync(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdateSettingRequest request)
    {
        await IsUserGM(request.TrainerId, request.GameId, accessToken, sessionAuth);
        var setting = await _settingService.GetActiveSetting(request.GameId, true) ?? throw new UnknownEntityException<SettingDto>(PropertyNames.GameId, request.GameId);
        var participant = GetSingleParticipant(request);
        if (setting.Participants.Any(activeParticipant =>
        {
            if (activeParticipant.Position.X == participant.Position.X)
            {
                if (activeParticipant.Position.Y == participant.Position.Y)
                {
                    return true;
                }
            }

            return false;
        }))
        {
            return Conflict();
        }
        setting.Participants = [..setting.Participants.Select(x =>
        {
            if (x.ParticipantId == participant.ParticipantId)
            {
                x.Position = participant.Position;
            }

            return x;
        })];

        await _settingService.UpdateSetting(setting, true);
        await SendRepositionLog(request.GameId, participant.Name, participant.Position);
        return Ok();
    }

    [HttpPut("position")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> UpdateTrainerPositionAsync(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdateSettingRequest request)
    {
        await VerifyIdentity(accessToken, sessionAuth, request.TrainerId);
        var setting = await _settingService.GetActiveSetting(request.GameId, true) ?? throw new UnknownEntityException<SettingDto>(PropertyNames.GameId, request.GameId);
        var currentParticipant = GetSingleParticipant(request);
        var participant = setting.Participants.First(participant => participant.ParticipantId == request.TrainerId);
        if (GetDistance(currentParticipant.Position, participant.Position) > participant.Speed)
        {
            return StatusCode(411);
        }
        if (setting.Participants.Any(activeParticipant =>
        {
            if (activeParticipant.Position.X == currentParticipant.Position.X)
            {
                if (activeParticipant.Position.Y == currentParticipant.Position.Y)
                {
                    return true;
                }
            }

            return false;
        }))
        {
            return Conflict();
        }
        setting.Participants = [..setting.Participants.Select(participant =>
        {
            if (participant.ParticipantId == request.TrainerId)
            {
                participant.Position = currentParticipant.Position;
            }

            return participant;
        })];

        await _settingService.UpdateSetting(setting, false);
        await SendRepositionLog(request.GameId, currentParticipant.Name, currentParticipant.Position);
        return Ok();
    }

    [HttpPut("{pokemonId}/position")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> UpdateTrainerPokemonPositionAsync(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdateSettingRequest request,
        Guid pokemonId)
    {
        await VerifyIdentity(accessToken, sessionAuth, request.TrainerId);
        var setting = await _settingService.GetActiveSetting(request.GameId, true) ?? throw new UnknownEntityException<SettingDto>(PropertyNames.GameId, request.GameId);
        var trainer = await TrainerService.GetTrainerById(request.TrainerId, request.GameId);
        var pokemon = await PokemonService.GetPokemonById(pokemonId);
        if (pokemon.TrainerId != request.TrainerId)
        {
            throw new InvalidPokemonException($"{request.TrainerId} cannot move this pokemon");
        }

        var currentParticipant = GetSingleParticipant(request);
        var participant = setting.Participants.First(participant => participant.ParticipantId == pokemonId);
        if (GetDistance(currentParticipant.Position, participant.Position) > participant.Speed)
        {
            return StatusCode(411);
        }
        if (setting.Participants.Any(activeParticipant =>
        {
            if (activeParticipant.Position.X == currentParticipant.Position.X)
            {
                if (activeParticipant.Position.Y == currentParticipant.Position.Y)
                {
                    return true;
                }
            }

            return false;
        }))
        {
            return Conflict();
        }
        setting.Participants = [..setting.Participants.Select(participant =>
        {
            if (participant.ParticipantId == pokemonId)
            {
                participant.Position = currentParticipant.Position;
            }

            return participant;
        })];

        await _settingService.UpdateSetting(setting, false);
        await SendRepositionLog(trainer.GameId, participant.Name, currentParticipant.Position);
        return Ok();
    }

    [HttpPut("activate")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> SetSettingToActive(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdateSettingRequest request)
    {
        await IsUserGM(request.TrainerId, request.GameId, accessToken, sessionAuth);
        var game = await GameService.GetGame(request.GameId, true);
        if (await _settingService.GetActiveSetting(request.GameId, true) != null)
        {
            return Conflict();
        }

        var setting = await _settingService.GetSetting(request.Setting.SettingId, true) ?? throw new UnknownEntityException<Setting>(PropertyNames.SettingId, request.Setting.SettingId);
        setting.IsActive = true;
        await _settingService.UpdateSetting(setting, true);

        var gm = await TrainerService.GetTrainerById(request.TrainerId, request.GameId);
        var newSettingLog = new LogDto(user: gm.TrainerName, action: $"activated a new encounter ({setting.Name})");
        await GameService.UpdateGameLogs(game, true, DtoHandler.ParseFromDto(newSettingLog));
        return Ok();
    }

    [HttpPut("deactivate")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> SetSettingToInactive(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdateSettingRequest request)
    {
        await IsUserGM(request.TrainerId, request.GameId, accessToken, sessionAuth);
        var setting = await _settingService.GetActiveSetting(request.GameId, true) ?? throw new UnknownEntityException<SettingDto>(PropertyNames.GameId, request.GameId);
        setting.IsActive = false;
        await _settingService.UpdateSetting(setting, true);
        return Ok();
    }

    [HttpPut("hp")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> UpdateParticipantsHp(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] UpdateSettingRequest request)
    {
        await IsUserGM(request.TrainerId, request.GameId, accessToken, sessionAuth);
        var setting = await _settingService.GetActiveSetting(request.GameId, true) ?? throw new UnknownEntityException<SettingDto>(PropertyNames.GameId, request.GameId);
        var participants = await Task.WhenAll(setting.Participants.Select(async participant => await GetWithUpdatedHP(participant, request.GameId)));
        setting.Participants = participants;
        var updatedSetting = await _settingService.UpdateSetting(setting, true);
        return Ok(new UpdateSettingResponse { Setting = updatedSetting});
    }

    [HttpDelete("delete")]
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

    [HttpDelete("delete/all")]
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

    private async Task<IActionResult> RemoveFromParticipants(Guid gameId, Guid participantId, bool isGm)
    {
        var game = await GameService.GetGame(gameId, isGm);
        var setting = await _settingService.GetActiveSetting(gameId, isGm) ?? throw new UnknownEntityException<SettingDto>(PropertyNames.GameId, gameId);
        var removedParticipant = setting.Participants.First(participant => participant.ParticipantId == participantId);
        setting.Participants = [..setting.Participants.Where(participant => participant.ParticipantId != participantId)];
        await _settingService.UpdateSetting(setting, isGm);
        var removalLog = new LogDto(
            user: removedParticipant.Name,
            action: $"has been removed from {setting.Name}");
        await GameService.UpdateGameLogs(game, isGm, DtoHandler.ParseFromDto(removalLog));
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
        var setting = await _settingService.GetActiveSetting(gameId, true) ?? throw new UnknownEntityException<SettingDto>(PropertyNames.GameId, gameId);
        var message = JsonSerializer.Serialize(setting);
        var messageAsBytes = System.Text.Encoding.ASCII.GetBytes(message);
        await webSocket.SendAsync
        (
            new ArraySegment<byte>(messageAsBytes),
            messageType,
            endOfMessage,
            CancellationToken.None
        );
    }

    private async Task<int> GetPokeballModifier(Pokemon pokemon, Guid trainerId, Pokeball pokeball, string[] environments)
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
            Pokeball.Dream_Ball => pokemon.PokemonStatus == Status.Asleep ? -10 : consideredBasic,
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

    private async Task<SettingParticipant> GetWithUpdatedHP(SettingParticipant participant, Guid gameId)
    {
        var dto =  participant.Type switch
        {
            SettingParticipantType.Trainer => SettingParticipantModel.FromTrainer(DtoHandler.ParseFromModel(await TrainerService.GetTrainerById(participant.ParticipantId, gameId)), participant.Position),
            SettingParticipantType.Pokemon => SettingParticipantModel.FromPokemon(DtoHandler.ParseFromModel(await PokemonService.GetPokemonById(participant.ParticipantId)), participant.Position, participant.Type),
            SettingParticipantType.EnemyNpc => SettingParticipantModel.FromNpc(DtoHandler.ParseFromModel(await _npcService.GetNpc(participant.ParticipantId)), participant.Position, participant.Type),
            SettingParticipantType.EnemyPokemon => SettingParticipantModel.FromPokemon(DtoHandler.ParseFromModel(await PokemonService.GetPokemonById(participant.ParticipantId)), participant.Position, participant.Type),
            SettingParticipantType.NeutralNpc => SettingParticipantModel.FromNpc(DtoHandler.ParseFromModel(await _npcService.GetNpc(participant.ParticipantId)), participant.Position, participant.Type),
            SettingParticipantType.NeutralPokemon => SettingParticipantModel.FromPokemon(DtoHandler.ParseFromModel(await PokemonService.GetPokemonById(participant.ParticipantId)), participant.Position, participant.Type),
            _ => throw new ArgumentOutOfRangeException(nameof(participant.Type)),
        };

        return DtoHandler.ParseFromDto(dto);
    }

    private async Task SendRepositionLog(Guid gameId, string participantName, MapPosition position)
    {
        var game = await GameService.GetGame(gameId, false);
        var repositionLog = new Log
        {
            User = participantName,
            Action = $"moved to point ({position.X}, {position.Y})",
            LogTimestamp = DateTimeOffset.Now
        };
        await GameService.UpdateGameLogs(game, false, repositionLog);
    }

    private static SettingParticipant GetSingleParticipant(UpdateSettingRequest request)
    {
        return request.Setting.Participants.SingleOrDefault() ?? throw new InvalidSettingException(PtaExceptionParts.TooManyParticipantsMessage);
    }

    private static double GetDistance(MapPosition start, MapPosition end)
    {
        return Math.Sqrt(Math.Pow(start.X - end.X, 2) + Math.Pow(start.Y - end.Y, 2));
    }
}
