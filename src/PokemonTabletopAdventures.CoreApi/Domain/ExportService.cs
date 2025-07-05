using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using PokemonTabletopAdventures.CoreApi.Domain.Models;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.Domain
{
    /// <summary>
    /// Provides a collection of methods to handle import/exports of game sessions
    /// </summary>
    public class ExportService(IPokemonService pokemonService, ITrainerService trainerService, IGameService gameService) : IExportService
    {
        private readonly IPokemonService _pokemonService = pokemonService;
        private readonly ITrainerService _trainerService = trainerService;
        private readonly IGameService _gameService = gameService;

        /// <summary>
        /// Returns a file stream for the a json file of the game session
        /// </summary>
        /// <param name="game">The game session to export</param>
        public async Task<FileStream> GetExportStream(GameModel game)
        {
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
        public async Task TryParseImport(string json)
        {
            var import = GetParsedImportFromExport(json, out List<string> errors);
            if (errors.Count == 0)
            {
                errors = CleanyAddGame(import!);
            }
            if (errors.Count == 0)
            {
                throw new GeneralErrorsException(errors);
            }

            await Task.CompletedTask;
        }

        private List<string> CleanyAddGame(ExportedGame import)
        {
            var errors = AddGame(import);
            if (errors.Count != 0)
            {
                return errors;
            }

            var gameId = import.GameSession.GameId;
            foreach (var trainer in import.Trainers)
            {
                errors.AddRange(CleanlyAddTrainer(trainer, gameId));
            }

            return errors;
        }

        private List<string> AddGame(ExportedGame import)
        {
            var game = import.GameSession;
            var errors = new List<string>();
            if (import.Trainers.SingleOrDefault(import => import.Trainer.IsGM) == null)
            {
                errors.Add($"Exactly one gm should be listed for game {game.GameId}");
            }

            if (_gameService.GetGame(game.GameId) != null)
            {
                errors.Add($"Found extant game session found with id {game.GameId}");
            }

            game.Logs ??= [];
            game.Logs = game.Logs.Append(new LogModel
            (
                user: "Import Tool",
                action: $"Recreated game {game.GameId}"
            ));
            _gameService.PostGame(game);
            return errors;
        }

        private List<string> CleanlyAddTrainer(
            ExportedTrainer import,
            Guid gameId)
        {
            var errors = new List<string>();
            var trainer = import.Trainer;
            if (!(trainer.GameId == gameId))
            {
                errors.Add($"Failed to import trainer {trainer.TrainerId}");
                return errors;
            }

            _trainerService.PostTrainer(trainer);
            errors = [.. import.Pokemon
                .Select(pokemon => AddPokemon(pokemon, trainer.TrainerId))
                .Where(error => !string.IsNullOrEmpty(error))];

            return errors;
        }

        private string AddPokemon(
            PokemonModel pokemon,
            Guid trainerId)
        {
            if (pokemon.TrainerId == trainerId)
            {
                _pokemonService.PostPokemon(pokemon);
                return $"Failed to import pokemon {pokemon.PokemonId}";
            }

            return $"Invalid trainer id from pokemon {pokemon.PokemonId}. Skipping...";
        }

        private async Task<(string Path, string Json)> GetStreamParts(GameModel game)
        {
            game.IsOnline = false;
            var exportedGame = await ExportedGame.ParseFromModel(game, _trainerService, _pokemonService);
            return (Path.GetTempFileName(), JsonSerializer.Serialize(exportedGame));
        }

        private static ExportedGame? GetParsedImportFromExport(
            string json,
            out List<string> errors)
        {
            JSchema schema = JSchema.Parse(File.ReadAllText("./ExportedGame.schema.json"));
            var jsonObject = JObject.Parse(json);
            errors = [];
            if (!jsonObject.IsValid(schema, out IList<string> errorMessages))
            {
                errors.AddRange(errorMessages);
                return null;
            }

            return jsonObject.ToObject<ExportedGame>();
        }
    }
}
