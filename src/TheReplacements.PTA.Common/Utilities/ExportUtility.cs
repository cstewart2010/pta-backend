using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheReplacements.PTA.Common.Enums;
using TheReplacements.PTA.Common.Internal;
using TheReplacements.PTA.Common.Models;

namespace TheReplacements.PTA.Common.Utilities
{
    public static class ExportUtility
    {
        public static FileStream GetExportStream(GameModel game)
        {
            game.IsOnline = false;
            var exportedGame = new ExportedGame(game);
            var path = Path.GetTempFileName();
            var json = JsonConvert.SerializeObject
            (
                exportedGame,
                Formatting.Indented
            );

            using var reader = new StringReader(json);
            using var writer = new StreamWriter(path);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                writer.WriteLine(line);
            }

            reader.Close();
            writer.Close();
            return new FileStream(path, FileMode.Open);
        }

        public static bool TryParseImport(
            string json,
            out IList<string> errors)
        {
            var import = GetParsedImportFromExport(json, out errors);
            if (errors.Any())
            {
                return false;
            }

            var gameId = import.GameSession.GameId;
            var warningErrors = new List<string>();
            if (!CleanyAddGame(import, out errors))
            {
                return false;
            }

            foreach (var trainer in import.Trainers)
            {
                CleanlyAddTrainer(trainer, gameId, out var trainerErrors);
                warningErrors.AddRange(trainerErrors);
            }

            errors = warningErrors;
            LoggerUtility.Info(MongoCollection.Trainer, $"Successfully imported game {gameId}");
            return true;
        }

        private static ExportedGame GetParsedImportFromExport(
            string json,
            out IList<string> errors)
        {
            JSchema schema = JSchema.Parse(File.ReadAllText("./ExportedGame.schema.json"));
            var jobject = JObject.Parse(json);
            if (!jobject.IsValid(schema, out IList<string> errorMessages))
            {
                errors = errorMessages;
                return null;
            }

            errors = Array.Empty<string>();
            return jobject.ToObject<ExportedGame>();
        }

        private static bool CleanyAddGame(
            ExportedGame import,
            out IList<string> errors)
        {
            var game = import.GameSession;
            errors = new List<string>();

            if (import.Trainers.Single(import => import.Trainer.IsGM) == null)
            {
                errors.Add($"Multiple gms lists for game {game.GameId}");
                return false;
            }

            if (DatabaseUtility.FindGame(game.GameId) != null)
            {
                errors.Add($"Game session found with id {game.GameId}");
                return false;
            }

            if (!DatabaseUtility.TryAddGame(game, out var error))
            {
                errors.Add(error.WriteErrorJsonString);
                return false;
            }

            return true;
        }

        private static bool CleanlyAddTrainer(
            ExportedTrainer import,
            string gameId,
            out List<string> errors)
        {
            errors = new List<string>();
            var trainer = import.Trainer;
            if (trainer.GameId != gameId)
            {
                errors.Add($"Invalid game id for trainer {trainer.TrainerId}. Skipping...");
                return false;
            }
            if (!DatabaseUtility.TryAddTrainer(import.Trainer, out _))
            {
                errors.Add($"Failed to import trainer {import.Trainer.TrainerId}");
                return false;
            }

            LoggerUtility.Info(MongoCollection.Trainer, $"Successfully imported trainer {import.Trainer.TrainerId}");
            var errorList = import.Pokemon
                .Select(pokemon => CleanlyAddPokemon(pokemon, trainer.TrainerId))
                .Where(error => !string.IsNullOrEmpty(error));
            errors.AddRange(errorList);
            return true;
        }

        private static string CleanlyAddPokemon(
            PokemonModel pokemon,
            string trainerId)
        {
            if (pokemon.TrainerId == trainerId)
            {
                if (DatabaseUtility.TryAddPokemon(pokemon, out _))
                {
                    return null;
                }

                return $"Failed to import pokemon {pokemon.PokemonId}";
            }

            return $"Invalid trainer id from pokemon {pokemon.PokemonId}. Skipping...";
        }
    }
}
