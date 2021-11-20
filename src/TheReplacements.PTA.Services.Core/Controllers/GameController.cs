using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using TheReplacements.PTA.Common.Utilities;
using TheReplacements.PTA.Common.Models;
using TheReplacements.PTA.Common.Enums;
using System.IO;
using TheReplacements.PTA.Services.Core.Extensions;

namespace TheReplacements.PTA.Services.Core.Controllers
{
    [ApiController]
    [Route("api/v1/game")]
    public class GameController : PtaControllerBase
    {
        protected override MongoCollection Collection { get; }

        public GameController()
        {
            Collection = MongoCollection.Game;
        }

        [HttpGet("{gameId}")]
        public ActionResult<GameModel> FindGame(string gameId)
        {
            Response.UpdateAccessControl();
            var document = GetDocument(gameId, Collection, out var notFound);
            if (!(document is GameModel game))
            {
                return notFound;
            }

            return ReturnSuccessfully(game);
        }

        [HttpGet("{gameId}/find/{trainerId}")]
        public ActionResult<object> FindTrainerInGame(string gameId, string trainerId)
        {
            Response.UpdateAccessControl();
            var gameDocument = GetDocument(gameId, Collection, out var notFound);
            if (!(gameDocument is GameModel))
            {
                return notFound;
            }

            var trainerDocument = GetDocument(trainerId, MongoCollection.Trainer, out notFound);
            if (!(trainerDocument is TrainerModel trainer))
            {
                return notFound;
            }

            if (trainer.GameId != gameId)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} Game {gameId} retrieved trainer {trainerId} who had game {trainer.GameId}");
                return BadRequest(new
                {
                    message = $"{trainerId} had an invalid game id",
                    trainer.GameId
                });
            }

            return ReturnSuccessfully(new
            {
                trainer.TrainerId,
                trainer.TrainerName,
                trainer.IsGM,
                trainer.Items
            });
        }

        [HttpPost("import")]
        public object ImportGame()
        {
            Response.UpdateAccessControl();
            var json = Request.GetJsonFromRequest();
            if (string.IsNullOrEmpty(json))
            {
                return BadRequest(new
                {
                    message = "Empty json file"
                });
            }

            if (!ExportUtility.TryParseImport(json, out var errors))
            {
                return BadRequest(errors);
            }

            return ReturnSuccessfully(Ok(errors));
        }

        [HttpPost("new")]
        public ActionResult<object> CreateNewGame()
        {
            Response.UpdateAccessControl();
            var game = Request.BuildGame();
            if (!DatabaseUtility.TryAddGame(game, out var error))
            {
                return BadRequest(error);
            }

            var trainer = Request.BuildGM(game.GameId, out var badRequest);
            if (trainer == null)
            {
                return BadRequest(badRequest);
            }

            if (!DatabaseUtility.TryAddTrainer(trainer, out error))
            {
                return BadRequest(error);
            }

            Response.AssignAuthAndToken();
            return ReturnSuccessfully(new
            {
                game.GameId,
                GameMaster = new
                {
                    trainer.TrainerId,
                    trainer.TrainerName,
                    trainer.IsGM,
                    trainer.Items
                }
            });
        }

        [HttpPost("{gameId}/new")]
        public ActionResult<object> AddPlayerToGame(string gameId)
        {
            Response.UpdateAccessControl();
            var gameDocument = GetDocument(gameId, Collection, out var notFound);
            if (!(gameDocument is GameModel))
            {
                return notFound;
            }

            if (!DatabaseUtility.HasGM(gameId, out var noGMError))
            {
                return BadRequest(noGMError);
            }

            var trainer = Request.BuildTrainer(gameId, out var badRequest);
            if (trainer == null)
            {
                return BadRequest(badRequest);
            }

            if (!DatabaseUtility.TryAddTrainer(trainer, out var error))
            {
                return BadRequest(error);
            }

            Response.AssignAuthAndToken();
            return ReturnSuccessfully(new
            {
                trainer.TrainerId,
                trainer.TrainerName,
                trainer.IsGM,
                trainer.Items
            });
        }

        [HttpPost("{gameMasterId}/wild")]
        public ActionResult<PokemonModel> AddPokemon(string gameMasterId)
        {
            Response.UpdateAccessControl();
            if (!Header.VerifyCookies(Request.Cookies, gameMasterId))
            {
                return Unauthorized();
            }

            if (!IsGMOnline(gameMasterId))
            {
                return NotFound(gameMasterId);
            }

            var pokemon = Request.BuildPokemon(gameMasterId, out var error);
            if (pokemon == null)
            {
                return BadRequest(error);
            }
            if (!DatabaseUtility.TryAddPokemon(pokemon, out var writeError))
            {
                return BadRequest(writeError);
            }

            Response.RefreshToken();
            return ReturnSuccessfully(pokemon);
        }

        [HttpPut("{gameId}/start")]
        public ActionResult StartGame(string gameId)
        {
            Response.UpdateAccessControl();
            var gameDocument = GetDocument(gameId, Collection, out var notFound);
            if (!(gameDocument is GameModel game))
            {
                return notFound;
            }

            var (gamePassword, username, password) = Request.GetStartGameCredentials(out var credentialErrors);
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

            Response.AssignAuthAndToken();
            return ReturnSuccessfully(Ok());
        }

        [HttpPut("{gameId}/end")]
        public ActionResult EndGame(string gameId)
        {
            Response.UpdateAccessControl();
            var gameMasterId = Request.Query["gameMasterId"];
            if (!Header.VerifyCookies(Request.Cookies, gameMasterId))
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
            return ReturnSuccessfully(Ok());
        }

        [HttpPut("{gameId}/addNpcs")]
        public ActionResult<object> AddNPCsToGame(string gameId)
        {
            Response.UpdateAccessControl();
            var gameMasterId = Request.Query["gameMasterId"];
            if (!Header.VerifyCookies(Request.Cookies, gameMasterId))
            {
                return Unauthorized();
            }

            var gameDocument = GetDocument(gameId, Collection, out var notFound);
            if (!(gameDocument is GameModel game))
            {
                return notFound;
            }

            var trainerDocument = GetDocument(gameMasterId, MongoCollection.Trainer, out notFound);
            if (!(trainerDocument is TrainerModel trainer && trainer.IsGM))
            {
                return notFound;
            }

            var npcIds = Request.GetNpcIds(out var error);
            if (!npcIds.Any())
            {
                return NotFound(error);
            }

            var newNpcList = game.NPCs.Union(npcIds);
            if (!DatabaseUtility.UpdateGameNpcList(gameId, newNpcList))
            {
                return StatusCode(500);
            }

            Response.RefreshToken();
            return ReturnSuccessfully(new
            {
                newNpcList
            });
        }

        [HttpPut("{gameId}/removeNpcs")]
        public ActionResult<object> RemovesNPCsFromGame(string gameId)
        {
            Response.UpdateAccessControl();
            var gameMasterId = Request.Query["gameMasterId"];
            if (!Header.VerifyCookies(Request.Cookies, gameMasterId))
            {
                return Unauthorized();
            }

            var gameDocument = GetDocument(gameId, Collection, out var notFound);
            if (!(gameDocument is GameModel game))
            {
                return notFound;
            }

            var trainerDocument = GetDocument(gameMasterId, MongoCollection.Trainer, out notFound);
            if (!(trainerDocument is TrainerModel trainer && trainer.IsGM))
            {
                return notFound;
            }

            var npcIds = Request.GetNpcIds(out var error);
            if (!npcIds.Any())
            {
                return NotFound(error);
            }

            var newNpcList = game.NPCs.Except(npcIds);
            if (!DatabaseUtility.UpdateGameNpcList(gameId, newNpcList))
            {
                return StatusCode(500);
            }

            Response.RefreshToken();
            return ReturnSuccessfully(new
            {
                newNpcList
            });
        }

        [HttpPut("reset")]
        public ActionResult<object> ChangeTrainerPassword()
        {
            Response.UpdateAccessControl();
            // TODO: Pass a token to verify password reset
            var returnNotFound = true;
            if (returnNotFound)
            {
                return NotFound();
            }

            var trainerId = Request.Query["trainerId"];
            var wasUpdateSucessful = DatabaseUtility.UpdateTrainerPassword
            (
                trainerId,
                Request.Query["password"]
            );

            if (!wasUpdateSucessful)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve trainer {trainerId}");
                return NotFound();
            }

            var trainer = DatabaseUtility.FindTrainerById(trainerId);
            Response.Cookies.Append("ptaSessionAuth", Header.GetCookie());
            Response.Cookies.Append("ptaActivityToken", EncryptionUtility.GenerateToken());
            LoggerUtility.Info(Collection, $"Client {ClientIp} successfully hit {Request.Path.Value} {Request.Method} endpoint");
            return new
            {
                trainer.TrainerId,
                trainer.TrainerName,
                trainer.IsGM,
                trainer.Items
            };
        }

        [HttpDelete("{gameId}")]
        public ActionResult<object> DeleteGame(string gameId)
        {
            Response.UpdateAccessControl();
            var gameDocument = GetDocument(gameId, Collection, out var notFound);
            if (!(gameDocument is GameModel game))
            {
                return notFound;
            }

            var gameMasterId = Request.Query["gameMasterId"];
            var trainerDocument = GetDocument(gameMasterId, MongoCollection.Trainer, out notFound);
            if (!(trainerDocument is TrainerModel trainer && trainer.IsGM))
            {
                return notFound;
            }

            var gamePassword = Request.Query["gameSessionPassword"];
            if (!IsGameAuthenticated(gamePassword, game, out var authError))
            {
                return authError;
            }

            return ReturnSuccessfully(new
            {
                pokemonDeletionResult = GetMassPokemonDeletion(gameId),
                trainerDeletionResult = GetMassTrainerDeletion(gameId),
                gameDeletionResult = GetGameDeletion(gameId)
            });;
        }

        [HttpDelete("{gameId}/export")]
        public ActionResult ExportGame(string gameId)
        {
            Response.UpdateAccessControl();
            var game = DatabaseUtility.FindGame(gameId);
            if (game == null)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve game {gameId}");
                return NotFound(gameId);
            }

            var gameMasterId = Request.Query["gameMasterId"];
            var gameMaster = DatabaseUtility.FindTrainerById(gameMasterId);
            if (gameMaster?.IsGM != true)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve game master with {gameMasterId}");
                return NotFound(gameMasterId);
            }

            var gamePassword = Request.Query["gameSessionPassword"];
            if (!EncryptionUtility.VerifySecret(gamePassword, game.PasswordHash))
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to log in to PTA");
                return Unauthorized(new
                {
                    message = "Could not login in to game with provided password",
                    gameId
                });
            }

            DatabaseUtility.UpdateGameOnlineStatus(gameId, false);
            var exportStream = ExportUtility.GetExportStream(game);

            DeleteGame(gameId);
            LoggerUtility.Info(Collection, $"Client {ClientIp} successfully hit {Request.Path.Value} {Request.Method} endpoint");
            return File
            (
                exportStream,
                "application/octet-stream",
                $"{game.Nickname}.json"
            );
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

        private static object GetGameDeletion(string gameId)
        {
            return new
            {
                message = DatabaseUtility.DeleteGame(gameId)
                    ? $"Successfully deleted game {gameId}"
                    : $"Failed to delete {gameId}"
            };
        }

        private static object GetMassTrainerDeletion(string gameId)
        {
            string message;
            if (DatabaseUtility.DeleteTrainersByGameId(gameId) > -1)
            {
                message = $"Successfully deleted all trainers associate with {gameId}";
                LoggerUtility.Info(MongoCollection.Trainer, message);
            }
            else
            {
                message = $"Failed to delete trainers";
                LoggerUtility.Error(MongoCollection.Trainer, message);
            }

            return new
            {
                message
            };
        }

        private static IEnumerable<object> GetMassPokemonDeletion(string gameId)
        {
            return DatabaseUtility.FindTrainersByGameId(gameId)
                .Select(trainer => GetPokemonDeletion(trainer.TrainerId))
                .Where(response => response != null);
        }
    }
}
