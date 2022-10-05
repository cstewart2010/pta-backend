using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using TheReplacement.PTA.Common.Utilities;
using TheReplacement.PTA.Common.Models;
using TheReplacement.PTA.Common.Enums;
using TheReplacement.PTA.Services.Core.Extensions;
using TheReplacement.PTA.Services.Core.Messages;
using System.Collections;
using System.Threading.Tasks;
using System;

namespace TheReplacement.PTA.Services.Core.Controllers
{
    [ApiController]
    [Route("api/v1/game")]
    public class GameController : PtaControllerBase
    {
        protected override MongoCollection Collection { get; }

        public GameController()
        {
            Collection = MongoCollection.Games;
        }

        [HttpGet("user/{userId}")]
        public ActionResult<IEnumerable> FindAllGames(Guid userId, [FromQuery] string nickname)
        {
            if (!Request.VerifyIdentity(userId))
            {
                return Unauthorized();
            }

            var user = DatabaseUtility.FindUserById(userId);
            if (!string.IsNullOrWhiteSpace(nickname))
            {
                return Ok(DatabaseUtility.FindAllGames(nickname)
                    .Where(game => !user.Games.Contains(game.GameId))
                    .Select(game => new FoundGameMessage(game.GameId, game.Nickname)));
            }

            return Ok(DatabaseUtility.FindMostRecent20Games(user));
        }

        [HttpGet("user/games/{userId}")]
        public ActionResult<IEnumerable> FindAllUserGames(Guid userId)
        {
            if (!Request.VerifyIdentity(userId))
            {
                return Unauthorized();
            }

            var user = DatabaseUtility.FindUserById(userId);
            return Ok(DatabaseUtility.FindAllGamesWithUser(user)
                    .Select(game => new FoundGameMessage(game.GameId, game.Nickname)));
        }

        [HttpGet("sprites/all")]
        public IEnumerable<SpriteModel> GetAllSprites()
        {
            return DatabaseUtility.GetAllSprites().OrderBy(sprite => sprite.FriendlyText);
        }

        [HttpGet("getGame/{gameId}")]
        public ActionResult<FoundGameMessage> FindGame(Guid gameId)
        {
            var document = GetDocument(gameId, Collection, out var notFound);
            if (!(document is GameModel))
            {
                return notFound;
            }

            return new FoundGameMessage(gameId);
        }

        [HttpGet("{gameId}/{trainerId}/find")]
        public ActionResult<FoundTrainerMessage> FindTrainerInGame(Guid gameId, Guid trainerId)
        {
            var gameDocument = GetDocument(gameId, Collection, out var notFound);
            if (!(gameDocument is GameModel))
            {
                return notFound;
            }

            var trainer = DatabaseUtility.FindTrainerById(trainerId, gameId);
            if (trainer == null)
            {
                return NotFound(trainerId);
            }

            if (trainer.GameId != gameId)
            {
                return BadRequest(new InvalidGameIdMessage(trainer));
            }

            return new FoundTrainerMessage(trainerId, gameId);
        }

        [HttpGet("{gameId}/all_logs")]
        public ActionResult<AllLogsMessage> GetAllLogs(Guid gameId)
        {
            var gameDocument = GetDocument(gameId, Collection, out var notFound);
            if (gameDocument is not GameModel game)
            {
                return notFound;
            }

            return new AllLogsMessage(game);
        }

        [HttpGet("{gameId}/logs")]
        public ActionResult<IEnumerable<LogModel>> GetLogs(Guid gameId)
        {
            var gameDocument = GetDocument(gameId, Collection, out var notFound);
            if (gameDocument is not GameModel game)
            {
                return notFound;
            }

            if (game.Logs == null)
            {
                return Array.Empty<LogModel>();
            }

            return game.Logs.Reverse().Take(50).ToArray();
        }

        [HttpPost("import")]
        public ActionResult<object> ImportGame()
        {
            var json = Request.GetJsonFromRequest();
            if (string.IsNullOrEmpty(json))
            {
                return BadRequest(new GenericMessage("empty json file"));
            }

            if (!ExportUtility.TryParseImport(json, out var errors))
            {
                return BadRequest(errors);
            }

            return Ok(errors);
        }

        [HttpPost("{userId}/newGame")]
        public ActionResult<CreatedGameMessage> CreateNewGame(Guid userId, [FromQuery] string nickname, [FromQuery] string gameSessionPassword, [FromQuery] string username)
        {
            var game = BuildGame(nickname, gameSessionPassword);
            if (!DatabaseUtility.TryAddGame(game, out var error))
            {
                return BadRequest(error);
            }

            var (gm, badRequest) = BuildGM(game.GameId, userId, username);
            if (gm == null)
            {
                return BadRequest(badRequest);
            }

            if (!DatabaseUtility.TryAddTrainer(gm, out error))
            {
                return BadRequest(error);
            }

            var gameCreationLog = new LogModel
            {
                User = gm.TrainerName,
                Action = $"created a new game and joined as game master at {DateTime.UtcNow}"
            };
            DatabaseUtility.UpdateGameLogs(game, gameCreationLog);
            Response.RefreshToken(userId);
            return new CreatedGameMessage(gm);
        }

        [HttpPost("{gameId}/{userId}/newUser")]
        public ActionResult<FoundTrainerMessage> AddPlayerToGame(Guid gameId, Guid userId, [FromQuery] string username)
        {
            if (gameId == Guid.Empty)
            {
                return BadRequest(nameof(gameId));
            }

            var gameDocument = GetDocument(gameId, Collection, out var notFound);
            if (gameDocument is not GameModel game)
            {
                return notFound;
            }

            if (!DatabaseUtility.HasGM(gameId, out var noGMError))
            {
                return BadRequest(noGMError);
            }

            var (trainer, badRequest) = BuildTrainer(gameId, userId, username);
            if (trainer == null)
            {
                return BadRequest(badRequest);
            }

            if (!DatabaseUtility.TryAddTrainer(trainer, out var error))
            {
                return BadRequest(error);
            }

            var trainerCreationLog = new LogModel
            {
                User = trainer.TrainerName,
                Action = $"joined at {DateTime.UtcNow}"
            };
            DatabaseUtility.UpdateGameLogs(game, trainerCreationLog);
            Response.RefreshToken(userId);
            return new FoundTrainerMessage(trainer.TrainerId, gameId);
        }

        [HttpPost("{gameId}/{gameMasterId}/wild")]
        public async Task<ActionResult<PokemonModel>> AddPokemon(Guid gameId, Guid gameMasterId)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            var (pokemon, error) = await Request.BuildPokemon(gameMasterId, gameId);
            if (pokemon == null)
            {
                return BadRequest(error);
            }
            if (!DatabaseUtility.TryAddPokemon(pokemon, out var writeError))
            {
                return BadRequest(writeError);
            }

            var game = DatabaseUtility.FindGame(gameId);
            var pokemonCreationLog = new LogModel
            {
                User = pokemon.SpeciesName,
                Action = $"spawned at {DateTime.UtcNow}"
            };
            DatabaseUtility.UpdateGameLogs(game, pokemonCreationLog);
            Response.RefreshToken(gameMasterId);
            return pokemon;
        }

        [HttpPost("{gameId}/{trainerId}/log")]
        public async Task<ActionResult<LogModel>> PostLogAsync(Guid gameId, Guid trainerId)
        {
            var body = await Request.GetRequestBody();
            if (!Request.VerifyIdentity(trainerId))
            {
                return Unauthorized();
            }

            var log = body.ToObject<LogModel>();
            log.Action += $" at {DateTime.UtcNow}";
            DatabaseUtility.UpdateGameLogs(DatabaseUtility.FindGame(gameId), log);
            Response.RefreshToken(trainerId);
            return log;
        }

        [HttpPost("{gameId}/{gameMasterId}/{trainerId}/allow")]
        public ActionResult AllowUser(Guid gameId, Guid gameMasterId, Guid trainerId)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            var trainer = DatabaseUtility.FindTrainerById(trainerId, gameId);
            if (trainer == null)
            {
                return NotFound(trainerId);
            }

            trainer.IsAllowed = true;
            DatabaseUtility.UpdateTrainer(trainer);
            var log = new LogModel
            {
                User = trainer.TrainerName,
                Action = $"joined the game at {DateTime.UtcNow}"
            };
            DatabaseUtility.UpdateGameLogs(DatabaseUtility.FindGame(gameId), log);
            Response.RefreshToken(gameMasterId);
            return Ok();
        }

        [HttpPost("{gameId}/{gameMasterId}/{trainerId}/disallow")]
        public ActionResult DisallowUser(Guid gameId, Guid gameMasterId, Guid trainerId)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            var trainer = DatabaseUtility.FindTrainerById(trainerId, gameId);
            if (trainer == null)
            {
                return NotFound(trainerId);
            }

            trainer.IsAllowed = false;
            DatabaseUtility.UpdateTrainer(trainer);
            var log = new LogModel
            {
                User = trainer.TrainerName,
                Action = $"was removed from the game at {DateTime.UtcNow}"
            };
            DatabaseUtility.UpdateGameLogs(DatabaseUtility.FindGame(gameId), log);
            Response.RefreshToken(gameMasterId);
            return Ok();
        }

        [HttpPut("{gameId}/{trainerId}/addStats")]
        public async Task<ActionResult<FoundTrainerMessage>> AddTrainerStats(Guid gameId, Guid trainerId)
        {
            if (!Request.VerifyIdentity(trainerId))
            {
                return Unauthorized();
            }

            var result = await Request.TryCompleteTrainer();
            if (!result)
            {
                return BadRequest(new GenericMessage("Failed to update trainer"));
            }
            return new FoundTrainerMessage(trainerId, gameId);
        }

        [HttpPut("{gameId}/{gameMasterId}/start")]
        public ActionResult<FoundGameMessage> StartGame(Guid gameId, Guid gameMasterId, [FromQuery] string gameSessionPassword)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            var gameDocument = GetDocument(gameId, Collection, out var notFound);
            if (gameDocument is not GameModel game)
            {
                return notFound;
            }

            if (!IsGameAuthenticated(gameSessionPassword, game, out var authError))
            {
                return authError;
            }

            var trainer = DatabaseUtility.FindTrainerById(gameMasterId, gameId);
            Response.AssignAuthAndToken(trainer.TrainerId);
            return new FoundGameMessage(gameId);
        }

        [HttpPut("{gameId}/{gameMasterId}/end")]
        public ActionResult EndGame(Guid gameId, Guid gameMasterId)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            var gameDocument = GetDocument(gameId, Collection, out var notFound);
            if (!(gameDocument is GameModel))
            {
                return notFound;
            }

            SetEndGameStatuses(gameId);
            return Ok();
        }

        [HttpPut("{gameId}/{gameMasterId}/addNpcs")]
        public ActionResult<UpdatedNpcListMessage> AddNPCsToGame(Guid gameId, Guid gameMasterId)
        {
            var npcIds = GetNpcs(gameMasterId, gameId, out var notFound);
            if (npcIds == null)
            {
                return notFound;
            }

            var gameDocument = GetDocument(gameId, Collection, out notFound);
            if (gameDocument is not GameModel game)
            {
                return notFound;
            }

            var newNpcList = game.NPCs.Union(npcIds);
            Response.RefreshToken(gameMasterId);
            return UpdateNpcList
            (
                gameId,
                newNpcList
            );
        }

        [HttpPut("{gameId}/{gameMasterId}/removeNpcs")]
        public ActionResult<UpdatedNpcListMessage> RemovesNPCsFromGame(Guid gameId, Guid gameMasterId)
        {
            var npcIds = GetNpcs(gameMasterId, gameId, out var notFound);
            if (npcIds == null)
            {
                return notFound;
            }

            var gameDocument = GetDocument(gameId, Collection, out notFound);
            if (gameDocument is not GameModel game)
            {
                return notFound;
            }

            var newNpcList = game.NPCs.Except(npcIds);
            Response.RefreshToken(gameMasterId);
            return UpdateNpcList
            (
                gameId,
                newNpcList
            );
        }

        [HttpDelete("{gameId}/{gameMasterId}")]
        public ActionResult<object> DeleteGame(Guid gameId, Guid gameMasterId, [FromQuery] string gameSessionPassword)
        {
            var gameDocument = GetDocument(gameId, Collection, out var notFound);
            if (gameDocument is not GameModel game)
            {
                return notFound;
            }

            var trainer = DatabaseUtility.FindTrainerById(gameMasterId, gameId);
            if (trainer?.IsGM != true)
            {
                return NotFound(gameMasterId);
            }

            if (!IsGameAuthenticated(gameSessionPassword, game, out var authError))
            {
                return authError;
            }

            return new
            {
                pokemonDeletionResult = GetMassPokemonDeletion(gameId),
                trainerDeletionResult = GetMassTrainerDeletion(gameId),
                gameDeletionResult = GetGameDeletion(gameId)
            };
        }

        [HttpDelete("{gameId}/{gameMasterId}/export")]
        public ActionResult ExportGame(Guid gameId, Guid gameMasterId, [FromQuery] string gameSessionPassword)
        {
            var game = DatabaseUtility.FindGame(gameId);
            if (game == null)
            {
                return NotFound(gameId);
            }

            var gameMaster = DatabaseUtility.FindTrainerById(gameMasterId, gameId);
            if (gameMaster?.IsGM != true)
            {
                return NotFound(gameMasterId);
            }

            if (!EncryptionUtility.VerifySecret(gameSessionPassword, game.PasswordHash))
            {
                return Unauthorized(new UnauthorizedMessage(gameId));
            }

            var exportLog = new LogModel
            {
                User = gameMaster.TrainerName,
                Action = $"exported game session at {DateTime.UtcNow}"
            };
            DatabaseUtility.UpdateGameOnlineStatus(gameId, false);
            DatabaseUtility.UpdateGameLogs(game, exportLog);
            var exportStream = ExportUtility.GetExportStream(game);

            DeleteGame(gameId, gameMasterId, gameSessionPassword);
            return File
            (
                exportStream,
                "application/octet-stream",
                $"{game.Nickname}.json"
            );
        }

        private static GameModel BuildGame(string nickname, string gameSessionPassword)
        {
            var guid = Guid.NewGuid();
            return new GameModel
            {
                GameId = guid,
                Nickname = string.IsNullOrEmpty(nickname)
                    ? guid.ToString().Split('-')[0]
                    : nickname,
                IsOnline = true,
                PasswordHash = EncryptionUtility.HashSecret(gameSessionPassword),
                NPCs = Array.Empty<Guid>(),
                Logs = Array.Empty<LogModel>()
            };
        }

        private static (TrainerModel GameMaster, AbstractMessage Message) BuildGM(
            Guid gameId,
            Guid userId,
            string username)
        {
            var (gm, badRequestMessage) = BuildTrainer
            (
                gameId,
                userId,
                username,
                true
            );

            if (gm != null)
            {
                gm.IsGM = true;
                gm.Sprite = "acetrainer";
            }

            return (gm, badRequestMessage);
        }

        private static (TrainerModel Trainer, AbstractMessage Message) BuildTrainer(
            Guid gameId,
            Guid userId,
            string username)
        {
            var (trainer, badRequestMessage) = BuildTrainer
            (
                gameId,
                userId,
                username,
                false
            );

            if (badRequestMessage != null)
            {
                return (null, badRequestMessage);
            }

            trainer.Sprite = "acetrainer";
            return (trainer, null);
        }

        private static (TrainerModel Trainer, AbstractMessage Error) BuildTrainer(
            Guid gameId,
            Guid userId,
            string username,
            bool isGM)
        {
            if (DatabaseUtility.FindTrainerByUsername(username, gameId) != null)
            {
                return (null, new GenericMessage($"Duplicate username {username}"));
            }

            var trainer = CreateTrainer(gameId, userId, username);
            trainer.IsGM = isGM;
            trainer.IsAllowed = isGM;
            return (trainer, null);
        }

        private static TrainerModel CreateTrainer(
            Guid gameId,
            Guid userId,
            string username)
        {
            var user = DatabaseUtility.FindUserById(userId);
            user.Games.Add(gameId);
            DatabaseUtility.UpdateUser(user);
            return new TrainerModel
            {
                GameId = gameId,
                TrainerId = userId,
                Honors = Array.Empty<string>(),
                TrainerName = username,
                TrainerClasses = Array.Empty<string>(),
                Feats = Array.Empty<string>(),
                IsOnline = true,
                Items = new List<ItemModel>(),
                TrainerStats = GetStats(),
                CurrentHP = 20,
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

        private IEnumerable<Guid> GetNpcs(
            Guid gameMasterId,
            Guid gameId,
            out ActionResult notFound)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                notFound = Unauthorized();
                return null;
            }

            var trainer = DatabaseUtility.FindTrainerById(gameMasterId, gameId);
            if (trainer?.IsGM != true)
            {
                notFound = NotFound(gameMasterId);
                return null;
            }

            var npcIds = Request.GetNpcIds(out var error);
            if (!npcIds.Any())
            {
                notFound = NotFound(error);
                return null;
            }

            notFound = null;
            return npcIds;
        }

        private ActionResult<UpdatedNpcListMessage> UpdateNpcList(
            Guid gameId,
            IEnumerable<Guid> newNpcList)
        {
            if (!DatabaseUtility.UpdateGameNpcList(gameId, newNpcList))
            {
                return StatusCode(500);
            }

            return new UpdatedNpcListMessage(newNpcList);
        }

        private static void SetEndGameStatuses(Guid gameId)
        {
            DatabaseUtility.UpdateGameOnlineStatus
            (
                gameId,
                false
            );

            foreach (var trainer in DatabaseUtility.FindTrainersByGameId(gameId))
            {
                DatabaseUtility.UpdateTrainerOnlineStatus
                (
                    trainer.TrainerId,
                    false
                );
            }
        }

        private static GenericMessage GetGameDeletion(Guid gameId)
        {
            string message = DatabaseUtility.DeleteGame(gameId)
                ? $"Successfully deleted game {gameId}"
                : $"Failed to delete {gameId}";
            return new GenericMessage(message);
        }

        private static GenericMessage GetMassTrainerDeletion(Guid gameId)
        {
            string message;
            if (DatabaseUtility.DeleteTrainersByGameId(gameId) > -1)
            {
                message = $"Successfully deleted all trainers associate with {gameId}";
            }
            else
            {
                message = $"Failed to delete trainers";
            }

            return new GenericMessage(message);
        }

        private static IEnumerable<GenericMessage> GetMassPokemonDeletion(Guid gameId)
        {
            return DatabaseUtility.FindTrainersByGameId(gameId)
                .Select(trainer => GetPokemonDeletion(trainer.TrainerId, trainer.GameId))
                .Where(response => response != null);
        }
    }
}
