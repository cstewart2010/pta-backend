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

        [HttpPut("trade")]
        public ActionResult<object> TradePokemon()
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            var gameMasterId = Request.Query["gameMasterId"];
            if (!Header.VerifyCookies(Request.Cookies, gameMasterId))
            {
                return Unauthorized();
            }

            var gameMaster = DatabaseUtility.FindTrainerById(gameMasterId);
            if (gameMaster?.IsOnline != true)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} attempt to authorize a trade while not being an online gm");
                return Unauthorized(gameMasterId);
            }

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

            if (gameMaster.GameId != leftTrainer?.GameId)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} attempt to authorize a trade for an unknown player");
                return Unauthorized(new
                {
                    gameMasterGameId = gameMasterId,
                    trainerGameId = leftTrainer.GameId
                });
            }

            if (leftTrainer.GameId != rightTrainer?.GameId)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} attempt to authorize a trade for an unknown player");
                return Unauthorized(new
                {
                    gameMasterGameId = gameMasterId,
                    rightTrainerGameId = rightTrainer.GameId
                });
            }

            if (!leftTrainer.IsOnline)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve trainer {leftPokemon.TrainerId}");
                return NotFound(leftPokemon.TrainerId);
            }

            if (!rightTrainer.IsOnline)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve trainer {rightPokemon.TrainerId}");
                return NotFound(rightPokemon.TrainerId);
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
            Response.Cookies.Append("ptaActivityToken", EncryptionUtility.GenerateToken());
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
            var trainerId = Request.Query["trainerId"];
            if (!Header.VerifyCookies(Request.Cookies, trainerId))
            {
                return Unauthorized();
            }

            var trainer = DatabaseUtility.FindTrainerById(trainerId);
            if (trainer?.IsOnline != true)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} attempted to update pokemon with unknown user");
                return NotFound(trainerId);
            }

            var pokemon = DatabaseUtility.FindPokemonById(pokemonId);
            if (pokemon == null)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} attempted to update pokemon with unknown pokemon {pokemonId}");
                return NotFound(pokemonId);
            }

            if (trainer.TrainerId != pokemon.TrainerId)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} attempted to update pokemon with unknown pokemon {pokemonId}");
                return Unauthorized(pokemonId);
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

            pokemon = DatabaseUtility.FindPokemonById(pokemonId);
            pokemon.AggregateStats();
            Response.Cookies.Append("ptaActivityToken", EncryptionUtility.GenerateToken());
            LoggerUtility.Info(Collection, $"Client {ClientIp} successfully hit {Request.Path.Value} {Request.Method} endpoint");
            return pokemon;
        }

        [HttpPut("{pokemonId}/evolve")]
        public ActionResult<PokemonModel> EvolvePokemon(string pokemonId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            var trainerId = Request.Query["trainerId"];
            if (!Header.VerifyCookies(Request.Cookies, trainerId))
            {
                return Unauthorized();
            }

            var trainer = DatabaseUtility.FindTrainerById(trainerId);
            if (trainer?.IsOnline != true)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} attempted to update pokemon with unknown user");
                return NotFound(trainerId);
            }

            var pokemon = DatabaseUtility.FindPokemonById(pokemonId);
            if (pokemon == null)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} attempted to update pokemon with unknown pokemon {pokemonId}");
                return NotFound(pokemonId);
            }

            if (trainer.TrainerId != pokemon.TrainerId)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} attempted to update pokemon with unknown pokemon {pokemonId}");
                return Unauthorized(pokemonId);
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
            Response.Cookies.Append("ptaActivityToken", EncryptionUtility.GenerateToken());
            LoggerUtility.Info(Collection, $"Client {ClientIp} successfully hit {Request.Path.Value} {Request.Method} endpoint");
            return evolvedForm;
        }

        [HttpDelete("{pokemonId}")]
        public ActionResult<object> DeletePokemon(string pokemonId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            var gameMasterId = Request.Query["gameMasterId"];
            if (!Header.VerifyCookies(Request.Cookies, gameMasterId))
            {
                return Unauthorized();
            }

            var gameMaster = DatabaseUtility.FindTrainerById(gameMasterId);
            if (!(gameMaster?.IsGM == true && gameMaster.IsOnline))
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} attempted to delete a pokemon without being a game master");
                return NotFound(gameMasterId);
            }

            var deleteResult = DatabaseUtility.DeletePokemon(pokemonId);
            if (!deleteResult)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve pokemon {pokemonId}");
                NotFound(pokemonId);
            }

            Response.Cookies.Append("ptaActivityToken", EncryptionUtility.GenerateToken());
            LoggerUtility.Info(Collection, $"Client {ClientIp} successfully hit {Request.Path.Value} {Request.Method} endpoint");
            return new
            {
                message = $"Successfully deleted {pokemonId}"
            };
        }
    }
}
