using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheReplacements.PTA.Common;
using TheReplacements.PTA.Common.Databases;
using TheReplacements.PTA.Common.Models;
using TheReplacements.PTA.Services.Core.Internal;

namespace TheReplacements.PTA.Services.Core.Controllers
{
    [ApiController]
    [Route("api/v1/trainer")]
    public class TrainerContoller : ControllerBase
    {
        private readonly ILogger<TrainerContoller> _logger;

        public TrainerContoller(ILogger<TrainerContoller> logger)
        {
            _logger = logger;
        }

        [HttpGet("{gameId}/{id}")]
        public ActionResult<Trainer> Get(string gameId, string id)
        {
            if (!TableHelpers.GameTable.Collection.Find(game => game.GameId == gameId).Any())
            {
                return NotFound(gameId);
            }

            var trainer = TableHelpers.TrainerTable.Collection.Find(trainer => trainer.TrainerId == id).FirstOrDefault();
            if (trainer == null)
            {
                return NotFound(id);
            }

            if (trainer.GameId != gameId)
            {
                return BadRequest();
            }

            return trainer;
        }

        [HttpPost("{gameId}")]
        public ActionResult<Trainer> PostPlayer(string gameId)
        {
            if (!TableHelpers.GameTable.Collection.Find(game => game.GameId == gameId).Any())
            {
                return NotFound(gameId);
            }
            if (!TableHelpers.TrainerTable.Collection.Find(trainer => trainer.IsGM).Any())
            {
                return BadRequest(new
                {
                    message = "No GM has been made"
                });
            }

            var username = Request.Query["username"];
            if (TableHelpers.TrainerTable.Collection.Find(trainer => trainer.Username == username).Any())
            {
                return BadRequest(new
                {
                    message = "Duplicate username"
                });
            }

            var trainer = new Trainer
            {
                GameId = gameId,
                TrainerId = Guid.NewGuid().ToString(),
                Username = username,
                PasswordHash = Request.Query["passwordHash"],
                Salt = Request.Query["salt"],
                Items = new List<Item>()
            };
            if (trainer.GameId == null || trainer.Username == null || trainer.PasswordHash == null || trainer.Salt == null)
            {
                return BadRequest();
            }

            TableHelpers.TrainerTable.Collection.InsertOne(trainer);
            return trainer;
        }

        [HttpPost("{gameId}/gm")]
        public ActionResult<Trainer> PostGM(string gameId)
        {
            if (TableHelpers.TrainerTable.Collection.Find(trainer => trainer.IsGM).Any())
            {
                return BadRequest(new
                {
                    message = "There is already a GM"
                });
            }

            if (!TableHelpers.GameTable.Collection.Find(game => game.GameId == gameId).Any())
            {
                return NotFound(gameId);
            }

            var trainer = new Trainer
            {
                GameId = gameId,
                TrainerId = Guid.NewGuid().ToString(),
                Username = Request.Query["userName"],
                PasswordHash = Request.Query["passwordHash"],
                Salt = Request.Query["salt"],
                IsGM = true,
                Items = new List<Item>()
            };
            if (trainer.GameId == null || trainer.Username == null || trainer.PasswordHash == null || trainer.Salt == null)
            {
                return BadRequest();
            }

            TableHelpers.TrainerTable.Collection.InsertOne(trainer);
            return trainer;
        }
    }
}
