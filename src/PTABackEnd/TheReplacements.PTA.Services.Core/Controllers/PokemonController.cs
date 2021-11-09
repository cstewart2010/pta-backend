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
    [Route("api/v1/pokemon")]
    public class PokemonController : ControllerBase
    {
        private readonly ILogger<PokemonController> _logger;

        public PokemonController(ILogger<PokemonController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{id}")]
        public ActionResult<PokemonModel> GetPokemon(string id)
        {
            return DatabaseUtility.FindPokemonById(id);
        }

        [HttpPut]
        public ActionResult<object> TradePokemon()
        {
            var leftTrainer = DatabaseUtility.FindTrainerById(Request.Query["leftTrainer"]);
            var rightTrainer = DatabaseUtility.FindTrainerById(Request.Query["rightTrainer"]);
            if (leftTrainer == null || rightTrainer == null)
            {
                return BadRequest();
            }

            if (leftTrainer.TrainerId == rightTrainer.TrainerId)
            {
                return BadRequest(new
                {
                    message = "Cannot trade pokemon to oneself"
                });
            }

            if (leftTrainer.GameId != rightTrainer.GameId)
            {
                return BadRequest(new
                {
                    message = "Cannot trade pokemon to trainers in different games"
                });
            }

            var leftPokemon = DatabaseUtility.FindPokemon(pokemon => pokemon.PokemonId == Request.Query["leftPokemon"] && pokemon.TrainerId == leftTrainer.TrainerId);
            var rightPokemon = DatabaseUtility.FindPokemon(pokemon => pokemon.PokemonId == Request.Query["rightPokemon"] && pokemon.TrainerId == rightTrainer.TrainerId);
            var pokemon = DatabaseUtility.FindPokemonById(Request.Query["pokemonId"]);
            if (leftPokemon == null || rightPokemon == null)
            {
                return NotFound();
            }

            Expression<Func<PokemonModel, bool>> leftFilter = pokemon => pokemon.PokemonId == leftPokemon.PokemonId;
            DatabaseUtility.UpdatePokemon
            (
                leftFilter,
                Builders<PokemonModel>.Update.Set("TrainerId", rightTrainer.TrainerId)
            );

            Expression<Func<PokemonModel, bool>> rightFilter = pokemon => pokemon.PokemonId == rightPokemon.PokemonId;
            DatabaseUtility.UpdatePokemon
            (
                rightFilter,
                Builders<PokemonModel>.Update.Set("TrainerId", leftTrainer.TrainerId)
            );

            return new
            {
                leftPokemon = new
                {
                    leftPokemon.PokemonId,
                    rightTrainer.TrainerId
                },
                rightPokemon = new
                {
                    rightPokemon.PokemonId,
                    leftTrainer.TrainerId
                }
            };
        }

        [HttpPut("{id}")]
        public ActionResult<PokemonModel> UpdatePokemon(string id)
        {
            Expression<Func<PokemonModel, bool>> filter = pokemon => pokemon.PokemonId == id;
            var updates = GetPokemonUpdates(filter);

            if (updates != null)
            {
                return BadRequest(new
                {
                    message = "No updates were found"
                });
            }

            var updateResult = DatabaseUtility.UpdatePokemon
            (
                filter,
                updates
            );
            if (!updateResult)
            {
                return StatusCode(500);
            }

            return DatabaseUtility.FindPokemon(filter);
        }

        [HttpDelete("{id}")]
        public ActionResult<object> DeletePokemon(string id)
        {
            var deleteResult = DatabaseUtility.DeletePokemon(id);
            if (!deleteResult)
            {
                NotFound(id);
            }

            return new
            {
                message = $"Successfully deleted {id}"
            };
        }

        [HttpDelete("trainer/{trainerId}")]
        public ActionResult<object> DeleteTrainerMons(string trainerId)
        {
            var deleteResponse = DatabaseUtility.DeleteTrainerMons(trainerId);
            if (deleteResponse != null)
            {
                return deleteResponse;
            }

            return StatusCode(500);
        }

        private UpdateDefinition<PokemonModel> GetPokemonUpdates(Expression<Func<PokemonModel, bool>> filter)
        {
            var pokemon = DatabaseUtility.FindPokemon(filter);
            var updates = new List<UpdateDefinition<PokemonModel>>();
            if (int.TryParse(Request.Query["experience"], out var experience))
            {
                updates.Add(Builders<PokemonModel>.Update.Set("Experience", experience));
            }
            if (int.TryParse(Request.Query["hpAdded"], out var added))
            {
                pokemon.HP.Added = added;
                updates.Add(Builders<PokemonModel>.Update.Set("HP", pokemon.HP));
            }
            if (int.TryParse(Request.Query["attackAdded"], out added))
            {
                pokemon.Attack.Added = added;
                updates.Add(Builders<PokemonModel>.Update.Set("Attack", pokemon.Attack));
            }
            if (int.TryParse(Request.Query["defenseAdded"], out added))
            {
                pokemon.Defense.Added = added;
                updates.Add(Builders<PokemonModel>.Update.Set("Defense", pokemon.Defense));
            }
            if (int.TryParse(Request.Query["specialAttackAdded"], out added))
            {
                pokemon.SpecialAttack.Added = added;
                updates.Add(Builders<PokemonModel>.Update.Set("SpecialAttack", pokemon.SpecialAttack));
            }
            if (int.TryParse(Request.Query["specialDefenseAdded"], out added))
            {
                pokemon.SpecialDefense.Added = added;
                updates.Add(Builders<PokemonModel>.Update.Set("SpecialDefense", pokemon.SpecialDefense));
            }
            if (int.TryParse(Request.Query["speedAdded"], out added))
            {
                pokemon.Speed.Added = added;
                updates.Add(Builders<PokemonModel>.Update.Set("Special", pokemon.Speed));
            }
            if (!string.IsNullOrWhiteSpace(Request.Query["nickname"]))
            {
                updates.Add(Builders<PokemonModel>.Update.Set("Nickname", Request.Query["nickname"]));
            }

            return updates.Any()
                ? Builders<PokemonModel>.Update.Combine(updates.ToArray())
                : null;
        }
    }
}
