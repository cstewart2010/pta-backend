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

        [HttpGet("{pokemonId}")]
        public ActionResult<PokemonModel> GetPokemon(string pokemonId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            return DatabaseUtility.FindPokemonById(pokemonId);
        }

        [HttpPost("wild")]
        public ActionResult<PokemonModel> AddPokemon()
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            var fails = new[] { "pokemon", "nature", "naturalMoves", "expYield", "catchRate", "experience", "level" }
                .Where(key => string.IsNullOrWhiteSpace(Request.Query[key]));
            if (fails.Any())
            {
                return BadRequest(new
                {
                    message = "Missing the follwoing parameters in the query",
                    fails
                });
            }
            var parseFails = new[]
            {
                    GetBadRequestMessage("expYield", result => result > 0, out var expYield),
                    GetBadRequestMessage("catchRate", result => result >= 0, out var catchRate),
                    GetBadRequestMessage("experience", result => result >= 0, out var experience),
                    GetBadRequestMessage("level", result => result > 0, out var level),
                }.Where(fail => fail != null);
            if (parseFails.Any())
            {
                return BadRequest(parseFails);
            }

            var naturalMoves = Request.Query["naturalMoves"].ToString().Split(",");
            if (naturalMoves.Length < 1 || naturalMoves.Length > 4)
            {
                return BadRequest(naturalMoves);
            }
            var tmMoves = Request.Query["tmMoves"].ToString()?.Split(",") ?? new string[0];
            if (tmMoves.Length > 4)
            {
                return BadRequest(tmMoves);
            }

            var pokemonName = Request.Query["pokemon"];
            var natureName = Request.Query["nature"];
            var pokemon = PokeAPIUtility.GetPokemon
            (
                pokemonName,
                natureName
            );

            pokemon.TrainerId = "Wild";
            pokemon.NaturalMoves = naturalMoves;
            pokemon.TMMoves = tmMoves;
            pokemon.ExpYield = expYield;
            pokemon.CatchRate = catchRate;
            pokemon.Experience = experience;
            pokemon.Level = level;
            if (!string.IsNullOrWhiteSpace(Request.Query["nickname"]))
            {
                pokemon.Nickname = Request.Query["nickname"];
            }

            DatabaseUtility.AddPokemon(pokemon);

            return pokemon;
        }

        [HttpPut("trade")]
        public ActionResult<object> TradePokemon()
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            var leftTrainer = DatabaseUtility.FindTrainerById(Request.Query["leftTrainerId"]);
            var rightTrainer = DatabaseUtility.FindTrainerById(Request.Query["rightTrainerId"]);
            if (leftTrainer == null || rightTrainer == null)
            {
                return NotFound();
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

            var leftPokemon = DatabaseUtility.FindPokemon(pokemon => pokemon.PokemonId == Request.Query["leftPokemonId"] && pokemon.TrainerId == leftTrainer.TrainerId);
            var rightPokemon = DatabaseUtility.FindPokemon(pokemon => pokemon.PokemonId == Request.Query["rightPokemonId"] && pokemon.TrainerId == rightTrainer.TrainerId);
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
                leftPokemon,
                rightPokemon
            };
        }

        [HttpPut("update/{pokemonId}")]
        public ActionResult<PokemonModel> UpdatePokemon(string pokemonId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            Expression<Func<PokemonModel, bool>> filter = pokemon => pokemon.PokemonId == pokemonId;
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

        [HttpPut("evolve/{pokemonId}")]
        public ActionResult<PokemonModel> EvolvePokemon(string pokemonId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            var pokemon = DatabaseUtility.FindPokemonById(pokemonId);
            if (pokemon == null)
            {
                return NotFound(pokemonId);
            }

            var evolvedFormName = Request.Query["nextForm"];
            var evolvedForm = PokeAPIUtility.GetEvolved(pokemon, evolvedFormName);
            if (evolvedForm == null)
            {
                return BadRequest(new
                {
                    message = $"Could not evolve {pokemon.Nickname} to {evolvedFormName}"
                });
            }


            var updates = new[]
            {
                Builders<PokemonModel>.Update.Set("DexNo", evolvedForm.DexNo),
                Builders<PokemonModel>.Update.Set("HP", evolvedForm.HP),
                Builders<PokemonModel>.Update.Set("Attack", evolvedForm.Attack),
                Builders<PokemonModel>.Update.Set("Defense", evolvedForm.Defense),
                Builders<PokemonModel>.Update.Set("SpecialAttack", evolvedForm.SpecialAttack),
                Builders<PokemonModel>.Update.Set("SpecialDefense", evolvedForm.SpecialDefense),
                Builders<PokemonModel>.Update.Set("Speed", evolvedForm.Speed),
                Builders<PokemonModel>.Update.Set("Nickname", evolvedForm.Nickname)
            };

            Expression<Func<PokemonModel, bool>> filter = pokemon => pokemon.PokemonId == pokemonId;
            DatabaseUtility.UpdatePokemon(filter, Builders<PokemonModel>.Update.Combine(updates));

            return evolvedForm;
        }

        [HttpDelete("{pokemonId}")]
        public ActionResult<object> DeletePokemon(string pokemonId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            var deleteResult = DatabaseUtility.DeletePokemon(pokemonId);
            if (!deleteResult)
            {
                NotFound(pokemonId);
            }

            return new
            {
                message = $"Successfully deleted {pokemonId}"
            };
        }

        private object GetBadRequestMessage(
            string parameter,
            Predicate<int> check,
            out int outVar)
        {
            var value = Request.Query[parameter];
            if (!(int.TryParse(value, out outVar) && check(outVar)))
            {
                return new
                {
                    message = $"Invalid {parameter}",
                    invalidValue = value
                };
            }

            return null;
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
                updates.Add(Builders<PokemonModel>.Update.Set("Speed", pokemon.Speed));
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
