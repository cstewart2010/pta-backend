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
        public ActionResult<IEnumerable<PublicTrainer>> FindTrainers(string gameId)
        {
            return GetTrainers(gameId);
        }

        [HttpGet("trainers/{trainerName}")]
        public ActionResult<FoundTrainerMessage> FindTrainer(string trainerName)
        {
            if (!Request.Query.TryGetValue("gameId", out var gameId))
            {
                return BadRequest(nameof(gameId));
            }

            var trainer = DatabaseUtility.FindTrainerByUsername(trainerName, gameId);
            return new FoundTrainerMessage(trainer.TrainerId, gameId);
        }

        [HttpGet("{gameId}/{trainerId}/{pokemonId}")]
        public ActionResult<PokemonModel> FindTrainerMon(
            string gameId,
            string trainerId,
            string pokemonId)
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
        public async Task<ActionResult<PokemonModel>> AddPokemon(string gameId, string gameMasterId, string trainerId)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            var (pokemon, error) = await Request.BuildPokemon(trainerId, gameId);
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
        public async Task<ActionResult<GenericMessage>> AddGroupHonor(string gameId, string gameMasterId)
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
        public async Task<ActionResult<GenericMessage>> AddSingleHonor(string gameId, string gameMasterId)
        {
            if (!Request.IsUserGM(gameMasterId, gameId))
            {
                return Unauthorized();
            }

            var body = await Request.GetRequestBody();
            var honor = body["honor"].ToString();
            var trainerId = body["trainerId"].ToString();
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
        public async Task<ActionResult<FoundTrainerMessage>> AddItemsToTrainerAsync(string gameId, string trainerId)
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

            var itemList = trainer.Items;
            var items = (await Request.GetRequestBody()).Select(token => token.ToObject<ItemModel>());
            foreach (var item in items)
            {
                itemList = UpdateAllItemsWithAddition
                (
                    itemList,
                    item,
                    trainer
                );
            }

            var result = DatabaseUtility.UpdateTrainerItemList
            (
                trainerId,
                itemList
            );

            if (!result)
            {
                throw new Exception();
            }

            var addedItemsLogs = items.Select(item => new LogModel
            {
                User = trainer.TrainerName,
                Action = $"added ({item.Amount}) {item.Name} at {DateTime.UtcNow}"
            });
            DatabaseUtility.UpdateGameLogs(DatabaseUtility.FindGame(trainer.GameId), addedItemsLogs.ToArray());
            Response.RefreshToken(trainerId);
            return new FoundTrainerMessage(trainerId, gameId);
        }

        [HttpPut("{gameId}/{trainerId}/removeItems")]
        public async Task<ActionResult<FoundTrainerMessage>> RemoveItemsFromTrainerAsync(string gameId, string trainerId)
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

            var itemList = trainer.Items;
            var items = (await Request.GetRequestBody()).Select(token => token.ToObject<ItemModel>());
            foreach (var item in items)
            {
                itemList = UpdateAllItemsWithReduction
                (
                    itemList,
                    item,
                    trainer
                );
            }

            var result = DatabaseUtility.UpdateTrainerItemList
            (
                trainerId,
                itemList
            );
            if (!result)
            {
                throw new Exception();
            }

            var removedItemsLogs = items.Select(item => new LogModel
            {
                User = trainer.TrainerName,
                Action = $"removed ({item.Amount}) {item.Name} at {DateTime.UtcNow}"
            });
            DatabaseUtility.UpdateGameLogs(DatabaseUtility.FindGame(trainer.GameId), removedItemsLogs.ToArray());
            Response.RefreshToken(trainerId);
            return new FoundTrainerMessage(trainerId, gameId);
        }

        [HttpDelete("{gameId}/{gameMasterId}/{trainerId}")]
        public ActionResult<GenericMessage> DeleteTrainer(string gameId, string trainerId, string gameMasterId)
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

            if (!(DatabaseUtility.DeleteTrainer(trainerId) && DatabaseUtility.FindTrainerById(trainerId, gameId) == null))
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

        private static List<ItemModel> UpdateAllItemsWithReduction(
            List<ItemModel> itemList,
            ItemModel itemToken,
            TrainerModel trainer)
        {
            var item = trainer.Items.FirstOrDefault(item => item.Name.Equals(itemToken.Name, StringComparison.CurrentCultureIgnoreCase));
            if ((item?.Amount ?? 0) >= itemToken.Amount)
            {
                itemList = trainer.Items
                    .Select(item => UpdateItemWithReduction(item, itemToken))
                    .Where(item => item.Amount > 0)
                    .ToList();
            }

            return itemList;
        }

        private static List<ItemModel> UpdateAllItemsWithAddition(
            List<ItemModel> itemList,
            ItemModel itemToken,
            TrainerModel trainer)
        {
            var item = trainer.Items.FirstOrDefault(item => item.Name.Equals(itemToken.Name, StringComparison.CurrentCultureIgnoreCase));
            if (item == null)
            {
                itemList.Add(itemToken);
            }
            else
            {
                itemList = trainer.Items.Select(item => UpdateItemWithAddition(item, itemToken)).ToList();
            }

            return itemList;
        }

        private static List<PublicTrainer> GetTrainers(string gameId)
        {
            return DatabaseUtility.FindTrainersByGameId(gameId)
                .Select(trainer => new PublicTrainer(trainer)).ToList();
        }

        private static ItemModel UpdateItemWithReduction(
            ItemModel item,
            ItemModel newItem)
        {
            if (item.Name == newItem.Name)
            {
                item.Amount -= newItem.Amount;
            }

            return item;
        }

        private static ItemModel UpdateItemWithAddition(
            ItemModel item,
            ItemModel newItem)
        {
            if (item.Name == newItem.Name)
            {
                item.Amount = item.Amount + newItem.Amount > 100
                        ? 100
                        : item.Amount + newItem.Amount;
            }

            return item;
        }
    }
}
