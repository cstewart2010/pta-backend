using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using TheReplacements.PTA.Common.Utilities;
using TheReplacements.PTA.Common.Models;
using TheReplacements.PTA.Common.Enums;
using TheReplacements.PTA.Services.Core.Extensions;

namespace TheReplacements.PTA.Services.Core.Controllers
{
    [ApiController]
    [Route("api/v1/trainer")]
    public class TrainerContoller : PtaControllerBase
    {
        protected override MongoCollection Collection { get; }

        public TrainerContoller()
        {
            Collection = MongoCollection.Trainer;
        }

        [HttpGet("{trainerId}/{pokemonId}")]
        public ActionResult<PokemonModel> FindTrainerMon(
            string trainerId,
            string pokemonId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            var document = GetDocument(pokemonId, MongoCollection.Pokemon, out var notFound);
            if (!(document is PokemonModel pokemon))
            {
                return notFound;
            }
            if (pokemon.TrainerId != trainerId)
            {
                return BadRequest(new
                {
                    message = "Invalid trainerId",
                    expected = trainerId,
                    found = pokemon.TrainerId
                });
            }

            return ReturnSuccessfully(pokemon);
        }

        [HttpPost("{trainerId}")]
        public ActionResult<PokemonModel> AddPokemon(string trainerId)
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

            var pokemon = Request.BuildPokemon(trainerId, out var error);
            if (pokemon == null)
            {
                return BadRequest(error);
            }
            if (!DatabaseUtility.TryAddPokemon(pokemon, out var writeError))
            {
                return BadRequest(writeError);
            }

            Response.RefreshToken();
            return ReturnSuccessfully(pokemon);
        }

        [HttpPut("login")]
        public ActionResult<object> Login()
        {
            Response.UpdateAccessControl();
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
            Response.AssignAuthAndToken();
            return ReturnSuccessfully(new
            {
                trainer.TrainerId,
                trainer.TrainerName,
                trainer.IsGM,
                trainer.Items
            });
        }

        [HttpPut("{trainerId}/logout")]
        public ActionResult Logout(string trainerId)
        {
            Response.UpdateAccessControl();
            if (!Header.VerifyCookies(Request.Cookies, trainerId))
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
        public ActionResult<object> AddItemsToTrainer(
            string gameId,
            string trainerId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            var gameMasterId = Request.Query["gameMasterId"];
            if (!Header.VerifyCookies(Request.Cookies, gameMasterId))
            {
                return Unauthorized();
            }

            if (DatabaseUtility.FindTrainerById(gameMasterId)?.IsOnline != true)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} attempt to authorize a trade while not being a gm");
                return Unauthorized(gameMasterId);
            }

            var trainer = DatabaseUtility.FindTrainerById(trainerId);
            if (trainer == null)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve trainer {trainer.TrainerId}");
                return NotFound(trainerId);
            }
            if (!Request.Query.Any())
            {
                return BadRequest(new
                {
                    message = "No item listed",
                    gameId
                });
            }

            foreach (var itemName in Request.Query.Keys)
            {
                var (itemIncrease, badDataObject) = GetCleanData(gameId, itemName);
                if (badDataObject != null)
                {
                    BadRequest(badDataObject);
                    continue;
                }

                IEnumerable<ItemModel> itemList;
                var item = trainer.Items.FirstOrDefault(item => item.Name.Equals(itemName, StringComparison.CurrentCultureIgnoreCase));
                if (item == null)
                {
                    itemList = trainer.Items.Append(new ItemModel
                    {
                        Name = itemName,
                        Amount = itemIncrease
                    });
                }
                else
                {
                    itemList = trainer.Items.Select(item =>
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
                    });
                }

                var result = DatabaseUtility.UpdateTrainerItemList
                (
                    trainerId,
                    itemList
                );
                if (!result)
                {
                    StatusCode(500);
                }
            }

            Response.RefreshToken();
            trainer = DatabaseUtility.FindTrainerById(trainerId);
            return ReturnSuccessfully(new
            {
                trainer.TrainerId,
                trainer.TrainerName,
                trainer.IsGM,
                trainer.Items
            });
        }

        [HttpPut("{trainerId}/removeItems")]
        public ActionResult<object> RemoveItemsFromTrainer(
            string gameId,
            string trainerId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            if (!Header.VerifyCookies(Request.Cookies, trainerId))
            {
                return Unauthorized();
            }

            var trainer = DatabaseUtility.FindTrainerById(trainerId);
            if (trainer?.IsOnline == true)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve trainer {trainerId}");
                return NotFound(trainerId);
            }

            if (!Request.Query.Any())
            {
                return BadRequest(new
                {
                    message = "No item listed",
                    gameId
                });
            }

            foreach (var itemName in Request.Query.Keys)
            {
                var (itemIncrease, badDataObject) = GetCleanData(gameId, itemName);
                if (badDataObject != null)
                {
                    BadRequest(badDataObject);
                    continue;
                }

                IEnumerable<ItemModel> itemList;
                var item = trainer.Items.FirstOrDefault(item => item.Name.Equals(itemName, StringComparison.CurrentCultureIgnoreCase));
                if (item.Amount < itemIncrease)
                {
                    BadRequest(new
                    {
                        message = $"{trainerId} has too few {itemName}",
                        amount = item.Amount
                    });
                }

                itemList = trainer.Items
                    .Select(item =>
                    {
                        if (item.Name == itemName)
                        {
                            return new ItemModel
                            {
                                Name = itemName,
                                Amount = item.Amount - itemIncrease
                            };
                        }

                        return item;
                    })
                    .Where(item => item.Amount > 0);

                var result = DatabaseUtility.UpdateTrainerItemList
                (
                    trainerId,
                    itemList
                );
                if (!result)
                {
                    StatusCode(500);
                }
            }

            Response.RefreshToken();
            trainer = DatabaseUtility.FindTrainerById(trainerId);
            return ReturnSuccessfully(new
            {
                trainer.TrainerId,
                trainer.TrainerName,
                trainer.IsGM,
                trainer.Items
            });
        }

        [HttpDelete("{trainerId}")]
        public ActionResult<object> DeleteTrainer(string trainerId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            var gameMasterId = Request.Query["gameMasterId"];
            if (!Header.VerifyCookies(Request.Cookies, gameMasterId))
            {
                return Unauthorized();
            }

            if (!(GetDocument(gameMasterId, Collection, out var error) is TrainerModel gameMaster && gameMaster.IsGM))
            {
                return error;
            }

            if (!(DatabaseUtility.DeleteTrainer(trainerId) && DatabaseUtility.FindTrainerById(trainerId) == null))
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to delete trainer {trainerId}");
                return NotFound();
            }

            Response.RefreshToken();
            return ReturnSuccessfully(new
            {
                message = $"Successfully deleted all pokemon associated with {trainerId}"
            });
        }

        private (int UpdatedAmount, object Error) GetCleanData(
            string gameId,
            string itemName)
        {
            var change = Request.Query[itemName];
            if (!int.TryParse(change, out var itemChange))
            {
                return
                (
                    0,
                    new
                    {
                        message = "No change listed",
                        gameId,
                        item = itemName
                    }
                );
            }
            if (itemChange < 1)
            {
                return
                (
                    0,
                    new
                    {
                        message = "Should not change item count less than 1",
                        gameId,
                        item = itemName
                    }
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
