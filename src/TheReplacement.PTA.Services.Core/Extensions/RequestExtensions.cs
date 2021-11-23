using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheReplacement.PTA.Common.Models;
using TheReplacement.PTA.Common.Utilities;

namespace TheReplacement.PTA.Services.Core.Extensions
{
    internal static class RequestExtensions
    {
        private static readonly IEnumerable<string> MandatoryPokemonKeys = new[] { "pokemon", "nature", "naturalMoves", "expYield", "catchRate", "experience", "level" };
        public static string GetJsonFromRequest(this HttpRequest request)
        {
            var jsonFile = request.Form.Files.First(file => Path.GetExtension(file.FileName).ToLower() == ".json");
            if (jsonFile.Length > 0)
            {
                using var reader = new StreamReader(jsonFile.OpenReadStream());
                var json = reader.ReadToEnd();
                reader.Close();
                return json;
            }

            return null;
        }

        public static GameModel BuildGame(this HttpRequest request)
        {
            var guid = Guid.NewGuid().ToString();
            var nickname = request.Query["nickname"].ToString();
            return new GameModel
            {
                GameId = guid,
                Nickname = string.IsNullOrEmpty(nickname)
                    ? guid.Split('-')[0]
                    : nickname,
                IsOnline = true,
                PasswordHash = EncryptionUtility.HashSecret(request.Query["gameSessionPassword"]),
                NPCs = Array.Empty<string>()
            };
        }

        public static TrainerModel BuildGM(
            this HttpRequest request,
            string gameId,
            out object badRequestMessage)
        {
            var trainer =  BuildTrainer
            (
                request,
                gameId,
                "gmUsername",
                "gmPassword",
                out badRequestMessage
            );

            if (trainer != null)
            {
                trainer.IsGM = true;
            }

            return trainer;
        }

        public static TrainerModel BuildTrainer(
            this HttpRequest request,
            string gameId,
            out object badRequestMessage)
        {
            var trainer =  BuildTrainer
            (
                request,
                gameId,
                "username",
                "password",
                out badRequestMessage
            );

            if (!(trainer == null || DatabaseUtility.FindTrainerByUsername(trainer.TrainerName, gameId) == null))
            {
                badRequestMessage = new
                {
                    message = "Duplicate trainerName",
                    gameId,

                };
            }

            return trainer;
        }

        public static PokemonModel BuildPokemon(
            this HttpRequest request,
            string trainerId,
            out object badRequestMessage)
        {
            if (ValidateMandatoryPokemonKeys(request, out badRequestMessage))
            {
                return null;
            }

            var pokemon = BuildDefaultPokemon(request, out badRequestMessage);
            if (pokemon == null)
            {
                return null;
            }

            if (!TryCompletePokemon(request, pokemon, out badRequestMessage))
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(request.Query["nickname"]))
            {
                pokemon.Nickname = request.Query["nickname"];
            }

            pokemon.TrainerId = trainerId;
            pokemon.AggregateStats();
            return pokemon;
        }

        public static (string GamePassword, string GameMasterUsername, string GamemasterPassword) GetStartGameCredentials(
            this HttpRequest request,
            out IEnumerable<string> errors)
        {
            errors = new[] { "gameSessionPassword", "gmUsername", "gmPassword" }
                .Select(key => request.Query.ContainsKey(key) ? null : $"Missing {key}")
                .Where(error => error != null);
            var gamePassword = request.Query["gameSessionPassword"];
            var username = request.Query["gmUsername"];
            var password = request.Query["gmPassword"];

            return (gamePassword, username, password);
        }

        public static (string GameId, string Username, string Password) GetTrainerCredentials(
            this HttpRequest request,
            out IEnumerable<string> errors)
        {
            errors = new[] { "gameId", "trainerName", "password" }
                .Select(key => request.Query.ContainsKey(key) ? null : $"Missing {key}")
                .Where(error => error != null);
            var gameId = request.Query["gameId"];
            var trainerName = request.Query["trainerName"];
            var password = request.Query["password"];

            return (gameId, trainerName, password);
        }

        public static IEnumerable<string> GetNpcIds(this HttpRequest request, out object error)
        {
            error = null;
            var npcIds = request.Query["npcIds"].ToString().Split(',');
            if (npcIds.Any())
            {
                npcIds = DatabaseUtility.FindNpcs(npcIds).Select(npc => npc.NPCId).ToArray();
                if (!npcIds.Any())
                {
                    error = npcIds;
                }
            }
            else
            {
                error = new
                {
                    message = "No npc Ids provided"
                };
            }

            return npcIds;
        }

        private static bool ValidateMandatoryPokemonKeys(HttpRequest request,
            out object error)
        {
            var fails = MandatoryPokemonKeys.Where(key => string.IsNullOrWhiteSpace(request.Query[key]));
            error = new
            {
                message = "Missing the following parameters in the query",
                fails
            };
            return fails.Any();
        }

        private static bool TryCompletePokemon(
            HttpRequest request,
            PokemonModel pokemon,
            out object error)
        {
            (pokemon.ExpYield, pokemon.CatchRate, pokemon.Experience, pokemon.Level) = GetNumerics(request, out var numberErrors);
            (pokemon.NaturalMoves, pokemon.TMMoves) = GetMoveLists(request, out var movesErrors);
            if (numberErrors.Any() || movesErrors.Any())
            {
                error = new
                {
                    numberErrors,
                    movesErrors
                };

                return false;
            }

            error = null;
            return true;
        }

        private static PokemonModel BuildDefaultPokemon(
            HttpRequest request,
            out object error)
        {
            error = null;
            var pokemonName = request.Query["pokemon"];
            var natureName = request.Query["nature"];
            var pokemon = PokeAPIUtility.GetPokemon
            (
                pokemonName,
                natureName
            );

            if (pokemon == null)
            {
                error = new
                {
                    message = "Failed to build pokemon"
                };

                return null;
            }

            return pokemon;
        }

        private static TrainerModel BuildTrainer(
            HttpRequest request,
            string gameId,
            string userKey,
            string passKey,
            out object badRequestMessage)
        {
            var username = request.Query[userKey].ToString();
            badRequestMessage = null;
            if (string.IsNullOrWhiteSpace(username))
            {
                badRequestMessage = new
                {
                    message = $"Missing {userKey}",
                    gameId
                };
                return null;
            }

            var password = request.Query[passKey].ToString();
            if (string.IsNullOrWhiteSpace(username))
            {
                badRequestMessage = new
                {
                    message = $"Missing {passKey}",
                    gameId
                };
                return null;
            }

            return CreateTrainer(gameId, username, password);
        }

        private static TrainerModel CreateTrainer(
            string gameId,
            string username,
            string password)
        {
            return new TrainerModel
            {
                GameId = gameId,
                TrainerId = Guid.NewGuid().ToString(),
                TrainerName = username,
                PasswordHash = EncryptionUtility.HashSecret(password),
                Items = new List<ItemModel>(),
                IsOnline = true,
                TrainerClasses = new List<string>(),
                TrainerStats = new TrainerStatsModel(),
                Level = 1,
                Feats = new List<string>(),
                ActivityToken = EncryptionUtility.GenerateToken()
            };
        }

        private static (int ExpYield, int CatchRate, int Experience, int Level) GetNumerics(
            HttpRequest request,
            out IEnumerable<object> parsingFailures)
        {
            parsingFailures = new[]
            {
                GetBadRequestMessage(request, "expYield", result => result > 0, out var expYield),
                GetBadRequestMessage(request, "catchRate", result => result >= 0, out var catchRate),
                GetBadRequestMessage(request, "experience", result => result >= 0, out var experience),
                GetBadRequestMessage(request, "level", result => result > 0, out var level),
            }.Where(fail => fail != null);

            return (expYield, catchRate, experience, level);
        }

        private static (IEnumerable<string> NaturalMoves, IEnumerable<string> TMMoves) GetMoveLists(
            HttpRequest request,
            out IEnumerable<object> parsingFailures)
        {
            var fails = new List<object>();
            var naturalMoves = GetMoves(request, "naturalMoves", out var naturalMovesError);
            if (naturalMoves.Count() < 1 || naturalMoves.Count() > 4)
            {
                fails.Add(naturalMovesError);
            }
            var tmMoves = GetMoves(request, "tmMoves", out var tmMovesError);
            if (tmMoves.Count() > 4)
            {
                fails.Add(tmMovesError);
            }
            parsingFailures = fails;
            return (naturalMoves, tmMoves);
        }

        private static IEnumerable<string> GetMoves(
            HttpRequest request,
            string moveType,
            out object error)
        {
            error = new
            {
                message = $"Invalid move list count",
                moveList = moveType
            };

            return request.Query[moveType].ToString().Split(",");
        }

        private static object GetBadRequestMessage(
            HttpRequest request,
            string parameter,
            Predicate<int> check,
            out int outVar)
        {
            var value = request.Query[parameter];
            if (!(int.TryParse(value, out outVar) && check(outVar)))
            {
                return new
                {
                    message = $"Invalid {parameter}",
                    invalidValue = value
                };
            }

            return null;
        }
    }
}
