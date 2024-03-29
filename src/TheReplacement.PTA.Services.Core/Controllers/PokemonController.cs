﻿using Microsoft.AspNetCore.Mvc;
using System.Linq;
using TheReplacement.PTA.Common.Enums;
using TheReplacement.PTA.Common.Utilities;
using TheReplacement.PTA.Common.Models;
using TheReplacement.PTA.Services.Core.Extensions;
using TheReplacement.PTA.Services.Core.Messages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        public ActionResult<PokemonModel> GetPokemon(Guid pokemonId)
        {
            var document = GetDocument(pokemonId, Collection, out var notFound);
            if (document is not PokemonModel pokemon)
            {
                return notFound;
            }

            return pokemon;
        }

        [HttpGet("{gameId}/{trainerId}/{pokemonId}/possibleEvolutions")]
        public ActionResult<IEnumerable<BasePokemonModel>> GetPossibleEvolutions(Guid gameId, Guid trainerId, Guid pokemonId)
        {
            if (!Request.VerifyIdentity(trainerId))
            {
                return Unauthorized();
            }

            var pokemon = GetPokemonFromTrainer(gameId, trainerId, pokemonId, out var error);
            if (pokemon == null)
            {
                return error;
            }

            return DexUtility.GetPossibleEvolutions(pokemon).ToList();
        }

        [HttpPut("{gameId}/{gameMasterId}/trade")]
        public ActionResult<object> TradePokemon(Guid gameId, Guid gameMasterId, [FromQuery] Guid leftPokemonId, [FromQuery] Guid rightPokemonId)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            var gameMaster = DatabaseUtility.FindTrainerById(gameMasterId, gameId);
            var (leftPokemon, rightPokemon) = GetTradePokemon(leftPokemonId, rightPokemonId, out var badRequest);
            if (badRequest != null)
            {
                return badRequest;
            }

            UpdatePokemonTrainerIds
            (
                leftPokemon,
                rightPokemon
            );

            var leftTrainer = DatabaseUtility.FindTrainerById(rightPokemon.TrainerId, gameId);
            var rightTrainer = DatabaseUtility.FindTrainerById(leftPokemon.TrainerId, gameId);
            var tradeLog = new LogModel
            (
                user: gameMaster.TrainerName,
                action: $"authorized a trade between {leftTrainer.TrainerName} and {rightTrainer.TrainerName}"
            );
            DatabaseUtility.UpdateGameLogs(DatabaseUtility.FindGame(gameId), tradeLog);
            Response.RefreshToken(gameMasterId);
            return new
            {
                leftPokemon,
                rightPokemon
            };
        }

        [HttpPut("{gameId}/{trainerId}/{pokemonId}/hp/{hp}")]
        public ActionResult UpdateHP(Guid gameId, Guid trainerId, Guid pokemonId, int hp)
        {
            if (!Request.VerifyIdentity(trainerId))
            {
                return Unauthorized();
            }

            var pokemon = DatabaseUtility.FindPokemonById(pokemonId);
            if (!(pokemon?.TrainerId == trainerId || DatabaseUtility.FindTrainerById(trainerId, gameId)?.IsGM == true))
            {
                return Unauthorized();
            }

            if (hp > pokemon.PokemonStats.HP || hp < -pokemon.PokemonStats.HP)
            {
                return BadRequest(nameof(hp));
            }

            DatabaseUtility.UpdatePokemonHP(pokemonId, hp);
            return Ok();
        }

        [HttpPut("{gameId}/{trainerId}/{pokemonId}/form/{form}")]
        public ActionResult<PokemonModel> SwitchForm(Guid gameId, Guid trainerId, Guid pokemonId, string form)
        {
            if (!Request.VerifyIdentity(trainerId))
            {
                return Unauthorized();
            }

            var pokemon = GetPokemonFromTrainer(gameId, trainerId, pokemonId, out var error);
            if (pokemon == null)
            {
                return error;
            }

            form = form.Replace('_', '/');
            if (!pokemon.AlternateForms.Contains(form))
            {
                return BadRequest(nameof(form));
            }

            var result = GetDifferentForm(pokemon, form);
            result.PokemonId = pokemon.PokemonId;
            result.OriginalTrainerId = pokemon.OriginalTrainerId;
            result.TrainerId = pokemon.TrainerId;
            result.IsOnActiveTeam = pokemon.IsOnActiveTeam;
            result.IsShiny = pokemon.IsShiny;
            result.CanEvolve = pokemon.CanEvolve;
            result.Pokeball = pokemon.Pokeball;
            if (!DatabaseUtility.TryChangePokemonForm(result, out var writeError))
            {
                return BadRequest(writeError);
            }
            var trainer = DatabaseUtility.FindTrainerById(trainerId, gameId);
            var changedFormLog = new LogModel
            (
                user: trainer.TrainerName,
                action: $"changed their {pokemon.Nickname} to its {form} form"
            );
            DatabaseUtility.UpdateGameLogs(DatabaseUtility.FindGame(trainer.GameId), changedFormLog);
            Response.RefreshToken(trainerId);
            return result;
        }

        [HttpPut("{gameId}/{gameMasterId}/{pokemonId}/canEvolve")]
        public ActionResult<AbstractMessage> MarkPokemonAsEvolvable(Guid gameId, Guid gameMasterId, Guid pokemonId)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            var pokemon = DatabaseUtility.FindPokemonById(pokemonId);
            var trainer = DatabaseUtility.FindTrainerById(pokemon.TrainerId, gameId);
            if (pokemon == null)
            {
                return BadRequest(nameof(pokemonId));
            }

            if (!DatabaseUtility.UpdatePokemonEvolvability(pokemonId, true))
            {
                return BadRequest(new GenericMessage($"Failed to mark pokemon {pokemonId} as evolvable"));
            }
            var evolutionLog = new LogModel
            (
                user: trainer.TrainerName,
                action: $"can now evolve their {pokemon.Nickname}"
            );
            DatabaseUtility.UpdateGameLogs(DatabaseUtility.FindGame(trainer.GameId), evolutionLog);
            Response.RefreshToken(gameMasterId);
            return Ok();
        }

        [HttpPut("{gameId}/{trainerId}/{pokemonId}/evolve")]
        public async Task<ActionResult<PokemonModel>> EvolvePokemonAsync(Guid gameId, Guid trainerId, Guid pokemonId)
        {
            if (!Request.VerifyIdentity(trainerId))
            {
                return Unauthorized();
            }

            var pokemon = GetPokemonFromTrainer(gameId, trainerId, pokemonId, out var error);
            if (pokemon == null)
            {
                return error;
            }

            var json = await Request.GetRequestBody();
            var evolvedForm = GetEvolved
            (
                pokemon,
                json["nextForm"].ToString(),
                json["keptMoves"].Select(token => token.ToString()),
                json["newMoves"].Select(token => token.ToString()),
                out var badRequest
            );
            if (evolvedForm == null)
            {
                return badRequest;
            }

            if (!DatabaseUtility.UpdatePokemonWithEvolution(pokemonId, evolvedForm))
            {
                return BadRequest(new GenericMessage("Evolution failed"));
            }
            var trainer = DatabaseUtility.FindTrainerById(trainerId, gameId);
            var evolutionLog = new LogModel
            (
                user: trainer.TrainerName,
                action: $"evolved their {pokemon.Nickname} to an {evolvedForm.SpeciesName}"
            );
            DatabaseUtility.UpdateGameLogs(DatabaseUtility.FindGame(trainer.GameId), evolutionLog);
            var dexItem = DatabaseUtility.GetPokedexItem(trainerId, gameId, evolvedForm.DexNo);
            if (dexItem != null)
            {
                if (!dexItem.IsCaught)
                {
                    if (!DatabaseUtility.UpdateDexItemIsCaught(trainerId, gameId, evolvedForm.DexNo))
                    {
                        return BadRequest(new GenericMessage("Failed to update Dex Item"));
                    }
                }
            }
            else if (!DatabaseUtility.UpdateDexItemIsCaught(trainerId, gameId, evolvedForm.DexNo))
            {
                return BadRequest(new GenericMessage("Failed to update Dex Item"));
            }
            Response.RefreshToken(trainerId);
            return pokemon;
        }

        [HttpPut("{gameId}/{gameMasterId}/{trainerId}/saw")]
        public ActionResult<AbstractMessage> UpdateDexItemIsSeen(Guid gameId, Guid gameMasterId, Guid trainerId, [FromQuery] int dexNo)
        {
            var actionResult = GetDexNoForPokedexUpdate(gameId, gameMasterId, trainerId);
            if (dexNo < 1 || dexNo >= DexUtility.GetDexEntries<BasePokemonModel>(DexType.BasePokemon).Count())
            {
                return actionResult;
            }

            var dexItem = DatabaseUtility.GetPokedexItem(trainerId, gameId, dexNo);
            if (dexItem != null)
            {
                if (dexItem.IsSeen)
                {
                    return new GenericMessage("Pokemon was already seen");
                }
                if (!DatabaseUtility.UpdateDexItemIsSeen(trainerId, gameId, dexNo))
                {
                    return BadRequest(new GenericMessage("Failed to update Dex Item"));
                }

                return new GenericMessage("Pokedex updated successfully");
            }

            return AddDexItem(trainerId, gameId, dexNo, isSeen: true);
        }

        [HttpPut("{gameId}/{gameMasterId}/{trainerId}/caught")]
        public ActionResult<AbstractMessage> UpdateDexItemIsCaught(Guid gameId, Guid gameMasterId, Guid trainerId, [FromQuery] int dexNo)
        {
            var actionResult = GetDexNoForPokedexUpdate(gameId, gameMasterId, trainerId);
            if (dexNo < 1 || dexNo >= DexUtility.GetDexEntries<BasePokemonModel>(DexType.BasePokemon).Count())
            {
                return actionResult;
            }

            var dexItem = DatabaseUtility.GetPokedexItem(trainerId, gameId, dexNo);
            if (dexItem != null)
            {
                if (dexItem.IsCaught)
                {
                    return new GenericMessage("Pokemon was already caught");
                }
                if (!DatabaseUtility.UpdateDexItemIsCaught(trainerId, gameId, dexNo))
                {
                    return BadRequest(new GenericMessage("Failed to update Dex Item"));
                }

                return new GenericMessage("Pokedex updated successfully");
            }

            return AddDexItem(trainerId, gameId, dexNo, isCaught: true);
        }

        [HttpDelete("{gameId}/{gameMasterId}/{pokemonId}")]
        public ActionResult<GenericMessage> DeletePokemon(Guid gameId, Guid gameMasterId, Guid pokemonId)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            if (!DatabaseUtility.DeletePokemon(pokemonId))
            {
                return NotFound(pokemonId);
            }

            Response.RefreshToken(gameMasterId);
            return new GenericMessage($"Successfully deleted {pokemonId}");
        }

        private static PokemonModel GetDifferentForm(PokemonModel pokemon, string form)
        {
            return DexUtility.GetNewPokemon
            (
                pokemon.SpeciesName,
                Enum.Parse<Nature>(pokemon.Nature),
                Enum.Parse<Gender>(pokemon.Gender),
                Enum.Parse<Status>(pokemon.PokemonStatus),
                pokemon.Nickname,
                form
            );
        }

        private ActionResult<AbstractMessage> AddDexItem(Guid trainerId, Guid gameId, int dexNo, bool isSeen = false, bool isCaught = false)
        {
            if (isCaught)
            {
                isSeen = true;
            }

            var result = DatabaseUtility.TryAddDexItem(
                trainerId,
                gameId,
                dexNo,
                isSeen,
                isCaught,
                out var error
            );
            if (!result)
            {
                return BadRequest(error);
            }

            return new GenericMessage("Pokedex item added successfully");
        }

        private ActionResult GetDexNoForPokedexUpdate(Guid gameId, Guid gameMasterId, Guid trainerId)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            var trainer = DatabaseUtility.FindTrainerById(trainerId, gameId);
            if (trainer == null)
            {
                return NotFound(trainerId);
            }

            return null;
        }

        private static void UpdatePokemonTrainerIds(
            PokemonModel leftPokemon,
            PokemonModel rightPokemon)
        {

            DatabaseUtility.UpdatePokemonTrainerId
            (
                leftPokemon.PokemonId,
                rightPokemon.TrainerId
            );

            DatabaseUtility.UpdatePokemonLocation
            (
                leftPokemon.PokemonId,
                rightPokemon.IsOnActiveTeam
            );

            DatabaseUtility.UpdatePokemonTrainerId
            (
                rightPokemon.PokemonId,
                leftPokemon.TrainerId
            );

            DatabaseUtility.UpdatePokemonLocation
            (
                rightPokemon.PokemonId,
                leftPokemon.IsOnActiveTeam
            );
        }

        private (PokemonModel LeftPokemon, PokemonModel RightPokemon) GetTradePokemon(
            Guid leftPokemonId,
            Guid rightPokemonId,
            out ActionResult notFound)
        {
            var leftDocument = GetDocument(leftPokemonId, Collection, out notFound);
            if (leftDocument is not PokemonModel leftPokemon)
            {
                return default;
            }

            var rightDocument = GetDocument(rightPokemonId, Collection, out notFound);
            if (rightDocument is not PokemonModel rightPokemon)
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
            Guid gameId,
            Guid trainerId,
            Guid pokemonId,
            out ActionResult error)
        {
            var trainerDocument = DatabaseUtility.FindTrainerById(trainerId, gameId);
            if (trainerDocument is not TrainerModel trainer)
            {
                error = NotFound(trainerId);
                return null;
            }

            var pokemonDocument = GetDocument(pokemonId, Collection, out error);
            if (pokemonDocument is not PokemonModel pokemon)
            {
                return null;
            }

            if (!(trainer.TrainerId == pokemon.TrainerId || trainer.IsGM))
            {
                error = Unauthorized(trainer.TrainerId);
                return null;
            }

            return pokemon;
        }

        private PokemonModel GetEvolved(
            PokemonModel currentForm,
            string evolvedFormName,
            IEnumerable<string> keptMoves,
            IEnumerable<string> newMoves,
            out BadRequestObjectResult badRequest)
        {
            badRequest = null;
            if (string.IsNullOrEmpty(evolvedFormName))
            {
                badRequest = BadRequest(new GenericMessage("Missing evolvedFormName"));
                return null;
            }

            var total = keptMoves.Count() + newMoves.Count();
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
                badRequest = BadRequest(new GenericMessage($"{currentForm.Nickname} doesn't contain one of {string.Join(", ", keptMoves)}"));
                return null;
            }

            var evolvedForm = DexUtility.GetEvolved(currentForm, keptMoves, evolvedFormName, newMoves);
            if (evolvedForm == null)
            {
                badRequest = BadRequest(new GenericMessage($"Could not evolve {currentForm.Nickname} to {evolvedFormName}"));
            }

            evolvedForm.Pokeball = currentForm.Pokeball;
            return evolvedForm;
        }
    }
}
