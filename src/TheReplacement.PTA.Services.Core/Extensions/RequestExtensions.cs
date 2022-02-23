using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
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

        public static async Task<(TrainerModel GameMaster, AbstractMessage Message)> BuildGM(
            this HttpRequest request,
            string gameId)
        {
            var (gm, badRequestMessage) = await BuildTrainer
            (
                request,
                gameId,
                "gmUsername",
                "gmPassword",
                true
            );

            if (gm != null)
            {
                gm.IsGM = true;
            }

            return (gm, badRequestMessage);
        }

        public static async Task<(TrainerModel Trainer, AbstractMessage Message)> BuildTrainer(
            this HttpRequest request,
            string gameId)
        {
            var (trainer, badRequestMessage) = await BuildTrainer
            (
                request,
                gameId,
                "username",
                "password",
                false
            );

            if (badRequestMessage != null)
            {
                return (null, badRequestMessage);
            }

            return (trainer, null);
        }

        public static async Task<bool> TryCompleteTrainer(this HttpRequest request)
        {
            var json = await request.GetRequestBody();
            var publicTrainer = PublicTrainer.FromJson(json);
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

        public static async Task<(PokemonModel Pokemon, AbstractMessage Message)> BuildPokemon(
            this HttpRequest request,
            string trainerId)
        {
            var (isValid, message) = await ValidateMandatoryPokemonKeys(request);
            if (isValid)
            {
                return (null, message);
            }

            var (pokemon, error) = await BuildDefaultPokemon(request);
            if (pokemon == null)
            {
                return (null, error);
            }

            pokemon.TrainerId = trainerId;
            pokemon.OriginalTrainerId = trainerId;
            return (pokemon, null);
        }

        public static async Task<(string GamePassword, string GameMasterUsername, string GamemasterPassword, IEnumerable<string> Errors)> GetStartGameCredentials(
            this HttpRequest request)
        {
            var body = await request.GetRequestBody();
            var errors = new[] { "gameSessionPassword", "gmUsername", "gmPassword" }
                .Select(key => body[key] != null ? null : $"Missing {key}")
                .Where(error => error != null);
            var gamePassword = (string)body["gameSessionPassword"];
            var username = (string)body["gmUsername"];
            var password = (string)body["gmPassword"];

            return (gamePassword, username, password, errors);
        }

        public static async Task<(string GameId, string Username, string Password, IEnumerable<string> Errors)> GetTrainerCredentials(
            this HttpRequest request)
        {
            var body = await request.GetRequestBody();
            var errors = new[] { "trainerName", "password" }
                .Select(key => body[key] != null ? null : $"Missing {key}")
                .Where(error => error != null);
            var gameId = request.Query["gameId"];
            var trainerName = (string)body["trainerName"];
            var password = (string)body["password"];

            return (gameId, trainerName, password, errors);
        }

        public static async Task<JToken> GetRequestBody(this HttpRequest request)
        {
            using var reader = new StreamReader(request.Body);
            var json = await reader.ReadToEndAsync();
            return JToken.Parse(json);
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

        private static async Task<(bool IsValid, InvalidQueryStringMessage Message)>  ValidateMandatoryPokemonKeys(
            HttpRequest request)
        {
            var body = await request.GetRequestBody();
            var fails = MandatoryPokemonKeys.Where(key => string.IsNullOrWhiteSpace((string)body[key]));
            var error = new InvalidQueryStringMessage
            {
                MissingParameters = fails
            };
            return (fails.Any(), error);
        }

        private static async Task<(PokemonModel Pokemon, AbstractMessage Message)> BuildDefaultPokemon(HttpRequest request)
        {
            var body = await request.GetRequestBody();
            if (!Enum.TryParse((string)body["gender"], true, out Gender gender))
            {
                return (null, new GenericMessage($"Invalid gender in request"));
            }

            if (!Enum.TryParse((string)body["nature"], true, out Nature nature))
            {
                return (null, new GenericMessage($"Invalid nature in request"));
            }

            if (!Enum.TryParse((string)body["status"], true, out Status status))
            {
                return (null, new GenericMessage($"Invalid status in request"));
            }

            var pokemon = DexUtility.GetNewPokemon
            (
                (string)body["pokemon"],
                nature,
                gender,
                status,
                (string)body["nickname"]
            );

            if (pokemon == null)
            {
                return (null, new GenericMessage("Failed to build pokemon"));
            }

            return (pokemon, null);
        }

        private static async Task<(TrainerModel Trainer, AbstractMessage Error)> BuildTrainer(
            HttpRequest request,
            string gameId,
            string userKey,
            string passKey,
            bool isGM)
        {
            var body = await request.GetRequestBody();
            var username = body[userKey].ToString();
            if (string.IsNullOrWhiteSpace(username))
            {
                return (null, new GenericMessage($"Missing {userKey} for {gameId}"));
            }

            if (DatabaseUtility.FindTrainerByUsername(username, gameId) != null)
            {
                return (null, new GenericMessage($"Duplicate username {username}"));
            }

            var password = body[passKey].ToString();
            if (string.IsNullOrWhiteSpace(password))
            {
                return (null, new GenericMessage($"Missing {passKey} for {gameId}"));
            }

            var trainer = CreateTrainer(gameId, username, password);
            trainer.IsGM = isGM;
            return (trainer, null);
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
                Honors = Array.Empty<string>(),
                TrainerName = username,
                PasswordHash = EncryptionUtility.HashSecret(password),
                TrainerClasses = Array.Empty<string>(),
                Feats = Array.Empty<string>(),
                IsOnline = true,
                Items = new List<ItemModel>(),
                TrainerStats = GetStats(),
                ActivityToken = EncryptionUtility.GenerateToken(),
                Origin = string.Empty
            };
        }

        private static StatsModel GetStats()
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
    }
}
