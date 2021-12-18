using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TheReplacement.PTA.Common.Enums;
using TheReplacement.PTA.Common.Models;
using TheReplacement.PTA.Common.Utilities;
using TheReplacement.PTA.Services.Core.Messages;
using TheReplacement.PTA.Services.Core.Objects;

namespace TheReplacement.PTA.Services.Core.Extensions
{
    internal static class RequestExtensions
    {
        private static readonly IEnumerable<string> MandatoryPokemonKeys = new[]
        {
            "pokemon",
            "nature",
            "gender",
            "status"
        };

        static RequestExtensions()
        {
            AuthKey = Environment.GetEnvironmentVariable("CookieKey");
        }

        internal static string AuthKey { get; }

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
        public static bool VerifyIdentity(
            this HttpRequest request,
            string id,
            bool isGM)
        {
            var trainer = DatabaseUtility.FindTrainerById(id);
            if (trainer == null)
            {
                return false;
            }

            if (isGM)
            {
                if (!trainer.IsGM)
                {
                    return false;
                }
            }

            if (!(request.Headers.TryGetValue("pta-activity-token", out var accessToken)
                && trainer.ActivityToken == accessToken
                && EncryptionUtility.ValidateToken(accessToken)))
            {
                LoggerUtility.Error(MongoCollection.Games, "Attempt 1");
                return false;
            }

            if (!(request.Headers.TryGetValue("pta-session-auth", out var cookie) && EncryptionUtility.VerifySecret(AuthKey, cookie)))
            {
                LoggerUtility.Error(MongoCollection.Games, "Attempt 2");
                return false;
            }

            return true;
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
            out AbstractMessage badRequestMessage)
        {
            var trainer =  BuildTrainer
            (
                request,
                gameId,
                "gmUsername",
                "gmPassword",
                true,
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
            out AbstractMessage badRequestMessage)
        {
            var trainer =  BuildTrainer
            (
                request,
                gameId,
                "username",
                "password",
                false,
                out badRequestMessage
            );

            if (badRequestMessage != null)
            {
                return null;
            }

            return trainer;
        }

        public static async Task<bool> TryCompleteTrainer(this HttpRequest request)
        {
            using var reader = new StreamReader(request.Body);
            var json = await reader.ReadToEndAsync();
            File.WriteAllText("trainer.txt", json);
            var publicTrainer = PublicTrainer.FromJsonString(json);
            var trainer = publicTrainer.ParseBackToModel();
            AddTrainerPokemon(publicTrainer.NewPokemon, trainer.TrainerId);
            return DatabaseUtility.UpdateTrainer(trainer);
        }

        private static void AddTrainerPokemon(IEnumerable<NewPokemon> pokemon, string trainerId)
        {
            foreach (var data in pokemon.Where(data => data != null))
            {
                var pokemonModel = DexUtility.GetNewPokemon(data.SpeciesName, data.Nickname);
                pokemonModel.IsOnActiveTeam = data.IsOnActiveTeam;
                pokemonModel.OriginalTrainerId = trainerId;
                pokemonModel.TrainerId = trainerId;
                DatabaseUtility.TryAddPokemon(pokemonModel, out _);
                if (DatabaseUtility.GetPokedexItem(trainerId, pokemonModel.DexNo) == null)
                {
                    DatabaseUtility.TryAddDexItem(trainerId, pokemonModel.DexNo, true, true, out _);
                }
                else
                {
                    DatabaseUtility.UpdateDexItemIsCaught(trainerId, pokemonModel.DexNo);
                }
            }
        }

        public static PokemonModel BuildPokemon(
            this HttpRequest request,
            string trainerId,
            out AbstractMessage badRequestMessage)
        {
            if (ValidateMandatoryPokemonKeys(request, out var invalid))
            {
                badRequestMessage = invalid;
                return null;
            }

            var pokemon = BuildDefaultPokemon(request, out var error);
            if (pokemon == null)
            {
                badRequestMessage = error;
                return null;
            }

            badRequestMessage = null;
            pokemon.TrainerId = trainerId;
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

        public static IEnumerable<string> GetNpcIds(
            this HttpRequest request,
            out AbstractMessage error)
        {
            error = null;
            var npcIds = request.Query["npcIds"].ToString().Split(',');
            if (npcIds.Any())
            {
                var foundNpcs = DatabaseUtility.FindNpcs(npcIds).Select(npc => npc.NPCId).ToArray();
                if (!npcIds.Any())
                {
                    error = new InvalidQueryStringMessage()
                    {
                        InvalidParameters = new[] { "npcIds" }
                    };
                }

                return foundNpcs;
            }

            error = new InvalidQueryStringMessage()
            {
                MissingParameters = new[] { "npcIds" }
            };

            return Array.Empty<string>();
        }

        private static bool ValidateMandatoryPokemonKeys(
            HttpRequest request,
            out InvalidQueryStringMessage error)
        {
            var fails = MandatoryPokemonKeys.Where(key => string.IsNullOrWhiteSpace(request.Query[key]));
            error = new InvalidQueryStringMessage
            {
                MissingParameters = fails
            };
            return fails.Any();
        }

        private static PokemonModel BuildDefaultPokemon(
            HttpRequest request,
            out AbstractMessage error)
        {
            error = null;
            if (!Enum.TryParse(request.Query["gender"], true, out Gender gender))
            {
                error = new GenericMessage($"Invalid gender in request");
                return null;
            }

            if (!Enum.TryParse(request.Query["nature"], true, out Nature nature))
            {
                error = new GenericMessage($"Invalid nature in request");
                return null;
            }

            if (!Enum.TryParse(request.Query["status"], true, out Status status))
            {
                error = new GenericMessage($"Invalid status in request");
                return null;
            }

            var pokemon = DexUtility.GetNewPokemon
            (
                request.Query["pokemon"],
                nature,
                gender,
                status,
                request.Query["nickname"]
            );

            if (pokemon == null)
            {
                error = new GenericMessage("Failed to build pokemon");
                return null;
            }

            return pokemon;
        }

        private static TrainerModel BuildTrainer(
            HttpRequest request,
            string gameId,
            string userKey,
            string passKey,
            bool isGM,
            out AbstractMessage error)
        {
            var username = request.Query[userKey].ToString();
            if (string.IsNullOrWhiteSpace(username))
            {
                error = new GenericMessage($"Missing {userKey} for {gameId}");
                return null;
            }

            if (DatabaseUtility.FindTrainerByUsername(username, gameId) != null)
            {
                error = new GenericMessage($"Duplicate username {username}");
                return null;
            }

            var password = request.Query[passKey].ToString();
            if (string.IsNullOrWhiteSpace(password))
            {
                error = new GenericMessage($"Missing {passKey} for {gameId}");
                return null;
            }

            error = null;
            var stats = GetStatsFromRequest(request.Query, false);
            var trainer = CreateTrainer(gameId, username, password, stats);
            trainer.IsGM = isGM;
            return trainer;
        }

        private static StatsModel GetStatsFromRequest(IQueryCollection query, bool isStarting)
        {
            if (!isStarting)
            {
                return new StatsModel
                {
                    HP = 20,
                    Attack = 1,
                    Defense = 1,
                    SpecialAttack = 1,
                    SpecialDefense = 1,
                    Speed = 1
                };
            }

            return new StatsModel
            {
                HP = 20,
                Attack = int.Parse(query["attack"]),
                Defense = int.Parse(query["defense"]),
                SpecialAttack = int.Parse(query["specialAttack"]),
                SpecialDefense = int.Parse(query["specialDefense"]),
                Speed = int.Parse(query["speed"])
            };
        }

        private static TrainerModel CreateTrainer(
            string gameId,
            string username,
            string password,
            StatsModel stats)
        {
            return new TrainerModel
            {
                GameId = gameId,
                TrainerId = Guid.NewGuid().ToString(),
                Honors = Array.Empty<string>(),
                TrainerName = username,
                PasswordHash = EncryptionUtility.HashSecret(password),
                TrainerClasses = Array.Empty<string>(),
                Feats = Array.Empty<string>(),
                IsOnline = true,
                Items = new List<ItemModel>(),
                TrainerStats = stats,
                ActivityToken = EncryptionUtility.GenerateToken(),
                Origin = string.Empty
            };
        }
    }
}
