using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Domain.Handlers;
using PokemonTabletopAdventures.CoreApi.Domain.Models;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Games;
using System.Text.Json;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.Domain;

/// <summary>
/// Provides a collection of methods to handle import/exports of game sessions
/// </summary>
internal class ExportService(
    IPokemonService pokemonService,
    ITrainerService trainerService,
    IGameService gameService,
    INpcService npcService,
    ISettingService settingService,
    IShopService shopService,
    IPokedexService pokedexService) : IExportService
{
    private readonly IPokemonService _pokemonService = pokemonService;
    private readonly ITrainerService _trainerService = trainerService;
    private readonly IGameService _gameService = gameService;
    private readonly INpcService _npcService = npcService;
    private readonly ISettingService _settingService = settingService;
    private readonly IShopService _shopService = shopService;
    private readonly IPokedexService _pokedexService = pokedexService;

    /// <summary>
    /// Returns a file stream for the a json file of the game session
    /// </summary>
    /// <param name="game">The game session to export</param>
    public async Task<FileStream> GetExportStream(Game model)
    {
        var game = DtoHandler.ParseFromModel(model);
        var (path, json) = await GetStreamParts(game);
        using var writer = new StreamWriter(path);
        writer.Write(json);
        writer.Close();
        return await Task.FromResult(new FileStream(path, FileMode.Open));
    }

    /// <summary>
    /// Return true if the game session was successfully imported
    /// </summary>
    /// <param name="json">The stringified json object to parse</param>
    /// <param name="errors">The errors found while attempting to import the game session</param>
    /// <returns></returns>
    public async Task<Game> ParseImport(string json)
    {
        var import = GetParsedImportFromExport(json);
        return await CleanyAddGame(import!);
    }

    private async Task<Game> CleanyAddGame(ExportedGame import)
    {
        await AddGame(import);
        var gameId = import.GameSession.GameId;
        foreach (var trainer in import.Trainers)
        {
            await CleanlyAddTrainer(trainer, gameId);
        }

        return await _gameService.GetGame(gameId, true);
    }

    private async Task<List<string>> AddGame(ExportedGame import)
    {
        var game = import.GameSession;
        var errors = new List<string>();
        if (import.Trainers.SingleOrDefault(import => import.Trainer.IsGM) == null)
        {
            errors.Add($"Exactly one gm should be listed for game {game.GameId}");
        }

        if (_gameService.GetGame(game.GameId, true) != null)
        {
            errors.Add($"Found extant game session found with id {game.GameId}");
        }

        game.Logs ??= [];
        game.Logs.Add(new LogDto(
            user: GameLogMessages.ImportTool,
            action: $"Recreated game {game.GameId}"));
        var model = await  DtoHandler.ParseFromDto(game, true, _npcService, _settingService, _trainerService);
        await _gameService.PostGame(model, "");
        return errors;
    }

    private async Task CleanlyAddTrainer(
        ExportedTrainer import,
        Guid gameId)
    {
        var errors = new List<string>();
        var trainer = import.Trainer;
        if (!(trainer.GameId == gameId))
        {
            errors.Add($"Failed to import trainer {trainer.TrainerId}");
        }

        var model = await DtoHandler.ParseFromDto(import.Trainer, _pokemonService, _pokedexService);
        await _trainerService.PostTrainer(model);
        errors = [.. import.Pokemon
            .Select(pokemon => AddPokemon(pokemon, trainer.TrainerId))
            .Where(error => !string.IsNullOrEmpty(error))];

        if (errors.Count > 0)
        {
            throw new ImportFailedException(errors);
        }
    }

    private string AddPokemon(
        PokemonDto pokemon,
        Guid trainerId)
    {
        if (pokemon.TrainerId == trainerId)
        {
            var model = DtoHandler.ParseFromDto(pokemon);
            _pokemonService.PostPokemon(model);
            return $"Import pokemon {pokemon.PokemonId}";
        }

        return $"Invalid trainer id from pokemon {pokemon.PokemonId}. Skipping...";
    }

    private async Task<(string Path, string Json)> GetStreamParts(GameDto game)
    {
        game.IsOnline = false;
        var exportedGame = await ExportedGame.ParseFromModel(game, _trainerService, _pokemonService);
        return (Path.GetTempFileName(), JsonSerializer.Serialize(exportedGame));
    }

    private static ExportedGame? GetParsedImportFromExport(string json)
    {
        JSchema schema = JSchema.Parse(File.ReadAllText(Paths.SchemaPath));
        var jsonObject = JObject.Parse(json);
        var errors = new List<string>();
        if (!jsonObject.IsValid(schema, out IList<string> errorMessages))
        {
            errors.AddRange(errorMessages);
            throw new ImportFailedException(errors);
        }

        return jsonObject.ToObject<ExportedGame>();
    }
}
