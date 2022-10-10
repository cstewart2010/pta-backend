using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using TheReplacement.PTA.Common.Utilities;
using TheReplacement.PTA.Common.Models;
using TheReplacement.PTA.Common.Enums;
using TheReplacement.PTA.Services.Core.Extensions;
using TheReplacement.PTA.Services.Core.Messages;
using TheReplacement.PTA.Services.Core.Objects;
using System.Threading.Tasks;

namespace TheReplacement.PTA.Services.Core.Controllers
{
    [ApiController]
    [Route("api/v1/trainer")]
    public class TrainerController : PtaControllerBase
    {
        protected override MongoCollection Collection { get; }

        public TrainerController()
        {
            Collection = MongoCollection.Trainers;
        }

        [HttpGet("{gameId}/trainers")]
        public ActionResult<IEnumerable<PublicTrainer>> FindTrainers(Guid gameId)
        {
            return GetTrainers(gameId);
        }

        [HttpGet("trainers/{trainerName}")]
        public ActionResult<FoundTrainerMessage> FindTrainer(string trainerName, [FromQuery] Guid gameId)
        {
            var trainer = DatabaseUtility.FindTrainerByUsername(trainerName, gameId);
            return new FoundTrainerMessage(trainer.TrainerId, gameId);
        }

        [HttpGet("{gameId}/{trainerId}/{pokemonId}")]
        public ActionResult<PokemonModel> FindTrainerMon(
            Guid gameId,
            Guid trainerId,
            Guid pokemonId)
        {
            var document = GetDocument(pokemonId, MongoCollection.Pokemon, out var notFound);
            if (document is not PokemonModel pokemon)
            {
                return notFound;
            }
            if (pokemon.TrainerId != trainerId && pokemon.GameId != gameId)
            {
                return BadRequest(new PokemonTrainerMismatchMessage(pokemon.TrainerId, trainerId));
            }

            return pokemon;
        }

        [HttpPost("{gameId}/{gameMasterId}/{trainerId}")]
        public ActionResult<PokemonModel> AddPokemon(Guid gameId, Guid gameMasterId, Guid trainerId, [FromQuery] WildPokemon wild)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            var (pokemon, error) = BuildPokemon(trainerId, gameId, wild);
            if (pokemon == null)
            {
                return BadRequest(error);
            }
            if (!DatabaseUtility.TryAddPokemon(pokemon, out var writeError))
            {
                return BadRequest(writeError);
            }

            Response.RefreshToken(gameMasterId);
            return pokemon;
        }

        [HttpPut("{gameId}/{gameMasterId}/groupHonor")]
        public async Task<ActionResult<GenericMessage>> AddGroupHonor(Guid gameId, Guid gameMasterId)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            var body = await Request.GetRequestBody();
            var honor = body["honor"].ToString();
            if (string.IsNullOrEmpty(honor))
            {
                return BadRequest(nameof(honor));
            }
            var trainers = DatabaseUtility.FindTrainersByGameId(gameId);
            foreach (var trainer in trainers)
            {
                DatabaseUtility.UpdateTrainerHonors(trainer.TrainerId, trainer.Honors.Append(honor));
            }

            var updatedHonorsLog = new LogModel
            {
                User = "The party",
                Action = $"has earned a new honor: {honor}"
            };

            DatabaseUtility.UpdateGameLogs(DatabaseUtility.FindGame(gameId), updatedHonorsLog);
            Response.RefreshToken(gameMasterId);
            return new GenericMessage($"Granted the party honor: {honor}");
        }

        [HttpPut("{gameId}/{gameMasterId}/honor")]
        public async Task<ActionResult<GenericMessage>> AddSingleHonor(Guid gameId, Guid gameMasterId)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            var body = await Request.GetRequestBody();
            var honor = body["honor"].ToString();
            var trainerId = (Guid)body["trainerId"];
            var trainer = DatabaseUtility.FindTrainerById(trainerId, gameId);
            var gameMaster = DatabaseUtility.FindTrainerById(gameMasterId, gameId);
            if (gameMaster.GameId != trainer.GameId)
            {
                return Unauthorized("Mismatched games");
            }
            if (string.IsNullOrEmpty(honor))
            {
                return BadRequest(nameof(honor));
            }
            if (!DatabaseUtility.UpdateTrainerHonors(trainerId, trainer.Honors.Append(honor)))
            {
                throw new Exception();
            }

            var updatedHonorsLog = new LogModel
            {
                User = gameMaster.TrainerName,
                Action = $"has granted {trainer.TrainerName} a new honor"
            };

            DatabaseUtility.UpdateGameLogs(DatabaseUtility.FindGame(gameMaster.GameId), updatedHonorsLog);
            Response.RefreshToken(gameMasterId);
            return new GenericMessage($"Granted {trainerId} honor: {honor}");
        }

        [HttpPut("{gameId}/{trainerId}/addItems")]
        public async Task<ActionResult<FoundTrainerMessage>> AddItemsToTrainerAsync(Guid gameId, Guid trainerId)
        {
            if (!Request.VerifyIdentity(trainerId))
            {
                return Unauthorized();
            }

            var trainer = DatabaseUtility.FindTrainerById(trainerId, gameId);
            if (trainer == null)
            {
                return NotFound(trainerId);
            }

            var items = (await Request.GetRequestBody()).Select(token => token.ToObject<ItemModel>());
            var addedItemsLogs = AddItemsToTrainer(trainer, items);
            DatabaseUtility.UpdateGameLogs(DatabaseUtility.FindGame(trainer.GameId), addedItemsLogs.ToArray());
            Response.RefreshToken(trainerId);
            return new FoundTrainerMessage(trainerId, gameId);
        }

        [HttpPut("{gameId}/{trainerId}/removeItems")]
        public async Task<ActionResult<FoundTrainerMessage>> RemoveItemsFromTrainerAsync(Guid gameId, Guid trainerId)
        {
            if (!Request.VerifyIdentity(trainerId))
            {
                return Unauthorized();
            }

            var trainer = DatabaseUtility.FindTrainerById(trainerId, gameId);
            if (trainer?.IsOnline != true)
            {
                return NotFound(trainerId);
            }

            var items = (await Request.GetRequestBody()).Select(token => token.ToObject<ItemModel>());
            var removedItemsLogs = RemoveItemsFromTrainer(trainer, items);
            DatabaseUtility.UpdateGameLogs(DatabaseUtility.FindGame(trainer.GameId), removedItemsLogs.ToArray());
            Response.RefreshToken(trainerId);
            return new FoundTrainerMessage(trainerId, gameId);
        }

        [HttpDelete("{gameId}/{gameMasterId}/{trainerId}")]
        public ActionResult<GenericMessage> DeleteTrainer(Guid gameId, Guid trainerId, Guid gameMasterId)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            var gameMaster = DatabaseUtility.FindTrainerById(trainerId, gameId);
            if (gameMaster?.IsGM != true)
            {
                return NotFound(trainerId);
            }

            var trainer = DatabaseUtility.FindTrainerById(trainerId, gameId);
            foreach (var pokemon in DatabaseUtility.FindPokemonByTrainerId(trainerId))
            {
                DatabaseUtility.DeletePokemon(pokemon.PokemonId);
            }

            if (!(DatabaseUtility.DeleteTrainer(gameId, trainerId) && DatabaseUtility.FindTrainerById(trainerId, gameId) == null))
            {
                return NotFound();
            }

            var deleteTrainerLog = new LogModel
            {
                User = gameMaster.TrainerName,
                Action = $"removed {trainer.TrainerName} and all of their pokemon from the game at {DateTime.UtcNow}"
            };
            DatabaseUtility.UpdateGameLogs(DatabaseUtility.FindGame(gameMaster.GameId), deleteTrainerLog);
            Response.RefreshToken(gameMasterId);
            return new GenericMessage($"Successfully deleted all pokemon associated with {trainerId}");
        }

        private static List<PublicTrainer> GetTrainers(Guid gameId)
        {
            return DatabaseUtility.FindTrainersByGameId(gameId)
                .Select(trainer => new PublicTrainer(trainer)).ToList();
        }
    }
}
