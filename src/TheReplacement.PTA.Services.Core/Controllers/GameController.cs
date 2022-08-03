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

        [HttpGet]
        public IEnumerable FindAllGames()
        {
            if (Request.Query.TryGetValue("nickname", out var nickname))
            {
                return DatabaseUtility.FindAllGames(nickname)
                    .Select(game => new FoundGameMessage(game.GameId, game.Nickname));
            }

            return DatabaseUtility.FindMostRecent20Games();
        }

        [HttpGet("sprites/all")]
        public IEnumerable<SpriteModel> GetAllSprites()
        {
            return DatabaseUtility.GetAllSprites();
        }

        [HttpGet("{gameId}")]
        public ActionResult<FoundGameMessage> FindGame(string gameId)
        {
            if (string.IsNullOrEmpty(gameId))
            {
                return BadRequest(nameof(gameId));
            }

            var document = GetDocument(gameId, Collection, out var notFound);
            if (!(document is GameModel))
            {
                return notFound;
            }

            return new FoundGameMessage(gameId);
        }

        [HttpGet("{gameId}/find/{trainerId}")]
        public ActionResult<FoundTrainerMessage> FindTrainerInGame(string gameId, string trainerId)
        {
            if (string.IsNullOrEmpty(gameId))
            {
                return BadRequest(nameof(gameId));
            }

            if (string.IsNullOrEmpty(trainerId))
            {
                return BadRequest(nameof(trainerId));
            }

            var gameDocument = GetDocument(gameId, Collection, out var notFound);
            if (!(gameDocument is GameModel))
            {
                return notFound;
            }

            var trainerDocument = GetDocument(trainerId, MongoCollection.Trainers, out notFound);
            if (trainerDocument is not TrainerModel trainer)
            {
                return notFound;
            }

            if (trainer.GameId != gameId)
            {
                return BadRequest(new InvalidGameIdMessage(trainer));
            }

            return new FoundTrainerMessage(trainer);
        }

        [HttpGet("{gameId}/all_logs")]
        public ActionResult<AllLogsMessage> GetAllLogs(string gameId)
        {
            if (string.IsNullOrEmpty(gameId))
            {
                return BadRequest(nameof(gameId));
            }

            var gameDocument = GetDocument(gameId, Collection, out var notFound);
            if (gameDocument is not GameModel game)
            {
                return notFound;
            }

            return new AllLogsMessage(game);
        }

        [HttpGet("{gameId}/logs")]
        public ActionResult<IEnumerable<LogModel>> GetLogs(string gameId)
        {
            if (string.IsNullOrEmpty(gameId))
            {
                return BadRequest(nameof(gameId));
            }

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

        [HttpPost("new")]
        public async Task<ActionResult<CreatedGameMessage>> CreateNewGame()
        {
            var game = Request.BuildGame();
            if (!DatabaseUtility.TryAddGame(game, out var error))
            {
                return BadRequest(error);
            }

            var (trainer, badRequest) = await Request.BuildGM(game.GameId);
            if (trainer == null)
            {
                return BadRequest(badRequest);
            }

            if (!DatabaseUtility.TryAddTrainer(trainer, out error))
            {
                return BadRequest(error);
            }

            var gameCreationLog = new LogModel
            {
                User = trainer.TrainerName,
                Action = $"created a new game and joined as game master at {DateTime.UtcNow}"
            };
            DatabaseUtility.UpdateGameLogs(game, gameCreationLog);
            Response.AssignAuthAndToken(trainer.TrainerId);
            return new CreatedGameMessage(trainer);
        }

        [HttpPost("{gameId}/new")]
        public async Task<ActionResult<FoundTrainerMessage>> AddPlayerToGame(string gameId)
        {
            if (string.IsNullOrEmpty(gameId))
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

            var (trainer, badRequest) = await Request.BuildTrainer(gameId);
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
            Response.AssignAuthAndToken(trainer.TrainerId);
            return new FoundTrainerMessage(trainer);
        }

        [HttpPost("{gameMasterId}/wild")]
        public async Task<ActionResult<PokemonModel>> AddPokemon(string gameMasterId)
        {
            if (string.IsNullOrEmpty(gameMasterId))
            {
                return BadRequest(nameof(gameMasterId));
            }

            if (!Request.VerifyIdentity(gameMasterId, true))
            {
                return Unauthorized();
            }

            var (pokemon, error) = await Request.BuildPokemon(gameMasterId);
            if (pokemon == null)
            {
                return BadRequest(error);
            }
            if (!DatabaseUtility.TryAddPokemon(pokemon, out var writeError))
            {
                return BadRequest(writeError);
            }

            var gameMaster = DatabaseUtility.FindTrainerById(gameMasterId);
            var game = DatabaseUtility.FindGame(gameMaster.GameId);
            var pokemonCreationLog = new LogModel
            {
                User = pokemon.SpeciesName,
                Action = $"spawned at {DateTime.UtcNow}"
            };
            DatabaseUtility.UpdateGameLogs(game, pokemonCreationLog);
            Response.RefreshToken(gameMasterId);
            return pokemon;
        }

        [HttpPost("{gameId}/log/{trainerId}")]
        public async Task<ActionResult<LogModel>> PostLogAsync(string gameId, string trainerId)
        {
            var body = await Request.GetRequestBody();
            if (string.IsNullOrEmpty(trainerId))
            {
                return BadRequest(nameof(trainerId));
            }

            if (!Request.VerifyIdentity(trainerId, false))
            {
                return Unauthorized();
            }

            var log = body.ToObject<LogModel>();
            log.Action += $" at {DateTime.UtcNow}";
            DatabaseUtility.UpdateGameLogs(DatabaseUtility.FindGame(gameId), log);
            Response.RefreshToken(trainerId);
            return log;
        }

        [HttpPut("{trainerId}/addStats")]
        public async Task<ActionResult<FoundTrainerMessage>> AddTrainerStats(string trainerId)
        {
            if (!Request.VerifyIdentity(trainerId, false))
            {
                return Unauthorized();
            }

            var result = await Request.TryCompleteTrainer();
            if (!result)
            {
                return BadRequest(new GenericMessage("Failed to update trainer"));
            }
            var trainer = DatabaseUtility.FindTrainerById(trainerId);
            return new FoundTrainerMessage(trainer);
        }

        [HttpPut("{gameId}/start")]
        public async Task<ActionResult<FoundGameMessage>> StartGame(string gameId)
        {
            if (string.IsNullOrEmpty(gameId))
            {
                return BadRequest(nameof(gameId));
            }

            var gameDocument = GetDocument(gameId, Collection, out var notFound);
            if (gameDocument is not GameModel game)
            {
                return notFound;
            }

            var (gamePassword, username, password, credentialErrors) = await Request.GetStartGameCredentials();
            if (credentialErrors.Any())
            {
                return BadRequest(credentialErrors);
            }

            if (!IsGameAuthenticated(gamePassword, game, out var authError))
            {
                return authError;
            }

            if (!IsTrainerAuthenticated(username, password, gameId, true, out authError))
            {
                return authError;
            }

            var trainer = DatabaseUtility.FindTrainerByUsername(username, gameId);
            Response.AssignAuthAndToken(trainer.TrainerId);
            return new FoundGameMessage(gameId);
        }

        [HttpPut("{gameId}/end")]
        public ActionResult EndGame(string gameId)
        {
            if (string.IsNullOrEmpty(gameId))
            {
                return BadRequest(nameof(gameId));
            }

            if (!Request.Query.TryGetValue("gameMasterId", out var gameMasterId))
            {
                return BadRequest(nameof(gameMasterId));
            }
            if (!Request.VerifyIdentity(gameMasterId, true))
            {
                return Unauthorized();
            }

            var gameDocument = GetDocument(gameId, Collection, out var notFound);
            if (!(gameDocument is GameModel))
            {
                return notFound;
            }

            if (!IsGMOnline(gameMasterId))
            {
                return NotFound(gameMasterId);
            }

            SetEndGameStatuses(gameId);
            return Ok();
        }

        [HttpPut("{gameId}/addNpcs")]
        public ActionResult<UpdatedNpcListMessage> AddNPCsToGame(string gameId)
        {
            if (string.IsNullOrEmpty(gameId))
            {
                return BadRequest(nameof(gameId));
            }


            if (!Request.Query.TryGetValue("gameMasterId", out var gameMasterId))
            {
                return BadRequest(nameof(gameMasterId));
            }
            var npcIds = GetNpcs(gameMasterId, out var notFound);
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

        [HttpPut("{gameId}/removeNpcs")]
        public ActionResult<UpdatedNpcListMessage> RemovesNPCsFromGame(string gameId)
        {
            if (string.IsNullOrEmpty(gameId))
            {
                return BadRequest(nameof(gameId));
            }

            if (!Request.Query.TryGetValue("gameMasterId", out var gameMasterId))
            {
                return BadRequest(nameof(gameMasterId));
            }
            var npcIds = GetNpcs(gameMasterId, out var notFound);
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

        [HttpPut("reset")]
        public ActionResult<object> ChangeTrainerPassword()
        {
            var returnNotFound = true;
            if (returnNotFound)
            {
                return NotFound();
            }

            if (!Request.Query.TryGetValue("trainerId", out var trainerId))
            {
                return BadRequest(nameof(trainerId));
            }

            if (!Request.Query.TryGetValue("password", out var password))
            {
                return BadRequest(nameof(password));
            }

            var wasUpdateSucessful = DatabaseUtility.UpdateTrainerPassword
            (
                trainerId,
                password
            );

            if (!wasUpdateSucessful)
            {
                return NotFound();
            }

            var trainer = DatabaseUtility.FindTrainerById(trainerId);
            Response.AssignAuthAndToken(trainer.TrainerId);
            return new FoundTrainerMessage(trainer);
        }

        [HttpDelete("{gameId}")]
        public ActionResult<object> DeleteGame(string gameId)
        {
            if (string.IsNullOrEmpty(gameId))
            {
                return BadRequest(nameof(gameId));
            }

            var gameDocument = GetDocument(gameId, Collection, out var notFound);
            if (gameDocument is not GameModel game)
            {
                return notFound;
            }

            if (!Request.Query.TryGetValue("gameMasterId", out var gameMasterId))
            {
                return BadRequest(nameof(gameMasterId));
            }
            var trainerDocument = GetDocument(gameMasterId, MongoCollection.Trainers, out notFound);
            if (!(trainerDocument is TrainerModel trainer && trainer.IsGM))
            {
                return notFound;
            }

            if (!Request.Query.TryGetValue("gameSessionPassword", out var gameSessionPassword))
            {
                return BadRequest(nameof(gameSessionPassword));
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

        [HttpDelete("{gameId}/export")]
        public ActionResult ExportGame(string gameId)
        {
            if (string.IsNullOrEmpty(gameId))
            {
                return BadRequest(nameof(gameId));
            }

            var game = DatabaseUtility.FindGame(gameId);
            if (game == null)
            {
                return NotFound(gameId);
            }

            if (!Request.Query.TryGetValue("gameMasterId", out var gameMasterId))
            {
                return BadRequest(nameof(gameMasterId));
            }
            var gameMaster = DatabaseUtility.FindTrainerById(gameMasterId);
            if (gameMaster?.IsGM != true)
            {
                return NotFound(gameMasterId);
            }

            if (!Request.Query.TryGetValue("gameSessionPassword", out var gameSessionPassword))
            {
                return BadRequest(nameof(gameSessionPassword));
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

            DeleteGame(gameId);
            return File
            (
                exportStream,
                "application/octet-stream",
                $"{game.Nickname}.json"
            );
        }

        private IEnumerable<string> GetNpcs(
            string gameMasterId,
            out ActionResult notFound)
        {
            if (!Request.VerifyIdentity(gameMasterId, true))
            {
                notFound = Unauthorized();
                return null;
            }

            var trainerDocument = GetDocument(gameMasterId, MongoCollection.Trainers, out notFound);
            if (!(trainerDocument is TrainerModel trainer && trainer.IsGM))
            {
                return null;
            }

            var npcIds = Request.GetNpcIds(out var error);
            if (!npcIds.Any())
            {
                notFound = NotFound(error);
                return null;
            }

            return npcIds;
        }

        private ActionResult<UpdatedNpcListMessage> UpdateNpcList(
            string gameId,
            IEnumerable<string> newNpcList)
        {
            if (!DatabaseUtility.UpdateGameNpcList(gameId, newNpcList))
            {
                return StatusCode(500);
            }

            return new UpdatedNpcListMessage(newNpcList);
        }

        private static void SetEndGameStatuses(string gameId)
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

        private static GenericMessage GetGameDeletion(string gameId)
        {
            string message = DatabaseUtility.DeleteGame(gameId)
                ? $"Successfully deleted game {gameId}"
                : $"Failed to delete {gameId}";
            return new GenericMessage(message);
        }

        private static GenericMessage GetMassTrainerDeletion(string gameId)
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

        private static IEnumerable<GenericMessage> GetMassPokemonDeletion(string gameId)
        {
            return DatabaseUtility.FindTrainersByGameId(gameId)
                .Select(trainer => GetPokemonDeletion(trainer.TrainerId))
                .Where(response => response != null);
        }
    }
}
