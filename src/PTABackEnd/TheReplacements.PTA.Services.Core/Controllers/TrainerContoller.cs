using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TheReplacements.PTA.Common.Utilities;
using TheReplacements.PTA.Common.Models;

namespace TheReplacements.PTA.Services.Core.Controllers
{
    [ApiController]
    [Route("api/v1/trainer")]
    public class TrainerContoller : ControllerBase
    {
        private readonly ILogger<TrainerContoller> _logger;

        public TrainerContoller(ILogger<TrainerContoller> logger)
        {
            _logger = logger;
        }

        [HttpGet("login")]
        public ActionResult<object> Login()
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            var username = Request.Query["username"];
            var gameId = Request.Query["gameId"];
            var trainer = DatabaseUtility.FindTrainerByUsername
            (
                username,
                gameId
            );
            if (trainer == null)
            {
                return NotFound(username);
            }

            if (!BCrypt.Net.BCrypt.Verify(Request.Query["password"], trainer.PasswordHash))
            {
                return Unauthorized(username);
            }

            return new
            {
                trainer.TrainerId,
                trainer.Username,
                trainer.IsGM,
                trainer.Items
            };
        }

        [HttpGet("{trainerId}/{pokemonId}")]
        public ActionResult<PokemonModel> FindTrainerMon(
            string trainerId,
            string pokemonId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            var pokemon = DatabaseUtility.FindPokemon(pokemon => pokemon.TrainerId == trainerId && pokemon.PokemonId == pokemonId);
            if (pokemon == null)
            {
                return NotFound(pokemonId);
            }

            return pokemon;
        }

        [HttpPost("{trainerId}")]
        public ActionResult<PokemonModel> AddPokemon(string trainerId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            var fails = new[] { "pokemon", "nature", "naturalMoves", "expYield", "catchRate", "experience", "level" }
                .Where(key => string.IsNullOrWhiteSpace(Request.Query[key]));
            if (fails.Any())
            {
                return BadRequest(new
                {
                    message = "Missing the following parameters in the query",
                    fails
                });
            }
            var parseFails = new[]
            {
                    GetBadRequestMessage("expYield", result => result > 0, out var expYield),
                    GetBadRequestMessage("catchRate", result => result >= 0, out var catchRate),
                    GetBadRequestMessage("experience", result => result >= 0, out var experience),
                    GetBadRequestMessage("level", result => result > 0, out var level),
                }.Where(fail => fail != null);
            if (parseFails.Any())
            {
                return BadRequest(parseFails);
            }

            var naturalMoves = Request.Query["naturalMoves"].ToString().Split(",");
            if (naturalMoves.Length < 1 || naturalMoves.Length > 4)
            {
                return BadRequest(naturalMoves);
            }
            var tmMoves = Request.Query["tmMoves"].ToString()?.Split(",") ?? new string[0];
            if (tmMoves.Length > 4)
            {
                return BadRequest(tmMoves);
            }

            var pokemonName = Request.Query["pokemon"];
            var natureName = Request.Query["nature"];
            var pokemon = PokeAPIUtility.GetPokemon
            (
                pokemonName,
                natureName
            );

            pokemon.TrainerId = trainerId;
            pokemon.NaturalMoves = naturalMoves;
            pokemon.TMMoves = tmMoves;
            pokemon.ExpYield = expYield;
            pokemon.CatchRate = catchRate;
            pokemon.Experience = experience;
            pokemon.Level = level;
            if (!string.IsNullOrWhiteSpace(Request.Query["nickname"]))
            {
                pokemon.Nickname = Request.Query["nickname"];
            }

            DatabaseUtility.AddPokemon(pokemon);

            return pokemon;
        }

        [HttpPut("{trainerId}/addItems")]
        public ActionResult<object> AddItemsToTrainer(
            string gameId,
            string trainerId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            var trainer = DatabaseUtility.FindTrainerById(trainerId);
            if (trainer == null)
            {
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

                var result = TrySendItemUpdate
                (
                    trainerId,
                    itemList
                );
                if (result == null)
                {
                    StatusCode(500);
                }
            }

            return DatabaseUtility.FindTrainerById(trainerId);
        }

        [HttpPut("{trainerId}/removeItems")]
        public ActionResult<object> RemoveItemsFromTrainer(
            string gameId,
            string trainerId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            var trainer = DatabaseUtility.FindTrainerById(trainerId);
            if (trainer == null)
            {
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

                var result = TrySendItemUpdate
                (
                    trainerId,
                    itemList
                );
                if (result == null)
                {
                    StatusCode(500);
                }
            }

            return DatabaseUtility.FindTrainerById(trainerId);
        }

        [HttpDelete("{trainerId}/pokemon")]
        public ActionResult<object> DeleteTrainerMons(string trainerId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            var deleteResponse = DatabaseUtility.DeleteTrainerMons(trainerId);
            if (deleteResponse != null)
            {
                return deleteResponse;
            }

            return StatusCode(500);
        }

        [HttpDelete("{trainerId}")]
        public ActionResult<object> DeleteTrainer(string trainerId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            var username = Request.Query["username"];
            var gameId = Request.Query["gameId"];
            Expression<Func<TrainerModel, bool>> filter = trainer => trainer.TrainerId == trainerId;
            var result = DatabaseUtility.DeleteTrainers(filter);
            if (result)
            {
                return DatabaseUtility.DeleteTrainerMons(trainerId);
            }
            else
            {
                return NotFound();
            }
        }

        private object GetBadRequestMessage(
            string parameter,
            Predicate<int> check,
            out int outVar)
        {
            var value = Request.Query[parameter];
            if (!(int.TryParse(value, out outVar) && check(outVar)))
            {
                return new
                {
                    message = $"Invalid {parameter}",
                    invalidValue = value
                };
            }

            return null;
        }

        private Tuple<int, object> GetCleanData(
            string gameId,
            string itemName)
        {
            var change = Request.Query[itemName];
            if (!int.TryParse(change, out var itemChange))
            {
                return new Tuple<int, object>
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
            if (itemChange == 0)
            {
                return new Tuple<int, object>
                (
                    0,
                    new
                    {
                        message = "Should not change item count by 0",
                        gameId,
                        item = itemName
                    }
                );
            }
            else if (itemChange > 100)
            {
                itemChange = 100;
            }

            return new Tuple<int, object>
            (
                itemChange,
                null
            );
        }

        private TrainerModel TrySendItemUpdate(
            string trainerId,
            IEnumerable<ItemModel> itemList)
        {
            Expression<Func<TrainerModel, bool>> filter = trainer => trainer.TrainerId == trainerId;
            var update = Builders<TrainerModel>.Update.Set
            (
                "Items",
                itemList
            );
            return DatabaseUtility.UpdateTrainer
            (
                filter,
                update
            );
        }
    }
}
