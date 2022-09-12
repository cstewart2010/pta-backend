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

        public static bool IsUserGM(
            this HttpRequest request,
            string userId,
            string gameId)
        {
            var gameMaster = DatabaseUtility.FindTrainerById(userId, gameId);
            return request.VerifyIdentity(userId) && gameMaster?.IsGM == true;
        }

        public static bool VerifyIdentity(
            this HttpRequest request,
            string id)
        {
            var user = DatabaseUtility.FindUserById(id);
            if (user == null)
            {
                return false;
            }

            if (!(request.Headers.TryGetValue("pta-activity-token", out var accessToken)
                && user.ActivityToken == accessToken
                && EncryptionUtility.ValidateToken(accessToken)))
            {
                return false;
            }

            if (!(request.Headers.TryGetValue("pta-session-auth", out var cookie) && EncryptionUtility.VerifySecret(AuthKey, cookie)))
            {
                return false;
            }

            return true;
        }

        public static async Task<bool> TryCompleteNpc(this HttpRequest request)
        {
            var json = await request.GetRequestBody();
            var publicNpc = PublicNpc.FromJson(json);
            var npc = publicNpc.ParseBackToModel();
           
            var result = DatabaseUtility.UpdateNpc(npc);
            return result;
        }
        public static async Task<bool> TryCompleteTrainer(this HttpRequest request)
        {
            var json = await request.GetRequestBody();
            var publicTrainer = PublicTrainer.FromJson(json);
            var trainer = publicTrainer.ParseBackToModel();
            AddTrainerPokemon(publicTrainer.NewPokemon, trainer);
            var result = DatabaseUtility.UpdateTrainer(trainer);
            if (result)
            {
                var game = DatabaseUtility.FindGame(trainer.GameId);
                var statsAddedLog = new LogModel
                {
                    User = trainer.TrainerName,
                    Action = $"updated their stats at {DateTime.UtcNow}"
                };
                DatabaseUtility.UpdateGameLogs(game, statsAddedLog);
            }

            return result;
        }

        public static void AddNpcPokemon(this HttpRequest request, IEnumerable<NewPokemon> pokemon, string npcId, string gameId)
        {
            foreach (var data in pokemon.Where(data => data != null))
            {
                var nickname = data.Nickname.Length > 18 ? data.Nickname.Substring(0, 18) : data.Nickname;
                var pokemonModel = DexUtility.GetNewPokemon(data.SpeciesName, nickname, data.Form);
                pokemonModel.IsOnActiveTeam = data.IsOnActiveTeam;
                pokemonModel.OriginalTrainerId = npcId;
                pokemonModel.TrainerId = npcId;
                pokemonModel.GameId = gameId;
                DatabaseUtility.TryAddPokemon(pokemonModel, out _);
            }
        }

        private static void AddTrainerPokemon(IEnumerable<NewPokemon> pokemon, TrainerModel trainer)
        {
            foreach (var data in pokemon.Where(data => data != null))
            {
                var nickname = data.Nickname.Length > 18 ? data.Nickname.Substring(0, 18) : data.Nickname;
                var pokemonModel = DexUtility.GetNewPokemon(data.SpeciesName, nickname, data.Form);
                pokemonModel.IsOnActiveTeam = data.IsOnActiveTeam;
                pokemonModel.OriginalTrainerId = trainer.TrainerId;
                pokemonModel.TrainerId = trainer.TrainerId;
                pokemonModel.GameId = trainer.GameId;
                DatabaseUtility.TryAddPokemon(pokemonModel, out _);
                var game = DatabaseUtility.FindGame(trainer.GameId);
                var caughtPokemonLog = new LogModel
                {
                    User = trainer.TrainerName,
                    Action = $"caught a {pokemonModel.SpeciesName} named {pokemonModel.Nickname} at {DateTime.UtcNow}"
                };
                DatabaseUtility.UpdateGameLogs(game, caughtPokemonLog);
                if (DatabaseUtility.GetPokedexItem(trainer.TrainerId, pokemonModel.DexNo) == null)
                {
                    DatabaseUtility.TryAddDexItem(trainer.TrainerId, pokemonModel.DexNo, true, true, out _);
                }
                else
                {
                    DatabaseUtility.UpdateDexItemIsCaught(trainer.TrainerId, pokemonModel.DexNo);
                }
            }
        }

        public static async Task<(PokemonModel Pokemon, AbstractMessage Message)> BuildPokemon(
            this HttpRequest request,
            string trainerId,
            string gameId)
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
            pokemon.GameId = gameId;
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

        public static async Task<(string Username, string Password, IEnumerable<string> Errors)> GetUserCredentials(
            this HttpRequest request)
        {
            var body = await request.GetRequestBody();
            var errors = new[] { "username", "password" }
                .Select(key => body[key] != null ? null : $"Missing {key}")
                .Where(error => error != null);
            var username = (string)body["username"];
            var password = (string)body["password"];

            return (username, password, errors);
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

            var form = (string)body["form"];
            if (!string.IsNullOrWhiteSpace(form))
            {
                return (null, new GenericMessage($"Mission form in request"));
            }

            var pokemon = DexUtility.GetNewPokemon
            (
                (string)body["pokemon"],
                nature,
                gender,
                status,
                (string)body["nickname"],
                form
            );

            if (pokemon == null)
            {
                return (null, new GenericMessage("Failed to build pokemon"));
            }

            return (pokemon, null);
        }
    }
}
