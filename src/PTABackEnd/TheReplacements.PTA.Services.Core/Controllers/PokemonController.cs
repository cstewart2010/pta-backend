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
            return DatabaseUtility.TableHelper.Pokemon.Find(pokemon => pokemon._id.ToString() == id).FirstOrDefault();
        }

        [HttpPost("add")]
        public ActionResult<PokemonModel> AddPokemon()
        {
            var fails = new[] { "trainerId", "pokemon", "nature", "naturalMoves", "expYield", "catchRate", "experience", "level" }
                .Where(key => string.IsNullOrWhiteSpace(Request.Query[key]));
            if (fails.Any())
            {
                return BadRequest(new
                {
                    message = "Missing the follwoing parameters in the query",
                    fails
                });
            }
            if (!(int.TryParse(Request.Query["expYield"], out var expYield) && expYield > 0))
            {
                return BadRequest(new
                {
                    message = "Invalid expYield",
                    invalidValue = Request.Query["expYield"]
                });
            }
            if (!(int.TryParse(Request.Query["catchRate"], out var catchRate) && catchRate >= 0))
            {
                return BadRequest(new
                {
                    message = "Invalid catchRate",
                    invalidValue = Request.Query["catchRate"]
                });
            }
            if (!(int.TryParse(Request.Query["experience"], out var experience) && experience >= 0))
            {
                return BadRequest(new
                {
                    message = "Invalid experience",
                    invalidValue = Request.Query["experience"]
                });
            }
            if (!(int.TryParse(Request.Query["level"], out var level) && level > 0))
            {
                return BadRequest(new
                {
                    message = "Invalid level",
                    invalidValue = Request.Query["level"]
                });
            }
            var trainerId = Request.Query["trainerId"];
            var pokemonName = Request.Query["pokemon"];
            var natureName = Request.Query["nature"];
            var pokemon = PokeAPIUtility.GetPokemon
            (
                pokemonName,
                natureName
            );

            pokemon.Trainerid = trainerId;
            pokemon.NaturalMoves = Request.Query["naturalMoves"].ToString().Split(",");
            pokemon.ExpYield = expYield;
            pokemon.CatchRate = catchRate;
            pokemon.Experience = experience;
            pokemon.Level = level;
            if (!string.IsNullOrWhiteSpace(Request.Query["nickname"]))
            {
                pokemon.Nickname = Request.Query["nickname"];
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["tmMoves"]))
            {
                pokemon.TMMoves = Request.Query["tmMoves"].ToString().Split(",");
            }

            return pokemon;
        }

        [HttpPut("{id}")]
        public ActionResult<PokemonModel> UpdatePokemon(string id)
        {
            Expression<Func<PokemonModel, bool>> filter = pokemon => pokemon._id.ToString() == id;
            var pokemon = DatabaseUtility.TableHelper.Pokemon.Find(filter).FirstOrDefault();
            var updates = new List<UpdateDefinition<PokemonModel>>();
            if (int.TryParse(Request.Query["expereience"], out var experience))
            {
                updates.Add(Builders<PokemonModel>.Update.Set("Experience", experience));
            }
            int added;
            if (int.TryParse(Request.Query["hpAdded"], out added))
            {
                pokemon.HP.Added = added;
                updates.Add(Builders<PokemonModel>.Update.Set("HP", pokemon.HP));
            }
            if (int.TryParse(Request.Query["attackAdded"], out added))
            {
                pokemon.Attack.Added = added;
                updates.Add(Builders<PokemonModel>.Update.Set("HP", pokemon.Attack));
            }
            if (int.TryParse(Request.Query["defenseAdded"], out added))
            {
                pokemon.Defense.Added = added;
                updates.Add(Builders<PokemonModel>.Update.Set("HP", pokemon.Defense));
            }
            if (int.TryParse(Request.Query["specialAttackAdded"], out added))
            {
                pokemon.SpecialAttack.Added = added;
                updates.Add(Builders<PokemonModel>.Update.Set("HP", pokemon.SpecialAttack));
            }
            if (int.TryParse(Request.Query["specialDefenseAdded"], out added))
            {
                pokemon.SpecialDefense.Added = added;
                updates.Add(Builders<PokemonModel>.Update.Set("HP", pokemon.SpecialDefense));
            }
            if (int.TryParse(Request.Query["speedAdded"], out added))
            {
                pokemon.Speed.Added = added;
                updates.Add(Builders<PokemonModel>.Update.Set("HP", pokemon.Speed));
            }
            if (!string.IsNullOrWhiteSpace(Request.Query["nickname"]))
            {
                updates.Add(Builders<PokemonModel>.Update.Set("Nickname", Request.Query["nickname"]));
            }

            if (!updates.Any())
            {
                return BadRequest(new
                {
                    message = "No updates were found"
                });
            }

            pokemon = DatabaseUtility.TableHelper.Pokemon.FindOneAndUpdate(filter, Builders<PokemonModel>.Update.Combine(updates.ToArray()));
            if (pokemon == null)
            {
                return StatusCode(500);
            }

            return pokemon;
        }

        [HttpDelete("{id}")]
        public ActionResult<PokemonModel> DeletePokemon(string id)
        {
            var pokemon = DatabaseUtility
                .TableHelper
                .Pokemon
                .FindOneAndDelete(pokemon => pokemon._id.ToString() == id);
            if (pokemon == null)
            {
                NotFound(id);
            }

            return pokemon;
        }
    }
}
