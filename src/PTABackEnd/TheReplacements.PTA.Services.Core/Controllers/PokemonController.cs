using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
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
            var pokemon = DatabaseUtility.FindPokemonById(pokemonId);
            if (pokemon == null)
            {
                return NotFound(pokemonId);
            }

            pokemon.AggregateStats();
            return pokemon;
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
            var tmMoves = Request.Query["tmMoves"].ToString()?.Split(",") ?? Array.Empty<string>();
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

            if (DatabaseUtility.TryAddPokemon(pokemon, out var error))
            {
                pokemon.AggregateStats();
                return pokemon;
            }

            return BadRequest(error);
        }

        [HttpPut("trade")]
        public ActionResult<object> TradePokemon()
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            var leftPokemon = DatabaseUtility.FindPokemonById(Request.Query["leftPokemonId"]);
            var rightPokemon = DatabaseUtility.FindPokemonById(Request.Query["leftPokemonId"]);

            if (leftPokemon == null || rightPokemon == null)
            {
                return NotFound();
            }

            if (leftPokemon.TrainerId == rightPokemon.TrainerId)
            {
                return BadRequest(new
                {
                    message = "Cannot trade pokemon to oneself"
                });
            }

            var leftTrainer = DatabaseUtility.FindTrainerById(leftPokemon.TrainerId);
            var rightTrainer = DatabaseUtility.FindTrainerById(rightPokemon.TrainerId);

            if (leftTrainer == null || rightTrainer == null)
            {
                return NotFound();
            }

            if (leftTrainer.GameId != rightTrainer.GameId)
            {
                return BadRequest(new
                {
                    message = "Cannot trade pokemon to trainers in different games"
                });
            }

            DatabaseUtility.UpdatePokemonTrainerId
            (
                leftPokemon.PokemonId,
                rightTrainer.TrainerId
            );

            DatabaseUtility.UpdatePokemonTrainerId
            (
                rightPokemon.PokemonId,
                rightTrainer.TrainerId
            );
            
            leftPokemon.AggregateStats();
            rightPokemon.AggregateStats();
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
            var updateResult = DatabaseUtility.UpdatePokemonStats
            (
                pokemonId,
                Request.Query.ToDictionary(pair => pair.Key, pair => pair.Value.ToString())
            );

            if (!updateResult)
            {
                return StatusCode(500);
            }

            var pokemon = DatabaseUtility.FindPokemonById(pokemonId);
            pokemon.AggregateStats();
            return pokemon;
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

            DatabaseUtility.UpdatePokemonWithEvolution(pokemonId, evolvedForm);
            evolvedForm.AggregateStats();
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
    }
}
