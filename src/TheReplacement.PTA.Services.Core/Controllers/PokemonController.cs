using Microsoft.AspNetCore.Mvc;
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
        public ActionResult<PokemonModel> GetPokemon(string pokemonId)
        {
            if (string.IsNullOrEmpty(pokemonId))
            {
                return BadRequest(nameof(pokemonId));
            }

            var document = GetDocument(pokemonId, Collection, out var notFound);
            if (document is not PokemonModel pokemon)
            {
                return notFound;
            }

            return pokemon;
        }

        [HttpPut("trade")]
        public ActionResult<object> TradePokemon()
        {
            if (!Request.Query.TryGetValue("gameMasterId", out var gameMasterId))
            {
                return BadRequest(nameof(gameMasterId));
            }

            if (!Request.VerifyIdentity(gameMasterId, true))
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

            UpdatePokemonTrainerIds
            (
                leftPokemon,
                rightPokemon
            );

            var leftTrainer = DatabaseUtility.FindTrainerById(rightPokemon.TrainerId);
            var rightTrainer = DatabaseUtility.FindTrainerById(leftPokemon.TrainerId);
            var tradeLog = new LogModel
            {
                User = gameMaster.TrainerName,
                Action = $"authorized a trade between {leftTrainer.TrainerName} and {rightTrainer.TrainerName} at {DateTime.UtcNow}"
            };
            DatabaseUtility.UpdateGameLogs(DatabaseUtility.FindGame(gameMaster.GameId), tradeLog);
            Response.RefreshToken(gameMasterId);
            return new
            {
                leftPokemon,
                rightPokemon
            };
        }

        [HttpPut("{pokemonId}/form/{form}")]
        public ActionResult<PokemonModel> SwitchForm(string pokemonId, string form)
        {
            if (!Request.Query.TryGetValue("trainerId", out var trainerId))
            {
                return BadRequest(nameof(trainerId));
            }
            if (!Request.VerifyIdentity(trainerId, false))
            {
                return Unauthorized();
            }

            var pokemon = GetPokemonFromTrainer(trainerId, pokemonId, out var error);
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
            if (!DatabaseUtility.TryChangePokemonForm(result, out var writeError))
            {
                return BadRequest(writeError);
            }
            var trainer = DatabaseUtility.FindTrainerById(trainerId);
            var changedFormLog = new LogModel
            {
                User = trainer.TrainerName,
                Action = $"changed their {pokemon.Nickname} to its {form} form at {DateTime.UtcNow}"
            };
            DatabaseUtility.UpdateGameLogs(DatabaseUtility.FindGame(trainer.GameId), changedFormLog);
            Response.RefreshToken(trainerId);
            return result;
        }

        [HttpPut("{gameMasterId}/canEvolve/{pokemonId}")]
        public ActionResult<AbstractMessage> MarkPokemonAsEvolvable(string gameMasterId, string pokemonId)
        {
            if (!Request.VerifyIdentity(gameMasterId, true))
            {
                return Unauthorized();
            }

            var pokemon = DatabaseUtility.FindPokemonById(pokemonId);
            var gameMaster = DatabaseUtility.FindTrainerById(gameMasterId);
            var trainer = DatabaseUtility.FindTrainerById(pokemon.TrainerId);
            var game = DatabaseUtility.FindGame(gameMaster.GameId);
            if (pokemon == null)
            {
                return BadRequest(nameof(pokemonId));
            }

            if (trainer.GameId != game.GameId)
            {
                return BadRequest(new GenericMessage($"Pokemon {pokemonId} is not part of the same game as {gameMasterId}"));
            }

            if (!DatabaseUtility.UpdatePokemonEvolvability(pokemonId, true))
            {
                return BadRequest(new GenericMessage($"Failed to mark pokemon {pokemonId} as evolvable"));
            }
            var evolutionLog = new LogModel
            {
                User = trainer.TrainerName,
                Action = $"can now evolve their {pokemon.Nickname} at {DateTime.UtcNow}"
            };
            DatabaseUtility.UpdateGameLogs(DatabaseUtility.FindGame(trainer.GameId), evolutionLog);
            Response.RefreshToken(gameMasterId);
            return Ok();
        }

        [HttpPut("{trainerId}/possibleEvolutions/{pokemonId}")]
        public ActionResult<IEnumerable<BasePokemonModel>> GetPossibleEvolutions(string trainerId, string pokemonId)
        {
            if (!Request.VerifyIdentity(trainerId, false))
            {
                return Unauthorized();
            }

            var pokemon = GetPokemonFromTrainer(trainerId, pokemonId, out var error);
            if (pokemon == null)
            {
                return error;
            }

            return DexUtility.GetPossibleEvolutions(pokemon).ToList();
        }

        [HttpPut("{trainerId}/evolve/{pokemonId}")]
        public async Task<ActionResult<PokemonModel>> EvolvePokemonAsync(string trainerId, string pokemonId)
        {
            if (!Request.VerifyIdentity(trainerId, false))
            {
                return Unauthorized();
            }

            var pokemon = GetPokemonFromTrainer(trainerId, pokemonId, out var error);
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
            var trainer = DatabaseUtility.FindTrainerById(trainerId);
            var evolutionLog = new LogModel
            {
                User = trainer.TrainerName,
                Action = $"evolved their {pokemon.Nickname} to an {evolvedForm.SpeciesName} at {DateTime.UtcNow}"
            };
            DatabaseUtility.UpdateGameLogs(DatabaseUtility.FindGame(trainer.GameId), evolutionLog);
            var dexItem = DatabaseUtility.GetPokedexItem(trainerId, evolvedForm.DexNo);
            if (dexItem != null)
            {
                if (!dexItem.IsCaught)
                {
                    if (!DatabaseUtility.UpdateDexItemIsCaught(trainerId, evolvedForm.DexNo))
                    {
                        return BadRequest(new GenericMessage("Failed to update Dex Item"));
                    }
                }
            }
            else if (!DatabaseUtility.UpdateDexItemIsCaught(trainerId, evolvedForm.DexNo))
            {
                return BadRequest(new GenericMessage("Failed to update Dex Item"));
            }
            Response.RefreshToken(trainerId);
            return pokemon;
        }

        [HttpPut("{trainerId}/saw")]
        public ActionResult<AbstractMessage> UpdateDexItemIsSeen(string trainerId)
        {
            var dexNo = GetDexNoForPokedexUpdate(trainerId, out var actionResult);
            if (dexNo < 1 || dexNo >= DexUtility.GetDexEntries<BasePokemonModel>(DexType.BasePokemon).Count())
            {
                return actionResult;
            }

            var dexItem = DatabaseUtility.GetPokedexItem(trainerId, dexNo);
            if (dexItem != null)
            {
                if (dexItem.IsSeen)
                {
                    return new GenericMessage("Pokemon was already seen");
                }
                if (!DatabaseUtility.UpdateDexItemIsSeen(trainerId, dexNo))
                {
                    return BadRequest(new GenericMessage("Failed to update Dex Item"));
                }

                return new GenericMessage("Pokedex updated successfully");
            }

            return AddDexItem(trainerId, dexNo, isSeen: true);
        }

        [HttpPut("{trainerId}/caught")]
        public ActionResult<AbstractMessage> UpdateDexItemIsCaught(string trainerId)
        {
            var dexNo = GetDexNoForPokedexUpdate(trainerId, out var actionResult);
            if (dexNo < 1 || dexNo >= DexUtility.GetDexEntries<BasePokemonModel>(DexType.BasePokemon).Count())
            {
                return actionResult;
            }

            var dexItem = DatabaseUtility.GetPokedexItem(trainerId, dexNo);
            if (dexItem != null)
            {
                if (dexItem.IsCaught)
                {
                    return new GenericMessage("Pokemon was already caught");
                }
                if (!DatabaseUtility.UpdateDexItemIsCaught(trainerId, dexNo))
                {
                    return BadRequest(new GenericMessage("Failed to update Dex Item"));
                }

                return new GenericMessage("Pokedex updated successfully");
            }

            return AddDexItem(trainerId, dexNo, isCaught: true);
        }

        [HttpDelete("{pokemonId}")]
        public ActionResult<GenericMessage> DeletePokemon(string pokemonId)
        {
            if (!Request.Query.TryGetValue("gameMasterId", out var gameMasterId))
            {
                return BadRequest(nameof(gameMasterId));
            }

            if (!Request.VerifyIdentity(gameMasterId, true))
            {
                return Unauthorized();
            }

            if (!IsGMOnline(gameMasterId))
            {
                return NotFound(gameMasterId);
            }

            var pokemon = DatabaseUtility.FindPokemonById(pokemonId);
            if (!DatabaseUtility.DeletePokemon(pokemonId))
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve pokemon {pokemonId}");
                return NotFound(pokemonId);
            }

            var gameMaster = DatabaseUtility.FindTrainerById(gameMasterId);
            var deletionLog = new LogModel
            {
                User = gameMaster.TrainerName,
                Action = $"removed {pokemon.Nickname} ({pokemonId}) at {DateTime.UtcNow}"
            };

            DatabaseUtility.UpdateGameLogs(DatabaseUtility.FindGame(gameMaster.GameId), deletionLog);
            Response.RefreshToken(gameMasterId);
            return new GenericMessage($"Successfully deleted {pokemonId}");
        }

        private static PokemonModel GetDifferentForm(PokemonModel pokemon, string form)
        {
            return DexUtility.GetNewPokemon(pokemon.SpeciesName, Enum.Parse<Nature>(pokemon.Nature), Enum.Parse<Gender>(pokemon.Gender), Enum.Parse<Status>(pokemon.PokemonStatus), pokemon.Nickname, form);
        }

        private ActionResult<AbstractMessage> AddDexItem(string trainerId, int dexNo, bool isSeen = false, bool isCaught = false)
        {
            if (isCaught)
            {
                isSeen = true;
            }

            var result = DatabaseUtility.TryAddDexItem(
                trainerId,
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

        private int GetDexNoForPokedexUpdate(string trainerId, out ActionResult actionResult)
        {
            if (!Request.Query.TryGetValue("gameMasterId", out var gameMasterId))
            {
                actionResult = BadRequest(nameof(gameMasterId));
                return -1;
            }
            if (!Request.VerifyIdentity(gameMasterId, true))
            {
                actionResult = Unauthorized();
                return -1;
            }

            if (!(Request.Query.TryGetValue("dexNo", out var dexNoQueryValue) && int.TryParse(dexNoQueryValue, out var dexNo)))
            {
                actionResult = BadRequest(nameof(dexNo));
                return -1;
            }

            var document = GetDocument(trainerId, MongoCollection.Trainers, out actionResult);
            if (!(document is TrainerModel))
            {
                return -1;
            }
            
            return dexNo;
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

        private (PokemonModel LeftPokemon, PokemonModel RightPokemon) GetTradePokemon(out ActionResult notFound)
        {
            if (!Request.Query.TryGetValue("leftPokemonId", out var leftPokemonId))
            {
                notFound = BadRequest(nameof(leftPokemonId));
                return default;
            }
            var leftDocument = GetDocument(leftPokemonId, Collection, out notFound);
            if (leftDocument is not PokemonModel leftPokemon)
            {
                return default;
            }

            if (!Request.Query.TryGetValue("rightPokemonId", out var rightPokemonId))
            {
                notFound = BadRequest(nameof(rightPokemonId));
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
            string trainerId,
            string pokemonId,
            out ActionResult error)
        {
            var trainerDocument = GetDocument(trainerId, MongoCollection.Trainers, out error);
            if (trainerDocument is not TrainerModel trainer)
            {
                return null;
            }

            var pokemonDocument = GetDocument(pokemonId, MongoCollection.Pokemon, out error);
            if (pokemonDocument is not PokemonModel pokemon)
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

            return evolvedForm;
        }
    }
}
