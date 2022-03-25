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
using Newtonsoft.Json.Linq;

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

        [HttpGet("refreshGM")]
        public ActionResult<GameMasterMessage> RefreshGM()
        {
            if (!Request.Query.TryGetValue("gameMasterId", out var gameMasterId))
            {
                return BadRequest(nameof(gameMasterId));
            }

            if (!Request.VerifyIdentity(gameMasterId, true))
            {
                return Unauthorized();
            }

            Response.RefreshToken(gameMasterId);
            return ReturnSuccessfully(new GameMasterMessage(gameMasterId));
        }

        [HttpGet("refreshTrainer")]
        public ActionResult<FoundTrainerMessage> RefreshTrainer()
        {
            if (!Request.Query.TryGetValue("trainerId", out var trainerId))
            {
                return BadRequest(nameof(trainerId));
            }

            if (!Request.VerifyIdentity(trainerId, false))
            {
                return Unauthorized();
            }

            Response.RefreshToken(trainerId);
            return ReturnSuccessfully(new FoundTrainerMessage(DatabaseUtility.FindTrainerById(trainerId)));
        }

        [HttpGet("trainers")]
        public ActionResult<IEnumerable<PublicTrainer>> FindTrainers()
        {
            return FindTrainers(null);
        }

        [HttpGet("trainers/{trainerName}")]
        public ActionResult<FoundTrainerMessage> FindTrainer(string trainerName)
        {
            if (!Request.Query.TryGetValue("gameId", out var gameId))
            {
                return BadRequest(nameof(gameId));
            }

            var trainer = DatabaseUtility.FindTrainerByUsername(trainerName, gameId);
            return ReturnSuccessfully(new FoundTrainerMessage(trainer));
        }

        [HttpGet("{trainerId}/{pokemonId}")]
        public ActionResult<PokemonModel> FindTrainerMon(
            string trainerId,
            string pokemonId)
        {
            var document = GetDocument(pokemonId, MongoCollection.Pokemon, out var notFound);
            if (!(document is PokemonModel pokemon))
            {
                return notFound;
            }
            if (pokemon.TrainerId != trainerId)
            {
                return BadRequest(new PokemonTrainerMismatchMessage(pokemon.TrainerId, trainerId));
            }

            return ReturnSuccessfully(pokemon);
        }

        [HttpPost("{trainerId}")]
        public async Task<ActionResult<PokemonModel>> AddPokemon(string trainerId)
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

            var (pokemon, error) = await Request.BuildPokemon(trainerId);
            if (pokemon == null)
            {
                return BadRequest(error);
            }
            if (!DatabaseUtility.TryAddPokemon(pokemon, out var writeError))
            {
                return BadRequest(writeError);
            }

            Response.RefreshToken(gameMasterId);
            return ReturnSuccessfully(pokemon);
        }

        [HttpPut("login")]
        public async Task<ActionResult<FoundTrainerMessage>> Login()
        {
            var (gameId, username, password, credentialErrors) = await Request.GetTrainerCredentials();
            if (credentialErrors.Any())
            {
                return BadRequest(credentialErrors);
            }

            if (!IsTrainerAuthenticated(username, password, gameId, false, out var authError))
            {
                return authError;
            }

            var trainer = DatabaseUtility.FindTrainerByUsername(username, gameId);
            Response.AssignAuthAndToken(trainer.TrainerId);
            return ReturnSuccessfully(new FoundTrainerMessage(trainer));
        }

        [HttpPut("{trainerId}/logout")]
        public ActionResult<AbstractMessage> Logout(string trainerId)
        {
            if (!Request.VerifyIdentity(trainerId, false))
            {
                return Unauthorized();
            }

            var trainerDocument = GetDocument(trainerId, Collection, out var notFound);
            if (!(trainerDocument is TrainerModel trainer))
            {
                return notFound;
            }

            DatabaseUtility.UpdateTrainerOnlineStatus(trainer.TrainerId, false);
            return ReturnSuccessfully(new GenericMessage(""));
        }

        [HttpPut("{trainerId}/addItems")]
        public async Task<ActionResult<FoundTrainerMessage>> AddItemsToTrainerAsync(string trainerId)
        {
            if (!Request.VerifyIdentity(trainerId, false))
            {
                return Unauthorized();
            }

            var trainer = DatabaseUtility.FindTrainerById(trainerId);
            if (trainer == null)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve trainer {trainer.TrainerId}");
                return NotFound(trainerId);
            }

            var itemList = trainer.Items;
            var items = await Request.GetRequestBody();
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
            Response.RefreshToken(trainerId);
            return ReturnSuccessfully(new FoundTrainerMessage(DatabaseUtility.FindTrainerById(trainerId)));
        }

        [HttpPut("{trainerId}/removeItems")]
        public async Task<ActionResult<FoundTrainerMessage>> RemoveItemsFromTrainerAsync(string trainerId)
        {
            if (!Request.VerifyIdentity(trainerId, false))
            {
                return Unauthorized();
            }

            var trainer = DatabaseUtility.FindTrainerById(trainerId);
            if (trainer?.IsOnline != true)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve trainer {trainerId}");
                return NotFound(trainerId);
            }

            var itemList = trainer.Items;
            var items = await Request.GetRequestBody();
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
            Response.RefreshToken(trainerId);
            return ReturnSuccessfully(new FoundTrainerMessage(DatabaseUtility.FindTrainerById(trainerId)));
        }

        [HttpDelete("{trainerId}")]
        public ActionResult<GenericMessage> DeleteTrainer(string trainerId)
        {
            if (!Request.Query.TryGetValue("gameMasterId", out var gameMasterId))
            {
                return BadRequest(nameof(gameMasterId));
            }

            if (!Request.VerifyIdentity(gameMasterId, true))
            {
                return Unauthorized();
            }

            if (!(GetDocument(gameMasterId, Collection, out var error) is TrainerModel gameMaster && gameMaster.IsGM))
            {
                return error;
            }

            foreach (var pokemon in DatabaseUtility.FindPokemonByTrainerId(trainerId))
            {
                DatabaseUtility.DeletePokemon(pokemon.PokemonId);
            }

            if (!(DatabaseUtility.DeleteTrainer(trainerId) && DatabaseUtility.FindTrainerById(trainerId) == null))
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to delete trainer {trainerId}");
                return NotFound();
            }

            Response.RefreshToken(gameMasterId);
            return ReturnSuccessfully(new GenericMessage($"Successfully deleted all pokemon associated with {trainerId}"));
        }

        private ActionResult<IEnumerable<PublicTrainer>> FindTrainers(string gameMasterId)
        {
            if (!Request.Query.TryGetValue("gameId", out var gameId))
            {
                return BadRequest(nameof(gameId));
            }

            if (!string.IsNullOrEmpty(gameMasterId))
            {
                Response.RefreshToken(gameMasterId);
            }
            return ReturnSuccessfully(GetTrainers(gameId));
        }

        private static List<ItemModel> UpdateAllItemsWithReduction(
            List<ItemModel> itemList,
            JToken itemToken,
            TrainerModel trainer)
        {
            var newItem = itemToken.ToObject<ItemModel>();
            //var (itemReduction, badDataObject) = GetCleanData(itemName);
            //if (badDataObject != null)
            //{
            //    return;
            //}

            var item = trainer.Items.FirstOrDefault(item => item.Name.Equals(newItem.Name, StringComparison.CurrentCultureIgnoreCase));
            if ((item?.Amount ?? 0) >= newItem.Amount)
            {
                itemList = trainer.Items
                    .Select(item => UpdateItemWithReduction(item, newItem))
                    .Where(item => item.Amount > 0)
                    .ToList();
            }

            return itemList;
        }

        private static List<ItemModel> UpdateAllItemsWithAddition(
            List<ItemModel> itemList,
            JToken itemToken,
            TrainerModel trainer)
        {
            var newItem = itemToken.ToObject<ItemModel>();
            //var (itemIncrease, badDataObject) = GetCleanData(itemToken);
            //if (badDataObject != null)
            //{
            //    return;
            //}

            var item = trainer.Items.FirstOrDefault(item => item.Name.Equals(newItem.Name, StringComparison.CurrentCultureIgnoreCase));
            if (item == null)
            {
                itemList.Add(newItem);
            }
            else
            {
                itemList = trainer.Items.Select(item => UpdateItemWithAddition(item, newItem)).ToList();
            }

            return itemList;
        }

        //private (int UpdatedAmount, AbstractMessage Error) GetCleanData(JToken itemToken)
        //{
        //    var change = Request.Query[itemToken];
        //    if (!int.TryParse(change, out var itemChange))
        //    {
        //        return
        //        (
        //            0,
        //            new GenericMessage($"No ${itemToken} change listed")
        //        );
        //    }
        //    if (itemChange < 1)
        //    {
        //        return
        //        (
        //            0,
        //            new GenericMessage($"Should not change ${itemToken} count less than 1")
        //        );
        //    }
        //    else if (itemChange > 100)
        //    {
        //        itemChange = 100;
        //    }

        //    return (itemChange, null);
        //}

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
