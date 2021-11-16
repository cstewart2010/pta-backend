using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using TheReplacements.PTA.Common.Utilities;
using TheReplacements.PTA.Common.Models;
using TheReplacements.PTA.Common.Enums;

namespace TheReplacements.PTA.Services.Core.Controllers
{
    [ApiController]
    [Route("api/v1/game")]
    public class GameController : ControllerBase
    {
        private const MongoCollection Collection = MongoCollection.Game;
        private readonly ILogger<GameController> _logger;

        public GameController(ILogger<GameController> logger)
        {
            _logger = logger;
        }

        private string ClientIp => Request.HttpContext.Connection.RemoteIpAddress.ToString();

        [HttpGet("{gameId}")]
        public ActionResult<GameModel> FindGame(string gameId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            var game = GetGame(gameId);
            if (game == null)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve game {gameId}");
                return NotFound(gameId);
            }

            return game;
        }

        [HttpGet("{gameId}/{trainerId}")]
        public ActionResult<object> FindTrainerInGame(string gameId, string trainerId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            if (DatabaseUtility.FindGame(gameId) == null)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve game {gameId}");
                return NotFound(gameId);
            }

            var trainer = DatabaseUtility.FindTrainerById(trainerId);
            if (trainer == null)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} Failed to retrieve trainer {trainerId}");
                return NotFound(trainerId);
            }

            if (trainer.GameId != gameId)
            {
                LoggerUtility.Error(Collection, $"{ClientIp}: Game {gameId} retrieved trainer {trainerId} who had game {trainer.GameId}");
                return BadRequest(new
                {
                    message = $"{trainerId} had an invalid game id",
                    trainer.GameId
                });
            }

            return new
            {
                trainer.TrainerId,
                trainer.TrainerName,
                trainer.IsGM,
                trainer.Items
            };
        }

        [HttpPost("new")]
        public ActionResult<object> CreateNewGame()
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            var guid = Guid.NewGuid().ToString();
            var game = new GameModel
            {
                GameId = guid,
                Nickname = Request.Query["nickname"].ToString() ?? guid.Split('-')[0],
                IsOnline = true,
                PasswordHash = DatabaseUtility.HashPassword(Request.Query["gameSessionPassword"])
            };
            if (!DatabaseUtility.TryAddGame(game, out var error))
            {
                return BadRequest(error);
            }

            var username = Request.Query["gmUsername"].ToString();
            if (string.IsNullOrWhiteSpace(username))
            {
                return BadRequest(new
                {
                    message = "Missing gmName",
                    game.GameId
                });
            }

            var password = Request.Query["gmPassword"].ToString();
            if (string.IsNullOrWhiteSpace(username))
            {
                return BadRequest(new
                {
                    message = "Missing gmPassword",
                    game.GameId
                });
            }

            var trainer = CreateTrainer(game.GameId, username, password);
            trainer.IsGM = true;
            if (DatabaseUtility.TryAddTrainer(trainer, out error))
            {
                return new
                {
                    game.GameId,
                    GameMaster = new
                    {
                        trainer.TrainerId,
                        trainer.TrainerName,
                        trainer.IsGM,
                        trainer.Items
                    }
                };
            }

            return BadRequest(error);
        }

        [HttpPost("{gameId}/new")]
        public ActionResult<object> AddPlayerToGame(string gameId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            if (DatabaseUtility.FindGame(gameId) == null)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve game {gameId}");
                return NotFound(gameId);
            }

            if (!DatabaseUtility.HasGM(gameId))
            {
                return BadRequest(new
                {
                    message = "No GM has been made",
                    gameId
                });
            }

            var trainerName = Request.Query["trainerName"].ToString();
            if (string.IsNullOrWhiteSpace(trainerName))
            {
                return BadRequest(new
                {
                    message = "Missing trainerName",
                    gameId
                });
            }

            var password = Request.Query["password"].ToString();
            if (string.IsNullOrWhiteSpace(trainerName))
            {
                return BadRequest(new
                {
                    message = "Missing password",
                    gameId
                });
            }

            var trainer = CreateTrainer(gameId, trainerName, password);
            if (DatabaseUtility.FindTrainerByUsername(trainerName, gameId) != null)
            {
                return BadRequest(new
                {
                    message = "Duplicate trainerName",
                    gameId,

                });
            }

            if (DatabaseUtility.TryAddTrainer(trainer, out var error))
            {
                return new
                {
                    trainer.TrainerId,
                    trainer.TrainerName,
                    trainer.IsGM,
                    trainer.Items
                };
            }

            return BadRequest(error);
        }

        [HttpPut("{gameId}/start")]
        public ActionResult StartGame(string gameId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            var gamePassword = Request.Query["gameSessionPassword"];

            var game = DatabaseUtility.FindGame(gameId);
            if (game == null)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve game {gameId}");
                return NotFound(gameId);
            }

            if (game.IsOnline)
            {
                return BadRequest(new
                {
                    message = "This game is already online",
                    gameId
                });
            }
            if (!BCrypt.Net.BCrypt.Verify(gamePassword, game.PasswordHash))
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to log in to PTA");
                return Unauthorized(new
                {
                    message = "Could not login in to game with provided password",
                    gameId
                });
            }

            var username = Request.Query["gmUsername"].ToString();
            if (string.IsNullOrWhiteSpace(username))
            {
                return BadRequest(new
                {
                    message = "Missing gmName",
                    game.GameId
                });
            }

            var password = Request.Query["gmPassword"].ToString();
            if (string.IsNullOrWhiteSpace(username))
            {
                return BadRequest(new
                {
                    message = "Missing gmPassword",
                    game.GameId
                });
            }

            var gameMaster = DatabaseUtility.FindTrainerByUsername(username, gameId);
            if (gameMaster == null)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to log in to PTA");
                return Unauthorized(new
                {
                    message = "No username found with provided",
                    username
                });
            }

            if (!BCrypt.Net.BCrypt.Verify(password, game.PasswordHash))
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to log in to PTA");
                return Unauthorized(new
                {
                    message = "Invalid password",
                    password
                });
            }

            if (!gameMaster.IsGM)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to log in to PTA");
                return Unauthorized(new
                {
                    message = $"This user is not the GM for {game.Nickname}",
                    username,
                    gameId
                });
            }

            DatabaseUtility.UpdateTrainerOnlineStatus(gameMaster.TrainerId, true);
            DatabaseUtility.UpdateGameOnlineStatus(game.GameId, true);
            return Ok();
        }

        [HttpPut("{gameId}/end")]
        public ActionResult EndGame(string gameId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            var trainerId = Request.Query["trainerId"];
            var game = DatabaseUtility.FindGame(gameId);
            if (game == null)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve game {gameId}");
                return NotFound(gameId);
            }

            var gameMaster = DatabaseUtility.FindTrainerById(trainerId);
            if (gameMaster == null)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve trainer {trainerId}");
                return NotFound(trainerId);
            }

            if (!gameMaster.IsGM)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to log in to PTA");
                return Unauthorized(new
                {
                    message = $"This user is not the GM for {game.Nickname}",
                    trainerId,
                    gameId
                });
            }

            DatabaseUtility.UpdateGameOnlineStatus
            (
                game.GameId,
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

            return Ok();
        }

        [HttpPut("{gameId}/addNpcs")]
        public ActionResult<object> AddNPCsToGame(string gameId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            var game = DatabaseUtility.FindGame(gameId);
            if (game == null)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve game {gameId}");
                return NotFound(gameId);
            }

            var npcIds = Request.Query["npcIds"].ToString().Split(',');
            if (npcIds.Length < 1)
            {
                return BadRequest(new
                {
                    message = "No npc Ids provided"
                });
            }

            var npcs = DatabaseUtility.FindNpcs(npcIds);
            if (!npcs.Any())
            {
                LoggerUtility.Error(MongoCollection.Npc, $"Client {ClientIp} failed to retrieve npcs {Request.Query["npcIds"]}");
                return NotFound(npcIds);
            }

            var newNpcList = game.NPCs.ToList();
            newNpcList.AddRange(npcs.Select(npc => npc.NPCId));
            if (DatabaseUtility.UpdateGameNpcList(gameId, newNpcList))
            {
                return new
                {
                    updatedNpcList = newNpcList
                };
            }

            return StatusCode(500);
        }

        [HttpPut("{gameId}/removeNpcs")]
        public ActionResult<object> RemovesNPCsFromGame(string gameId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            var game = DatabaseUtility.FindGame(gameId);
            if (game == null)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve game {gameId}");
                return NotFound(gameId);
            }

            var npcIds = Request.Query["npcIds"].ToString().Split(',');
            if (npcIds.Length < 1)
            {
                return BadRequest(new
                {
                    message = "No npc Ids provided"
                });
            }

            var npcs = DatabaseUtility.FindNpcs(npcIds);
            if (!npcs.Any())
            {
                LoggerUtility.Error(MongoCollection.Npc, $"Client {ClientIp} failed to retrieve npcs {Request.Query["npcIds"]}");
                return NotFound(npcIds);
            }

            var newNpcList = game.NPCs.ToList().Except(npcs.Select(npc => npc.NPCId));
            if (DatabaseUtility.UpdateGameNpcList(gameId, newNpcList))
            {
                return new
                {
                    updatedNpcList = newNpcList
                };
            }

            return StatusCode(500);
        }

        [HttpPut("reset")]
        public ActionResult<object> ChangeTrainerPassword()
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            var trainerId = Request.Query["trainerId"];
            var wasUpdateSucessful = DatabaseUtility.UpdateTrainerPassword
            (
                trainerId,
                Request.Query["password"]
            );

            if (wasUpdateSucessful)
            {
                var trainer = DatabaseUtility.FindTrainerById(trainerId);
                return new
                {
                    trainer.TrainerId,
                    trainer.TrainerName,
                    trainer.IsGM,
                    trainer.Items
                };
            }

            LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve trainer {trainerId}");
            return NotFound();
        }

        [HttpDelete("{gameId}")]
        public ActionResult<object> DeleteGame(string gameId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            if (DatabaseUtility.FindGame(gameId) == null)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve game {gameId}");
                return NotFound(gameId);
            }

            var results = DatabaseUtility.FindTrainersByGameId(gameId)
                .Select
                (
                    trainer =>
                    {
                        if (DatabaseUtility.DeletePokemonByTrainerId(trainer.TrainerId) > -1)
                        {
                            return new
                            {
                                message = $"Successfully deleted all pokemon associated with {trainer.TrainerId}"
                            };
                        }
                        return null;
                    }
                )
                .Where(response => response != null)
                .ToList();

            var trainerDeletionResult = new
            {
                message = DeleteTrainers(gameId)
            };
            var gameDeletionResult = new
            {
                message = DatabaseUtility.DeleteGame(gameId)
                    ? $"Successfully deleted game {gameId}"
                    : $"Failed to delete {gameId}"
            };

            return new
            {
                gameDeletionResult,
                trainerDeletionResult,
                pokemonDeletionResult = results
            };
        }

        private string DeleteTrainers(string gameId)
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

            return message;
        }

        private ActionResult<GameModel> GetGame(string id)
        {
            var game = DatabaseUtility.FindGame(id);
            if (game == null)
            {
                return NotFound(id);
            }
            return game;
        }

        private TrainerModel CreateTrainer(
            string gameId,
            string username,
            string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            return new TrainerModel
            {
                GameId = gameId,
                TrainerId = Guid.NewGuid().ToString(),
                TrainerName = Request.Query["trainerName"],
                PasswordHash = DatabaseUtility.HashPassword(Request.Query["password"]),
                Items = new List<ItemModel>(),
                IsOnline = true,
                TrainerClasses = new List<string>(),
                TrainerStats = new TrainerStatsModel(),
                Level = 1,
                Feats = new List<string>()
            };
        }
    }
}
