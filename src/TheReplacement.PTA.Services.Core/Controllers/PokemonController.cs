﻿using Microsoft.AspNetCore.Mvc;
using System.Linq;
using TheReplacement.PTA.Common.Utilities;
using TheReplacement.PTA.Common.Models;
using TheReplacement.PTA.Common.Enums;
using TheReplacement.PTA.Services.Core.Extensions;
using TheReplacement.PTA.Services.Core.Messages;

namespace TheReplacement.PTA.Services.Core.Controllers
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

            var (leftPokemon, rightPokemon) = GetTradePokemon(out var badRequest);
            if (badRequest != null)
            {
                return badRequest;
            }

            var areTrainersOnline = AreTrainersOnline
            (
                gameMaster.GameId,
                leftPokemon.TrainerId,
                rightPokemon.TrainerId,
                out badRequest
            );
            if (!areTrainersOnline)
            {
                return badRequest;
            }

            UpdatePokemonTrainerIds
            (
                leftPokemon,
                rightPokemon
            );
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

        private bool AreTrainersOnline(
            string gameId,
            string leftTrainerId,
            string rightTrainerId,
            out ActionResult badResult)
        {
            if (!IsTrainerOnline(gameId, leftTrainerId, out badResult))
            {
                return false;
            }
            if (!IsTrainerOnline(gameId, rightTrainerId, out badResult))
            {
                return false;
            }

            return true;
        }

        private bool IsTrainerOnline(
            string gameId,
            string trainerId,
            out ActionResult badResult)
        {
            if (!(GetDocument(trainerId, MongoCollection.Trainer, out badResult) is TrainerModel trainer))
            {
                return false;
            }
            if (!TradeCheck(gameId, trainer.GameId, out badResult))
            {
                return false;
            }
            if (!trainer.IsOnline)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve trainer {trainerId}");
                badResult = NotFound(trainerId);
                return false;
            }

            return true;
        }

        private void UpdatePokemonTrainerIds(
            PokemonModel leftPokemon,
            PokemonModel rightPokemon)
        {

            DatabaseUtility.UpdatePokemonTrainerId
            (
                leftPokemon.PokemonId,
                rightPokemon.TrainerId
            );

            leftPokemon.AggregateStats();
            DatabaseUtility.UpdatePokemonTrainerId
            (
                rightPokemon.PokemonId,
                leftPokemon.TrainerId
            );

            rightPokemon.AggregateStats();
            Response.RefreshToken();
        }

        private bool TradeCheck(
            string gameMasterGameId, 
            string trainerGameId,
            out ActionResult authError)
        {
            authError = null;
            var result = gameMasterGameId == trainerGameId;
            if (!result)
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

        private (PokemonModel LeftPokemon, PokemonModel RightPokemon) GetTradePokemon(out ActionResult notFound)
        {
            var leftPokemonId = Request.Query["leftPokemonId"];
            var leftDocument = GetDocument(leftPokemonId, Collection, out notFound);
            if (!(leftDocument is PokemonModel leftPokemon))
            {
                return default;
            }

            var rightPokemonId = Request.Query["rightPokemonId"];
            var rightDocument = GetDocument(rightPokemonId, Collection, out notFound);
            if (!(rightDocument is PokemonModel rightPokemon))
            {
                return default;
            }

            if (leftPokemon.TrainerId == rightPokemon.TrainerId)
            {
                notFound = BadRequest(new GenericMessage("Cannot trade pokemon to oneself"));
                return default;
            }

            return (leftPokemon, rightPokemon);
        }

        private PokemonModel GetPokemonFromTrainer(
            string trainerId,
            string pokemonId,
            out ActionResult error)
        {
            var trainerDocument = GetDocument(trainerId, MongoCollection.Trainer, out error);
            if (!(trainerDocument is TrainerModel trainer))
            {
                return null;
            }

            var pokemonDocument = GetDocument(pokemonId, MongoCollection.Pokemon, out error);
            if (!(pokemonDocument is PokemonModel pokemon))
            {
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