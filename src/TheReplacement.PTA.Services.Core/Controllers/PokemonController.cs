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
            var document = GetDocument(pokemonId, Collection, out var notFound);
            if (!(document is PokemonModel pokemon))
            {
                return notFound;
            }

            return ReturnSuccessfully(pokemon);
        }

        [HttpPut("trade")]
        public ActionResult<object> TradePokemon()
        {
            var gameMasterId = Request.Query["gameMasterId"];
            if (!Request.VerifyIdentity(gameMasterId))
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

            Response.RefreshToken(gameMasterId);
            return ReturnSuccessfully(new
            {
                leftPokemon,
                rightPokemon
            });
        }

        [HttpPut("{pokemonId}/evolve")]
        public ActionResult<PokemonModel> EvolvePokemon(string pokemonId)
        {
            var trainerId = Request.Query["trainerId"];
            if (!Request.VerifyIdentity(trainerId))
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
            Response.RefreshToken(trainerId);
            return ReturnSuccessfully(pokemon);
        }

        [HttpDelete("{pokemonId}")]
        public ActionResult<GenericMessage> DeletePokemon(string pokemonId)
        {
            var gameMasterId = Request.Query["gameMasterId"];
            if (!Request.VerifyIdentity(gameMasterId))
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

            Response.RefreshToken(gameMasterId);
            return ReturnSuccessfully(new GenericMessage($"Successfully deleted {pokemonId}"));
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
            if (!(GetDocument(trainerId, MongoCollection.Trainers, out badResult) is TrainerModel trainer))
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

            DatabaseUtility.UpdatePokemonTrainerId
            (
                rightPokemon.PokemonId,
                leftPokemon.TrainerId
            );
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
            var trainerDocument = GetDocument(trainerId, MongoCollection.Trainers, out error);
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
                badRequest = BadRequest(new GenericMessage("Missing evolvedFormName"));
                return null;
            }

            var keptMoves = Request.Query["keptMoves"].ToString()?.Split(',');
            var newMoves = Request.Query["newMoves"].ToString()?.Split(',');
            var total = keptMoves.Length + newMoves.Length;
            if (total < 3)
            {
                badRequest = BadRequest(new GenericMessage("Too few moves"));
                return null;
            }
            if (total > 6)
            {
                badRequest = BadRequest(new GenericMessage("Too many moves"));
                return null;
            }

            var moveComparer = currentForm.Moves.Select(move => move.ToLower());
            if (!keptMoves.All(move => moveComparer.Contains(move.ToLower())))
            {
                badRequest = BadRequest(new GenericMessage($"{currentForm.Nickname} doesn't contain one of {Request.Query["keptMoves"]}"));
                return null;
            }

            var evolvedForm = StaticDocumentUtility.GetEvolved(currentForm, keptMoves, Request.Query["nextForm"], newMoves);
            if (evolvedForm == null)
            {
                badRequest = BadRequest(new GenericMessage($"Could not evolve {currentForm.Nickname} to {evolvedFormName}"));
            }

            return evolvedForm;
        }
    }
}
