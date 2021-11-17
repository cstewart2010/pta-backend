using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using TheReplacements.PTA.Common.Utilities;
using TheReplacements.PTA.Common.Models;
using TheReplacements.PTA.Common.Enums;
using System.IO;

namespace TheReplacements.PTA.Services.Core.Controllers
{
    [ApiController]
    [Route("api/v1/game")]
    public class GameController : ControllerBase
    {
        private const MongoCollection Collection = MongoCollection.Game;

        private string ClientIp => Request.HttpContext.Connection.RemoteIpAddress.ToString();

        [HttpGet("{gameId}")]
        public ActionResult<GameModel> FindGame(string gameId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            var game = DatabaseUtility.FindGame(gameId);
            if (game == null)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve game {gameId}");
                return NotFound(gameId);
            }

            LoggerUtility.Info(Collection, $"Client {ClientIp} successfully hit {Request.Path.Value} {Request.Method} endpoint");
            return game;
        }

        [HttpGet("{gameId}/find/{trainerId}")]
        public ActionResult<object> FindTrainerInGame(string gameId, string trainerId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            if (DatabaseUtility.FindGame(gameId) == null)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve game {gameId}");
                return NotFound(gameId);
            }

            var trainer = DatabaseUtility.FindTrainerById(trainerId);
            if (trainer == null)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} Failed to retrieve trainer {trainerId}");
                return NotFound(trainerId);
            }

            if (trainer.GameId != gameId)
            {
                LoggerUtility.Error(Collection, $"{ClientIp}: Game {gameId} retrieved trainer {trainerId} who had game {trainer.GameId}");
                return BadRequest(new
                {
                    message = $"{trainerId} had an invalid game id",
                    trainer.GameId
                });
            }

            LoggerUtility.Info(Collection, $"Client {ClientIp} successfully hit {Request.Path.Value} {Request.Method} endpoint");
            return new
            {
                trainer.TrainerId,
                trainer.TrainerName,
                trainer.IsGM,
                trainer.Items
            };
        }

        [HttpPost("import")]
        public object ImportGame()
        {
            var jsonFile = Request.Form.Files.First(file => Path.GetExtension(file.FileName).ToLower() == ".json");
            string json;
            if (jsonFile.Length > 0)
            {
                using var reader = new StreamReader(jsonFile.OpenReadStream());
                json = reader.ReadToEnd();
                reader.Close();
            }
            else
            {
                return BadRequest(new
                {
                    message = "Empty json file"
                });
            }

            if (!ExportUtility.TryParseImport(json, out var errors))
            {
                return BadRequest(errors);
            }

            return Ok(errors);
        }

        [HttpPost("new")]
        public ActionResult<object> CreateNewGame()
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            var guid = Guid.NewGuid().ToString();
            var nickname = Request.Query["nickname"].ToString();
            var game = new GameModel
            {
                GameId = guid,
                Nickname = string.IsNullOrEmpty(nickname)
                    ? guid.Split('-')[0]
                    : nickname,
                IsOnline = true,
                PasswordHash = DatabaseUtility.HashPassword(Request.Query["gameSessionPassword"]),
                NPCs = Array.Empty<string>()
            };
            if (!DatabaseUtility.TryAddGame(game, out var error))
            {
                return BadRequest(error);
            }

            var username = Request.Query["gmUsername"].ToString();
            if (string.IsNullOrWhiteSpace(username))
            {
                return BadRequest(new
                {
                    message = "Missing gmName",
                    game.GameId
                });
            }

            var password = Request.Query["gmPassword"].ToString();
            if (string.IsNullOrWhiteSpace(username))
            {
                return BadRequest(new
                {
                    message = "Missing gmPassword",
                    game.GameId
                });
            }

            var trainer = CreateTrainer(game.GameId, username, password);
            trainer.IsGM = true;
            if (!DatabaseUtility.TryAddTrainer(trainer, out error))
            {
                return BadRequest(error);
            }

            Response.Cookies.Append("ptaSessionAuth", Header.GetCookie());
            LoggerUtility.Info(Collection, $"Client {ClientIp} successfully hit {Request.Path.Value} {Request.Method} endpoint");
            return new
            {
                game.GameId,
                GameMaster = new
                {
                    trainer.TrainerId,
                    trainer.TrainerName,
                    trainer.IsGM,
                    trainer.Items
                }
            };
        }

        [HttpPost("{gameId}/new")]
        public ActionResult<object> AddPlayerToGame(string gameId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            if (DatabaseUtility.FindGame(gameId) == null)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve game {gameId}");
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

            var trainerName = Request.Query["trainerName"].ToString();
            if (string.IsNullOrWhiteSpace(trainerName))
            {
                return BadRequest(new
                {
                    message = "Missing trainerName",
                    gameId
                });
            }

            var password = Request.Query["password"].ToString();
            if (string.IsNullOrWhiteSpace(trainerName))
            {
                return BadRequest(new
                {
                    message = "Missing password",
                    gameId
                });
            }

            var trainer = CreateTrainer(gameId, trainerName, password);
            if (DatabaseUtility.FindTrainerByUsername(trainerName, gameId) != null)
            {
                return BadRequest(new
                {
                    message = "Duplicate trainerName",
                    gameId,

                });
            }

            if (!DatabaseUtility.TryAddTrainer(trainer, out var error))
            {
                return BadRequest(error);
            }

            Response.Cookies.Append("ptaSessionAuth", Header.GetCookie());
            LoggerUtility.Info(Collection, $"Client {ClientIp} successfully hit {Request.Path.Value} {Request.Method} endpoint");
            return new
            {
                trainer.TrainerId,
                trainer.TrainerName,
                trainer.IsGM,
                trainer.Items
            };
        }

        [HttpPost("{gameMasterId}/wild")]
        public ActionResult<PokemonModel> AddPokemon(string gameMasterId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            if (!Header.VerifyCookies(Request.Cookies))
            {
                return Unauthorized();
            }

            var gameMaster = DatabaseUtility.FindTrainerById(gameMasterId);
            if (!(gameMaster?.IsGM == true && gameMaster.IsOnline))
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrievethe online game master with {gameMasterId}");
                return NotFound(gameMasterId);
            }

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
            var tmMoves = Request.Query["tmMoves"].ToString()?.Split(",") ?? Array.Empty<string>();
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

            pokemon.TrainerId = gameMasterId;
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

            if (!DatabaseUtility.TryAddPokemon(pokemon, out var error))
            {
                return BadRequest(error);
            }

            pokemon.AggregateStats();
            LoggerUtility.Info(Collection, $"Client {ClientIp} successfully hit {Request.Path.Value} {Request.Method} endpoint");
            return pokemon;
        }

        [HttpPut("{gameId}/start")]
        public ActionResult StartGame(string gameId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;

            var game = DatabaseUtility.FindGame(gameId);
            if (game == null)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve game {gameId}");
                return NotFound(gameId);
            }

            if (game.IsOnline)
            {
                return BadRequest(new
                {
                    message = "This game is already online",
                    gameId
                });
            }

            var gamePassword = Request.Query["gameSessionPassword"];
            if (!DatabaseUtility.VerifyTrainerPassword(gamePassword, game.PasswordHash))
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to log in to PTA");
                return Unauthorized(new
                {
                    message = "Could not login in to game with provided password",
                    gameId
                });
            }

            var username = Request.Query["gmUsername"].ToString();
            if (string.IsNullOrWhiteSpace(username))
            {
                return BadRequest(new
                {
                    message = "Missing gmName",
                    game.GameId
                });
            }

            var password = Request.Query["gmPassword"].ToString();
            if (string.IsNullOrWhiteSpace(username))
            {
                return BadRequest(new
                {
                    message = "Missing gmPassword",
                    game.GameId
                });
            }

            var gameMaster = DatabaseUtility.FindTrainerByUsername(username, gameId);
            if (gameMaster == null)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to log in to PTA");
                return Unauthorized(new
                {
                    message = "No username found with provided",
                    username
                });
            }

            if (!DatabaseUtility.VerifyTrainerPassword(password, game.PasswordHash))
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to log in to PTA");
                return Unauthorized(new
                {
                    message = "Invalid password",
                    password
                });
            }

            if (!gameMaster.IsGM)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to log in to PTA");
                return Unauthorized(new
                {
                    message = $"This user is not the GM for {game.Nickname}",
                    username,
                    gameId
                });
            }

            DatabaseUtility.UpdateTrainerOnlineStatus(gameMaster.TrainerId, true);
            DatabaseUtility.UpdateGameOnlineStatus(game.GameId, true);
            Response.Cookies.Append("ptaSessionAuth", Header.GetCookie());
            LoggerUtility.Info(Collection, $"Client {ClientIp} successfully hit {Request.Path.Value} {Request.Method} endpoint");
            return Ok();
        }

        [HttpPut("{gameId}/end")]
        public ActionResult EndGame(string gameId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            if (!Header.VerifyCookies(Request.Cookies))
            {
                return Unauthorized();
            }

            var game = DatabaseUtility.FindGame(gameId);
            if (game == null)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve game {gameId}");
                return NotFound(gameId);
            }

            var gameMasterId = Request.Query["gameMasterId"];
            var gameMaster = DatabaseUtility.FindTrainerById(gameMasterId);
            if (!(gameMaster?.IsGM == true && gameMaster.IsOnline == true))
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve online game master with {gameMasterId}");
                return NotFound(gameMasterId);
            }

            DatabaseUtility.UpdateGameOnlineStatus
            (
                game.GameId,
                false
            );

            foreach (var trainer in DatabaseUtility.FindTrainersByGameId(gameId))
            {
                DatabaseUtility.UpdateTrainerOnlineStatus
                (
                    trainer.TrainerId,
                    false
                );
            }

            LoggerUtility.Info(Collection, $"Client {ClientIp} successfully hit {Request.Path.Value} {Request.Method} endpoint");
            return Ok();
        }

        [HttpPut("{gameId}/addNpcs")]
        public ActionResult<object> AddNPCsToGame(string gameId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            if (!Header.VerifyCookies(Request.Cookies))
            {
                return Unauthorized();
            }

            var game = DatabaseUtility.FindGame(gameId);
            if (game == null)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve game {gameId}");
                return NotFound(gameId);
            }

            var gameMasterId = Request.Query["gameMasterId"];
            var gameMaster = DatabaseUtility.FindTrainerById(gameMasterId);
            if (gameMaster?.IsGM != true)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve game master with {gameMasterId}");
                return NotFound(gameMasterId);
            }

            var npcIds = Request.Query["npcIds"].ToString().Split(',');
            if (npcIds.Length < 1)
            {
                return BadRequest(new
                {
                    message = "No npc Ids provided"
                });
            }

            var npcs = DatabaseUtility.FindNpcs(npcIds);
            if (!npcs.Any())
            {
                LoggerUtility.Error(MongoCollection.Npc, $"Client {ClientIp} failed to retrieve npcs {Request.Query["npcIds"]}");
                return NotFound(npcIds);
            }

            var newNpcList = game.NPCs.ToList();
            newNpcList.AddRange(npcs.Select(npc => npc.NPCId));
            if (!DatabaseUtility.UpdateGameNpcList(gameId, newNpcList))
            {
                return StatusCode(500);
            }

            LoggerUtility.Info(Collection, $"Client {ClientIp} successfully hit {Request.Path.Value} {Request.Method} endpoint");
            return new
            {
                updatedNpcList = newNpcList
            };
        }

        [HttpPut("{gameId}/removeNpcs")]
        public ActionResult<object> RemovesNPCsFromGame(string gameId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            if (!Header.VerifyCookies(Request.Cookies))
            {
                return Unauthorized();
            }

            var game = DatabaseUtility.FindGame(gameId);
            if (game == null)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve game {gameId}");
                return NotFound(gameId);
            }

            var gameMasterId = Request.Query["gameMasterId"];
            var gameMaster = DatabaseUtility.FindTrainerById(gameMasterId);
            if (gameMaster?.IsGM != true)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve game master with {gameMasterId}");
                return NotFound(gameMasterId);
            }

            var npcIds = Request.Query["npcIds"].ToString().Split(',');
            if (npcIds.Length < 1)
            {
                return BadRequest(new
                {
                    message = "No npc Ids provided"
                });
            }

            var npcs = DatabaseUtility.FindNpcs(npcIds);
            if (!npcs.Any())
            {
                LoggerUtility.Error(MongoCollection.Npc, $"Client {ClientIp} failed to retrieve npcs {Request.Query["npcIds"]}");
                return NotFound(npcIds);
            }

            var newNpcList = game.NPCs.ToList().Except(npcs.Select(npc => npc.NPCId));
            if (!DatabaseUtility.UpdateGameNpcList(gameId, newNpcList))
            {
                return StatusCode(500);
            }

            LoggerUtility.Info(Collection, $"Client {ClientIp} successfully hit {Request.Path.Value} {Request.Method} endpoint");
            return new
            {
                updatedNpcList = newNpcList
            };
        }

        [HttpPut("reset")]
        public ActionResult<object> ChangeTrainerPassword()
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;

            // TODO: Pass a token to verify password reset
            var trainerId = Request.Query["trainerId"];
            var wasUpdateSucessful = DatabaseUtility.UpdateTrainerPassword
            (
                trainerId,
                Request.Query["password"]
            );

            if (!wasUpdateSucessful)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve trainer {trainerId}");
                return NotFound();
            }

            var trainer = DatabaseUtility.FindTrainerById(trainerId);
            LoggerUtility.Info(Collection, $"Client {ClientIp} successfully hit {Request.Path.Value} {Request.Method} endpoint");
            return new
            {
                trainer.TrainerId,
                trainer.TrainerName,
                trainer.IsGM,
                trainer.Items
            };
        }

        [HttpDelete("{gameId}")]
        public ActionResult<object> DeleteGame(string gameId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            var game = DatabaseUtility.FindGame(gameId);
            if (game == null)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve game {gameId}");
                return NotFound(gameId);
            }

            var gameMasterId = Request.Query["gameMasterId"];
            var gameMaster = DatabaseUtility.FindTrainerById(gameMasterId);
            if (gameMaster?.IsGM != true)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve game master with {gameMasterId}");
                return NotFound(gameMasterId);
            }

            var gamePassword = Request.Query["gameSessionPassword"];
            if (!DatabaseUtility.VerifyTrainerPassword(gamePassword, game.PasswordHash))
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to log in to PTA");
                return Unauthorized(new
                {
                    message = "Could not login in to game with provided password",
                    gameId
                });
            }

            var results = DatabaseUtility.FindTrainersByGameId(gameId)
                .Select
                (
                    trainer =>
                    {
                        if (DatabaseUtility.DeletePokemonByTrainerId(trainer.TrainerId) > -1)
                        {
                            return new
                            {
                                message = $"Successfully deleted all pokemon associated with {trainer.TrainerId}"
                            };
                        }
                        return null;
                    }
                )
                .Where(response => response != null)
                .ToList();

            var trainerDeletionResult = new
            {
                message = DeleteTrainers(gameId)
            };
            var gameDeletionResult = new
            {
                message = DatabaseUtility.DeleteGame(gameId)
                    ? $"Successfully deleted game {gameId}"
                    : $"Failed to delete {gameId}"
            };

            LoggerUtility.Info(Collection, $"Client {ClientIp} successfully hit {Request.Path.Value} {Request.Method} endpoint");
            return new
            {
                gameDeletionResult,
                trainerDeletionResult,
                pokemonDeletionResult = results
            };
        }

        [HttpDelete("{gameId}/export")]
        public ActionResult ExportGame(string gameId)
        {
            Response.Headers["Access-Control-Allow-Origin"] = Header.AccessUrl;
            var game = DatabaseUtility.FindGame(gameId);
            if (game == null)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve game {gameId}");
                return NotFound(gameId);
            }

            var gameMasterId = Request.Query["gameMasterId"];
            var gameMaster = DatabaseUtility.FindTrainerById(gameMasterId);
            if (gameMaster?.IsGM != true)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve game master with {gameMasterId}");
                return NotFound(gameMasterId);
            }

            var gamePassword = Request.Query["gameSessionPassword"];
            if (!DatabaseUtility.VerifyTrainerPassword(gamePassword, game.PasswordHash))
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to log in to PTA");
                return Unauthorized(new
                {
                    message = "Could not login in to game with provided password",
                    gameId
                });
            }

            DatabaseUtility.UpdateGameOnlineStatus(gameId, false);
            var exportStream = ExportUtility.GetExportStream(game);

            DeleteGame(gameId);
            LoggerUtility.Info(Collection, $"Client {ClientIp} successfully hit {Request.Path.Value} {Request.Method} endpoint");
            return File
            (
                exportStream,
                "application/octet-stream",
                $"{game.Nickname}.json"
            );
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

        private string DeleteTrainers(string gameId)
        {
            string message;
            if (DatabaseUtility.DeleteTrainersByGameId(gameId) > -1)
            {
                message = $"Successfully deleted all trainers associate with {gameId}";
                LoggerUtility.Info(MongoCollection.Trainer, message);
            }
            else
            {
                message = $"Failed to delete trainers";
                LoggerUtility.Error(MongoCollection.Trainer, message);
            }

            return message;
        }

        private TrainerModel CreateTrainer(
            string gameId,
            string username,
            string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            return new TrainerModel
            {
                GameId = gameId,
                TrainerId = Guid.NewGuid().ToString(),
                TrainerName = username,
                PasswordHash = DatabaseUtility.HashPassword(password),
                Items = new List<ItemModel>(),
                IsOnline = true,
                TrainerClasses = new List<string>(),
                TrainerStats = new TrainerStatsModel(),
                Level = 1,
                Feats = new List<string>()
            };
        }
    }
}
