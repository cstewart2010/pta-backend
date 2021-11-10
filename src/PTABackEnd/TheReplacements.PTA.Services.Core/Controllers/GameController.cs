using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
            return GetGame(gameId);
        }

        [HttpGet("{gameId}/{trainerId}")]
        public ActionResult<object> FindTrainerInGame(string gameId, string trainerId)
        {
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
                trainer.Username,
                trainer.IsGM,
                trainer.Items
            };
        }

        [HttpPost("new")]
        public ActionResult<GameModel> CreateNewGame()
        {
            var game = new GameModel
            {
                GameId = Guid.NewGuid().ToString()
            };
            DatabaseUtility.AddGame(game);
            return GetGame(game.GameId);
        }

        [HttpPost("{gameId}/new")]
        public ActionResult<object> AddPlayerToGame(string gameId)
        {
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

            var username = Request.Query["username"];
            Expression<Func<TrainerModel, bool>> filter = trainer => trainer.Username == username;
            if (DatabaseUtility.FindTrainers(filter).Any())
            {
                return BadRequest(new
                {
                    message = "Duplicate username",
                    gameId,
                    username
                });
            }

            var trainer = CreateTrainer
            (
                gameId,
                username
            );
            if (trainer == null)
            {
                return BadRequest(new
                {
                    message = "Missing password",
                    gameId,
                    username
                });
            }

            DatabaseUtility.AddTrainer(trainer);
            return new
            {
                trainer.TrainerId,
                trainer.Username,
                trainer.IsGM,
                trainer.Items
            };
        }

        [HttpPost("{gameId}/gm")]
        public ActionResult<object> AddGMToGame(string gameId)
        {
            if (DatabaseUtility.FindGame(gameId) == null)
            {
                return NotFound(gameId);
            }

            if (DatabaseUtility.HasGM(gameId))
            {
                return BadRequest(new
                {
                    message = "There is already a GM",
                    gameId
                });
            }

            var username = Request.Query["username"];
            var trainer = CreateTrainer
            (
                gameId,
                username
            );
            if (trainer == null)
            {
                return BadRequest(new
                {
                    message = "Missing password",
                    gameId,
                    username
                });
            }

            trainer.IsGM = true;
            DatabaseUtility.AddTrainer(trainer);
            return new
            {
                trainer.TrainerId,
                trainer.Username,
                trainer.IsGM,
                trainer.Items
            };
        }

        [HttpPut("{gameId}/reset")]
        public ActionResult<object> ChangeTrainerPassword(string gameId)
        {
            var username = Request.Query["username"];
            Expression<Func<TrainerModel, bool>> filter = trainer => trainer.Username == username && trainer.GameId == gameId;
            var update = Builders<TrainerModel>
                .Update
                .Set
                (
                    "PasswordHash",
                    GetHashPassword(Request.Query["password"])
                );
            var trainer = DatabaseUtility.UpdateTrainer
            (
                filter,
                update
            );
            if (trainer != null)
            {
                return new
                {
                    trainer.TrainerId,
                    trainer.Username,
                    trainer.IsGM,
                    trainer.Items
                };
            }

            return NotFound();
        }

        [HttpDelete("{gameId}")]
        public ActionResult<object> DeleteGame(string gameId)
        {
            Expression<Func<TrainerModel, bool>> filter = trainer => trainer.GameId == gameId;
            var trainers = DatabaseUtility.FindTrainers(filter);
            var results = trainers.Select(trainer => DatabaseUtility.DeleteTrainerMons(trainer.TrainerId)).ToList();
            results.Add(new
            {
                message = DeleteTrainers(filter),
                gameId = gameId
            });
            if (!DatabaseUtility.DeleteGame(gameId))
            {
                return NotFound(gameId);
            }

            return results;
        }

        private string DeleteTrainers(Expression<Func<TrainerModel, bool>> filter)
        {
            string message;
            if (DatabaseUtility.DeleteTrainers(filter))
            {
                message = $"Successfully deleted all trainers";
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

        private TrainerModel CreateTrainer(
            string gameId,
            string username)
        {
            foreach (var key in new[] { "username", "password" })
            {
                if (string.IsNullOrWhiteSpace(Request.Query[key]))
                {
                    return null;
                }
            }

            return new TrainerModel
            {
                GameId = gameId,
                TrainerId = Guid.NewGuid().ToString(),
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Request.Query["password"]),
                Items = new List<ItemModel>()
            };
        }

        private string GetHashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}
