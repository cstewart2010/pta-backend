using Microsoft.AspNetCore.Mvc;
using System.Linq;
using TheReplacements.PTA.Common.Utilities;
using TheReplacements.PTA.Common.Models;
using TheReplacements.PTA.Common.Enums;
using TheReplacements.PTA.Services.Core.Extensions;

namespace TheReplacements.PTA.Services.Core.Controllers
{
    [ApiController]
    [Route("api/v1/pokemon")]
    public class PokemonController : PtaControllerBase
    {
        protected override MongoCollection Collection { get; }

        public PokemonController()
        {
            Collection = MongoCollection.Pokemon;
        }

        [HttpGet("{pokemonId}")]
        public ActionResult<PokemonModel> GetPokemon(string pokemonId)
        {
            Response.UpdateAccessControl();
            var document = GetDocument(pokemonId, Collection, out var notFound);
            if (!(document is PokemonModel pokemon))
            {
                return notFound;
            }

            pokemon.AggregateStats();
            return ReturnSuccessfully(pokemon);
        }

        [HttpPut("trade")]
        public ActionResult<object> TradePokemon()
        {
            Response.UpdateAccessControl();
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
            var leftDocument = GetDocument(leftPokemonId, Collection, out var notFound);
            if (!(leftDocument is PokemonModel leftPokemon))
            {
                return notFound;
            }

            var rightPokemonId = Request.Query["rightPokemonId"];
            var rightDocument = GetDocument(rightPokemonId, Collection, out notFound);
            if (!(rightDocument is PokemonModel rightPokemon))
            {
                return notFound;
            }

            if (leftPokemon.TrainerId == rightPokemon.TrainerId)
            {
                return BadRequest(new
                {
                    message = "Cannot trade pokemon to oneself"
                });
            }

            if (!(GetDocument(leftPokemon.TrainerId, MongoCollection.Trainer, out notFound) is TrainerModel leftTrainer))
            {
                return notFound;
            }
            if (!(GetDocument(rightPokemon.TrainerId, MongoCollection.Trainer, out notFound) is TrainerModel rightTrainer))
            {
                return notFound;
            }

            if (!TradeCheck(gameMaster.GameId, leftTrainer?.GameId, out var authError))
            {
                return authError;
            }
            if (!TradeCheck(gameMaster.GameId, rightTrainer?.GameId, out authError))
            {
                return authError;
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
            Response.RefreshToken();
            return ReturnSuccessfully(new
            {
                leftPokemon,
                rightPokemon
            });
        }

        [HttpPut("{pokemonId}/update")]
        public ActionResult<PokemonModel> UpdatePokemon(string pokemonId)
        {
            Response.UpdateAccessControl();
            var trainerId = Request.Query["trainerId"];
            if (!Header.VerifyCookies(Request.Cookies, trainerId))
            {
                return Unauthorized();
            }

            var pokemon = GetPokemonFromTrainer(trainerId, pokemonId, out var error);
            if (pokemon == null)
            {
                return error;
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
            Response.RefreshToken();
            return ReturnSuccessfully(pokemon);
        }

        [HttpPut("{pokemonId}/evolve")]
        public ActionResult<PokemonModel> EvolvePokemon(string pokemonId)
        {
            Response.UpdateAccessControl();
            var trainerId = Request.Query["trainerId"];
            if (!Header.VerifyCookies(Request.Cookies, trainerId))
            {
                return Unauthorized();
            }

            var pokemon = GetPokemonFromTrainer(trainerId, pokemonId, out var error);
            if (pokemon == null)
            {
                return error;
            }

            var evolvedForm = GetEvolved(Request.Query["nextForm"], pokemon, out var badRequest);
            if (evolvedForm == null)
            {
                return badRequest;
            }

            DatabaseUtility.UpdatePokemonWithEvolution(pokemonId, evolvedForm);
            evolvedForm.AggregateStats();
            Response.RefreshToken();
            return ReturnSuccessfully(pokemon);
        }

        [HttpDelete("{pokemonId}")]
        public ActionResult<object> DeletePokemon(string pokemonId)
        {
            Response.UpdateAccessControl();
            var gameMasterId = Request.Query["gameMasterId"];
            if (!Header.VerifyCookies(Request.Cookies, gameMasterId))
            {
                return Unauthorized();
            }

            if (!IsGMOnline(gameMasterId))
            {
                return NotFound(gameMasterId);
            }

            if (!DatabaseUtility.DeletePokemon(pokemonId))
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve pokemon {pokemonId}");
                NotFound(pokemonId);
            }

            Response.RefreshToken();
            return ReturnSuccessfully(new
            {
                message = $"Successfully deleted {pokemonId}"
            });
        }

        private bool TradeCheck(string gameMasterGameId, string trainerGameId, out ActionResult authError)
        {
            authError = null;
            var result = gameMasterGameId == trainerGameId;
            if (gameMasterGameId == trainerGameId)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} attempt to authorize a trade for an unknown player");
                authError = Unauthorized(new
                {
                    gameMasterGameId,
                    trainerGameId
                });
            }

            return result;
        }

        private PokemonModel GetPokemonFromTrainer(
            string trainerId,
            string pokemonId,
            out ActionResult error)
        {
            error = null;

            var trainerDocument = GetDocument(trainerId, MongoCollection.Trainer, out var notFound);
            if (!(trainerDocument is TrainerModel trainer))
            {
                error = notFound;
                return null;
            }

            var pokemonDocument = GetDocument(pokemonId, MongoCollection.Pokemon, out notFound);
            if (!(pokemonDocument is PokemonModel pokemon))
            {
                error = notFound;
                return null;
            }

            if (trainer.TrainerId != pokemon.TrainerId)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} attempted to update pokemon with unknown pokemon {pokemonId}");
                error = Unauthorized(pokemonId);
                return null;
            }

            return pokemon;
        }
        private PokemonModel GetEvolved(
            string evolvedFormName,
            PokemonModel currentForm,
            out BadRequestObjectResult badRequest)
        {
            badRequest = null;
            if (string.IsNullOrEmpty(evolvedFormName))
            {
                badRequest = BadRequest(new
                {
                    message = "Missing evolvedFormName"
                });

                return null;
            }

            var evolvedForm = PokeAPIUtility.GetEvolved(currentForm, evolvedFormName);
            if (evolvedForm == null)
            {
                badRequest = BadRequest(new
                {
                    message = $"Could not evolve {currentForm.Nickname} to {evolvedFormName}"
                });
            }

            return evolvedForm;
        }
    }
}
