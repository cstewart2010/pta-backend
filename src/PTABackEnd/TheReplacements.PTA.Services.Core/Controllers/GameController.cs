using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheReplacements.PTA.Common.Databases;
using TheReplacements.PTA.Common.Models;
using TheReplacements.PTA.Services.Core.Internal;

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
        public ActionResult<Game> Get(string id)
        {
            var game = TableHelpers.GameTable.Collection.Find(game => game.GameId == id).FirstOrDefault();
            if (game == null)
            {
                return NotFound(id);
            }
            return game;
        }

        [HttpPost]
        public ActionResult<Game> Post()
        {
            var id = Guid.NewGuid().ToString();
            TableHelpers.GameTable.Collection.InsertOne(new Game
            {
                GameId = id
            });
            return TableHelpers.GameTable.Collection.Find(game => game.GameId == id).FirstOrDefault();
        }

        [HttpDelete("{id}")]
        public void Delete(string id)
        {
            var result = TableHelpers.GameTable.Collection.DeleteOne(game => game.GameId == id);
            if (!result.IsAcknowledged)
            {
                NotFound(id);
            }
        }
    }
}
