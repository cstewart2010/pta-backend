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

        [HttpGet("{id}")]
        public ActionResult<GameModel> Get(string id)
        {
            return GetGame(id);
        }

        [HttpGet("{gameId}/{id}")]
        public ActionResult<object> GetTrainer(string gameId, string id)
        {
            if (DatabaseUtility.FindGame(gameId) == null)
            {
                return NotFound(gameId);
            }

            var trainer = DatabaseUtility.FindTrainerById(id);
            if (trainer == null)
            {
                return NotFound(id);
            }

            if (trainer.GameId != gameId)
            {
                return BadRequest(new
                {
                    message = $"{id} had an invalid game id",
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

        [HttpPost]
        public ActionResult<GameModel> Post()
        {
            var id = Guid.NewGuid().ToString();
            DatabaseUtility.AddGame(id);
            return GetGame(id);
        }

        [HttpDelete("{id}")]
        public ActionResult<object> DeleteAsync(string id)
        {
            var gameResult = DatabaseUtility
                .TableHelper
                .Game
                .DeleteOne(game => game.GameId == id);
            if (!gameResult.IsAcknowledged)
            {
                NotFound(id);
            }

            Expression<Func<TrainerModel, bool>> filter = trainer => trainer.GameId == id;
            var trainers = DatabaseUtility.FindTrainers(filter);
            var results = new List<object>()
            {
                new
                {
                    message = DeleteTrainers(filter),
                    gameId = id
                }
            };
            results.AddRange(trainers.Select(DatabaseUtility.DeletePokemon));

            return results;
        }

        private string DeleteTrainers(Expression<Func<TrainerModel, bool>> filter)
        {
            string message;
            if (DatabaseUtility.TableHelper.Trainer.DeleteMany(filter).IsAcknowledged)
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
    }
}
