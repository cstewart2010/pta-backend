﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using TheReplacements.PTA.Common.Utilities;
using TheReplacements.PTA.Common.Models;

namespace TheReplacements.PTA.Services.Core.Controllers
{
    [ApiController]
    [Route("api/v1/game")]
    public class GameController : ControllerBase
    {
        private readonly ILogger<GameController> _logger;

        public GameController(ILogger<GameController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{gameId}")]
        public ActionResult<GameModel> FindGame(string gameId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            var game = GetGame(gameId);
            if (game == null)
            {
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
                return NotFound(gameId);
            }

            var trainer = DatabaseUtility.FindTrainerById(trainerId);
            if (trainer == null)
            {
                return NotFound(trainerId);
            }

            if (trainer.GameId != gameId)
            {
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
                //GameId = guid,
                Nickname = Request.Query["nickname"].ToString() ?? guid.Split('-')[0],
                IsOnline = true,
                PasswordHash = DatabaseUtility.HashPassword(Request.Query["gameSessionPassword"])
            };
            if (!DatabaseUtility.TryAddGame(game, out var error))
            {
                return BadRequest(error);
            }            

            var username = Request.Query["gmName"];
            var trainer = CreateTrainer(game.GameId);
            if (trainer == null)
            {
                return BadRequest(new
                {
                    message = "Missing password",
                    game.GameId,
                    username
                });
            }

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


            var trainerName = Request.Query["trainerName"];
            if (DatabaseUtility.FindTrainerByUsername(trainerName, gameId) != null)
            {
                return BadRequest(new
                {
                    message = "Duplicate trainerName",
                    gameId,

                });
            }
                        
            var trainer = CreateTrainer(gameId);
            if (trainer == null)
            {
                return BadRequest(new
                {
                    message = "Missing password",
                    gameId,
                    trainerName
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
                return Unauthorized(new
                {
                    message = "Could not login in to game with provided password",
                    gameId
                });
            }
            var username = Request.Query["gmUsername"];
            var password = Request.Query["gmPassword"];

            var gameMaster = DatabaseUtility.FindTrainerByUsername(username, gameId);
            if (gameMaster == null)
            {
                return NotFound(new
                {
                    message = "No username found with provided",
                    username
                });
            }

            if (!BCrypt.Net.BCrypt.Verify(password, game.PasswordHash))
            {
                return Unauthorized(new
                {
                    message = "Invalid password",
                    password
                });
            }

            if (!gameMaster.IsGM)
            {
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
            var gameMaster = DatabaseUtility.FindTrainerById(trainerId);
            if (gameMaster == null)
            {
                return NotFound(new
                {
                    message = "No trainerId found with provided",
                    trainerId
                });
            }

            if (!gameMaster.IsGM)
            {
                return Unauthorized(new
                {
                    message = $"This user is not the GM for {game.Nickname}",
                    trainerId,
                    gameId
                });
            }

            DatabaseUtility.UpdateGameOnlineStatus(game.GameId, false);
            foreach (var trainer in DatabaseUtility.FindTrainersByGameId(gameId))
            {
                DatabaseUtility.UpdateTrainerOnlineStatus(trainer.TrainerId, false);
            }

            return Ok();
        }

        [HttpPut("{gameId}/addNpcs")]
        public ActionResult<object> AddNPCsToGame(string gameId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            var game = DatabaseUtility.FindGame(gameId);
            var npcIds = Request.Query["npcIds"].ToString()?.Split(',');
            if ((npcIds?.Length).GetValueOrDefault() < 1)
            {
                return BadRequest(new
                {
                    message = "No npc Ids provided"
                });
            }
            var npcs = DatabaseUtility.FindNpcs(npcIds);
            if (!npcs.Any())
            {
                return BadRequest(new
                {
                    message = "No valid npc Ids provided"
                });
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
            var npcIds = Request.Query["npcIds"].ToString()?.Split(',');
            if ((npcIds?.Length).GetValueOrDefault() < 1)
            {
                return BadRequest(new
                {
                    message = "No npc Ids provided"
                });
            }
            var npcs = DatabaseUtility.FindNpcs(npcIds);
            if (!npcs.Any())
            {
                return BadRequest(new
                {
                    message = "No valid npc Ids provided"
                });
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

        [HttpPut("{gameId}/reset")]
        public ActionResult<object> ChangeTrainerPassword(string gameId)
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

            return NotFound();
        }

        [HttpDelete("{gameId}")]
        public ActionResult<object> DeleteGame(string gameId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            var trainers = DatabaseUtility.FindTrainersByGameId(gameId);
            var results = trainers.Select
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
            results.Add(new
            {
                message = DeleteTrainers(gameId)
            });
            results.Add(new
            {
                message = DatabaseUtility.DeleteGame(gameId)
                    ? $"Successfully deleted game {gameId}"
                    : $"Failed to delete {gameId}"
            });

            return results;
        }

        private string DeleteTrainers(string gameId)
        {
            string message;
            if (DatabaseUtility.DeleteTrainersByGameId(gameId) > -1)
            {
                message = $"Successfully deleted all trainers associate with {gameId}";
                _logger.LogInformation(message);
            }
            else
            {
                message = $"Failed to delete trainers";
                _logger.LogWarning(message);
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

        private TrainerModel CreateTrainer(string gameId)
        {
            if (string.IsNullOrWhiteSpace(Request.Query["password"]))
            {
                return null;
            }

            return new TrainerModel
            {
                GameId = gameId,
                TrainerId = Guid.NewGuid().ToString(),
                TrainerName = Request.Query["trainerName"],
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Request.Query["password"]),
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