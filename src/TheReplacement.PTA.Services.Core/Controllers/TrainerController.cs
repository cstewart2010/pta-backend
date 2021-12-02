﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using TheReplacement.PTA.Common.Utilities;
using TheReplacement.PTA.Common.Models;
using TheReplacement.PTA.Common.Enums;
using TheReplacement.PTA.Services.Core.Extensions;
using TheReplacement.PTA.Services.Core.Messages;

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
        public ActionResult<PokemonModel> AddPokemon(string trainerId)
        {
            if (!Request.Query.TryGetValue("gameMasterId", out var gameMasterId))
            {
                return BadRequest(nameof(gameMasterId));
            }

            if (!Request.VerifyIdentity(gameMasterId))
            {
                return Unauthorized();
            }

            if (!IsGMOnline(gameMasterId))
            {
                return NotFound(gameMasterId);
            }

            var pokemon = Request.BuildPokemon(trainerId, out var error);
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
        public ActionResult<FoundTrainerMessage> Login()
        {
            var (gameId, username, password) = Request.GetTrainerCredentials(out var credentialErrors);
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
        public ActionResult Logout(string trainerId)
        {
            if (!Request.VerifyIdentity(trainerId))
            {
                return Unauthorized();
            }

            var trainerDocument = GetDocument(trainerId, Collection, out var notFound);
            if (!(trainerDocument is TrainerModel trainer))
            {
                return notFound;
            }

            DatabaseUtility.UpdateTrainerOnlineStatus(trainer.TrainerId, false);
            return ReturnSuccessfully(Ok());
        }

        [HttpPut("{trainerId}/addItems")]
        public ActionResult<FoundTrainerMessage> AddItemsToTrainer(string trainerId)
        {
            if (!Request.Query.TryGetValue("gameMasterId", out var gameMasterId))
            {
                return BadRequest(nameof(gameMasterId));
            }
            if (!Request.VerifyIdentity(gameMasterId))
            {
                return Unauthorized();
            }

            if (!IsGMOnline(gameMasterId))
            {
                return Unauthorized(gameMasterId);
            }

            var trainer = DatabaseUtility.FindTrainerById(trainerId);
            if (trainer == null)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve trainer {trainer.TrainerId}");
                return NotFound(trainerId);
            }

            var itemList = trainer.Items;
            foreach (var itemName in Request.Query.Keys)
            {
                UpdateAllItemsWithAddition
                (
                    itemList,
                    itemName,
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
            Response.RefreshToken(gameMasterId);
            return ReturnSuccessfully(new FoundTrainerMessage(DatabaseUtility.FindTrainerById(trainerId)));
        }

        [HttpPut("{trainerId}/removeItems")]
        public ActionResult<FoundTrainerMessage> RemoveItemsFromTrainer(string trainerId)
        {
            if (!Request.VerifyIdentity(trainerId))
            {
                return Unauthorized();
            }

            var trainer = DatabaseUtility.FindTrainerById(trainerId);
            if (trainer?.IsOnline == true)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve trainer {trainerId}");
                return NotFound(trainerId);
            }

            var itemList = trainer.Items;
            foreach (var itemName in Request.Query.Keys)
            {
                UpdateAllItemsWithReduction(itemList, itemName, trainer);
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

            if (!Request.VerifyIdentity(gameMasterId))
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

        private void UpdateAllItemsWithReduction(
            List<ItemModel> itemList,
            string itemName,
            TrainerModel trainer)
        {
            var (itemReduction, badDataObject) = GetCleanData(itemName);
            if (badDataObject != null)
            {
                return;
            }

            var item = trainer.Items.FirstOrDefault(item => item.Name.Equals(itemName, StringComparison.CurrentCultureIgnoreCase));
            if ((item?.Amount) >= itemReduction)
            {
                itemList = trainer.Items
                    .Select(item => UpdateItemWithReduction(item, itemName, itemReduction))
                    .Where(item => item.Amount > 0)
                    .ToList();
            }
        }

        private static ItemModel UpdateItemWithReduction(
            ItemModel item,
            string itemName,
            int itemReduction)
        {
            if (item.Name == itemName)
            {
                return new ItemModel
                {
                    Name = itemName,
                    Amount = item.Amount - itemReduction
                };
            }

            return item;
        }

        private void UpdateAllItemsWithAddition(
            List<ItemModel> itemList,
            string itemName,
            TrainerModel trainer)
        {
            var (itemIncrease, badDataObject) = GetCleanData(itemName);
            if (badDataObject != null)
            {
                return;
            }

            var item = trainer.Items.FirstOrDefault(item => item.Name.Equals(itemName, StringComparison.CurrentCultureIgnoreCase));
            if (item == null)
            {
                itemList.Add(new ItemModel
                {
                    Name = itemName,
                    Amount = itemIncrease
                });
            }
            else
            {
                itemList = trainer.Items.Select(item => UpdateItemWithAddition(item, itemName, itemIncrease)).ToList();
            }
        }

        private static ItemModel UpdateItemWithAddition(
            ItemModel item,
            string itemName,
            int itemIncrease)
        {
            if (item.Name == itemName)
            {
                return new ItemModel
                {
                    Name = itemName,
                    Amount = item.Amount + itemIncrease > 100
                        ? 100
                        : item.Amount + itemIncrease
                };
            }

            return item;
        }

        private (int UpdatedAmount, AbstractMessage Error) GetCleanData(string itemName)
        {
            var change = Request.Query[itemName];
            if (!int.TryParse(change, out var itemChange))
            {
                return
                (
                    0,
                    new GenericMessage($"No ${itemName} change listed")
                );
            }
            if (itemChange < 1)
            {
                return
                (
                    0,
                    new GenericMessage($"Should not change ${itemName} count less than 1")
                );
            }
            else if (itemChange > 100)
            {
                itemChange = 100;
            }

            return (itemChange, null);
        }
    }
}