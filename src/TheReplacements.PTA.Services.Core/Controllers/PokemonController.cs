using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using TheReplacements.PTA.Common.Utilities;
using TheReplacements.PTA.Common.Models;
using TheReplacements.PTA.Common.Enums;

namespace TheReplacements.PTA.Services.Core.Controllers
{
    [ApiController]
    [Route("api/v1/pokemon")]
    public class PokemonController : ControllerBase
    {
        private const MongoCollection Collection = MongoCollection.Pokemon;

        private string ClientIp => Request.HttpContext.Connection.RemoteIpAddress.ToString();

        [HttpGet("{pokemonId}")]
        public ActionResult<PokemonModel> GetPokemon(string pokemonId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            var pokemon = DatabaseUtility.FindPokemonById(pokemonId);
            if (pokemon == null)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve pokemon {pokemonId}");
                return NotFound(pokemonId);
            }

            pokemon.AggregateStats();
            LoggerUtility.Info(Collection, $"Client {ClientIp} successfully hit {Request.Path.Value} {Request.Method} endpoint");
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
                    message = "Missing the following parameters in the query",
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

            if (!DatabaseUtility.TryAddPokemon(pokemon, out var error))
            {
                return BadRequest(error);
            }

            pokemon.AggregateStats();
            LoggerUtility.Info(Collection, $"Client {ClientIp} successfully hit {Request.Path.Value} {Request.Method} endpoint");
            return pokemon;
        }

        [HttpPut("trade")]
        public ActionResult<object> TradePokemon()
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            var leftPokemonId = Request.Query["leftPokemonId"];
            var rightPokemonId = Request.Query["rightPokemonId"];
            var leftPokemon = DatabaseUtility.FindPokemonById(leftPokemonId);
            var rightPokemon = DatabaseUtility.FindPokemonById(rightPokemonId);

            if (leftPokemon == null)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve pokemon {leftPokemonId}");
                return NotFound(leftPokemonId);
            }
            if (rightPokemon == null)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve pokemon {rightPokemonId}");
                return NotFound(rightPokemonId);
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

            if (leftTrainer == null || !leftTrainer.IsOnline)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve trainer {leftPokemon.TrainerId}");
                return NotFound(leftPokemon.TrainerId);
            }

            if (rightTrainer == null || !rightTrainer.IsOnline)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve trainer {rightPokemon.TrainerId}");
                return NotFound(rightPokemon.TrainerId);
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
            LoggerUtility.Info(Collection, $"Client {ClientIp} successfully hit {Request.Path.Value} {Request.Method} endpoint");
            return new
            {
                leftPokemon,
                rightPokemon
            };
        }

        [HttpPut("{pokemonId}/update")]
        public ActionResult<PokemonModel> UpdatePokemon(string pokemonId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            if (DatabaseUtility.FindPokemonById(pokemonId) == null)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve pokemon {pokemonId}");
                return NotFound(pokemonId);
            }

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
            LoggerUtility.Info(Collection, $"Client {ClientIp} successfully hit {Request.Path.Value} {Request.Method} endpoint");
            return pokemon;
        }

        [HttpPut("{pokemonId}/evolve")]
        public ActionResult<PokemonModel> EvolvePokemon(string pokemonId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            var pokemon = DatabaseUtility.FindPokemonById(pokemonId);
            if (pokemon == null)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve pokemon {pokemonId}");
                return NotFound(pokemonId);
            }

            var evolvedFormName = Request.Query["nextForm"].ToString();
            if (string.IsNullOrEmpty(evolvedFormName))
            {
                return BadRequest(new
                {
                    message = "Missing evolvedFormName"
                });
            }

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
            LoggerUtility.Info(Collection, $"Client {ClientIp} successfully hit {Request.Path.Value} {Request.Method} endpoint");
            return evolvedForm;
        }

        [HttpDelete("{pokemonId}")]
        public ActionResult<object> DeletePokemon(string pokemonId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            var deleteResult = DatabaseUtility.DeletePokemon(pokemonId);
            if (!deleteResult)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve pokemon {pokemonId}");
                NotFound(pokemonId);
            }

            LoggerUtility.Info(Collection, $"Client {ClientIp} successfully hit {Request.Path.Value} {Request.Method} endpoint");
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
