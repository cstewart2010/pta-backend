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

        [HttpGet]
        public ActionResult<object> Login()
        {
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

            if (trainer.PasswordHash != BCrypt.Net.BCrypt.HashPassword(Request.Query["password"]))
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

        [HttpPost("{gameId}")]
        public ActionResult<object> PostPlayer(string gameId)
        {
            if (DatabaseUtility.FindGame(gameId) == null)
            {
                return NotFound(gameId);
            }
            if (!DatabaseUtility.HasGM(gameId))
            {
                return BadRequest(new
                {
                    message = "No GM has been made",
                    gameId
                });
            }

            var username = Request.Query["username"];
            if (DatabaseUtility.FindTrainers(trainer => trainer.Username == username).Any())
            {
                return BadRequest(new
                {
                    message = "Duplicate username",
                    gameId,
                    username
                });
            }

            var trainer = CreateTrainer
            (
                gameId,
                username
            );
            if (trainer == null)
            {
                return BadRequest(new
                {
                    message = "Missing password",
                    gameId,
                    username
                });
            }

            DatabaseUtility.AddTrainer(trainer);
            return new
            {
                trainer.TrainerId,
                trainer.Username,
                trainer.IsGM,
                trainer.Items
            };
        }

        [HttpPost("{gameId}/gm")]
        public ActionResult<object> PostGM(string gameId)
        {
            if (DatabaseUtility.FindGame(gameId) == null)
            {
                return NotFound(gameId);
            }

            if (DatabaseUtility.HasGM(gameId))
            {
                return BadRequest(new
                {
                    message = "There is already a GM",
                    gameId
                });
            }

            var username = Request.Query["username"];
            var trainer = CreateTrainer
            (
                gameId,
                username
            );
            if (trainer == null)
            {
                return BadRequest(new
                {
                    message = "Missing password",
                    gameId,
                    username
                });
            }

            trainer.IsGM = true;
            DatabaseUtility.AddTrainer(trainer);
            return new
            {
                trainer.TrainerId,
                trainer.Username,
                trainer.IsGM,
                trainer.Items
            };
        }

        [HttpPut("{gameId}/{trainerId}")]
        public ActionResult<object> AddItems(string gameId, string trainerId)
        {
            var itemName = Request.Query["item"];
            var increase = Request.Query["increase"];
            if (string.IsNullOrWhiteSpace(itemName))
            {
                return BadRequest(new
                {
                    message = "No item listed",
                    gameId
                });
            }
            if (!int.TryParse(increase, out var itemIncrease))
            {
                return BadRequest(new
                {
                    message = "No increase listed",
                    gameId,
                    item = itemName
                });
            }
            if (itemIncrease == 0)
            {
                return BadRequest(new
                {
                    message = "Should not increase item count by 0",
                    gameId,
                    item = itemName
                });
            }
            else if (itemIncrease > 100)
            {
                itemIncrease = 100;
            }
            else if (itemIncrease < -100)
            {
                itemIncrease = -100;
            }
            var trainer = DatabaseUtility.FindTrainerById(trainerId);
            if (trainer == null)
            {
                return NotFound(trainerId);
            }

            IEnumerable<ItemModel> itemList;
            var item = trainer.Items.FirstOrDefault(item => item.Name.Equals(itemName, StringComparison.CurrentCultureIgnoreCase));
            if (itemIncrease > 0)
            {
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
            }
            else
            {
                if (item == null)
                {
                    return BadRequest(new
                    {
                        message = $"{trainerId} has no item named {itemName}"
                    });
                }

                if (item.Amount < -itemIncrease)
                {
                    return BadRequest(new
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
                                Amount = item.Amount + itemIncrease
                            };
                        }

                        return item;
                    })
                    .Where(item => item.Amount > 0);
            }

            Expression<Func<TrainerModel, bool>> filter = trainer => trainer.TrainerId == trainerId;
            var update = Builders<TrainerModel>.Update.Set("Items", itemList);
            var result = DatabaseUtility.TableHelper.Trainer.UpdateOne(filter, update);
            if (result.IsAcknowledged)
            {
                return DatabaseUtility.FindTrainer(filter);
            }

            return StatusCode(500);
        }

        [HttpPut("reset")]
        public ActionResult ChangePassword()
        {
            var username = Request.Query["username"];
            var gameId = Request.Query["gameId"];
            Expression<Func<TrainerModel, bool>> filter = trainer => trainer.Username == username && trainer.GameId == gameId;
            var update = Builders<TrainerModel>.Update.Set("PasswordHash", BCrypt.Net.BCrypt.HashPassword(Request.Query["password"]));
            var result = DatabaseUtility.TableHelper.Trainer.UpdateOne(filter, update);
            if (result.IsAcknowledged)
            {
                return Ok();
            }

            return NotFound();
        }

        [HttpDelete("{trainerId}")]
        public ActionResult<object> DeleteTrainer(string trainerId)
        {
            var username = Request.Query["username"];
            var gameId = Request.Query["gameId"];
            Expression<Func<TrainerModel, bool>> filter = trainer => trainer.TrainerId == trainerId;
            var result = DatabaseUtility.TableHelper.Trainer.DeleteOne(filter);
            if (result.IsAcknowledged)
            {
                return DatabaseUtility.DeleteTrainerMon(trainerId);
            }
            else
            {
                return NotFound();
            }
        }

        private TrainerModel CreateTrainer(string gameId, string username)
        {
            foreach (var key in new[] { "username", "password" })
            {
                if (string.IsNullOrWhiteSpace(Request.Query[key]))
                {
                    return null;
                }
            }

            return new TrainerModel
            {
                GameId = gameId,
                TrainerId = Guid.NewGuid().ToString(),
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Request.Query["password"]),
                Items = new List<ItemModel>()
            };
        }
    }
}
