﻿using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using TheReplacement.PTA.Common.Utilities;
using TheReplacement.PTA.Common.Models;
using TheReplacement.PTA.Common.Enums;
using TheReplacement.PTA.Services.Core.Extensions;
using TheReplacement.PTA.Services.Core.Messages;
using System.Collections;
using System;
using Microsoft.AspNetCore.Cors;

namespace TheReplacement.PTA.Services.Core.Controllers
{
    [ApiController]
    [Route("api/v1/game")]
    public class GameController : PtaControllerBase
    {
        protected override MongoCollection Collection { get; }

        public GameController()
        {
            Collection = MongoCollection.Games;
        }

        [HttpGet]
        public IEnumerable FindAllGames()
        {
            if (Request.Method == "OPTIONS")
            {
                return null;
            }
            if (Request.Query.TryGetValue("nickname", out var nickname))
            {
                return DatabaseUtility.FindAllGames(nickname)
                    .Select(game => new FoundGameMessage(game.GameId));
            }

            return DatabaseUtility.FindAllGames();
        }

        [HttpGet("{gameId}")]
        public ActionResult<FoundGameMessage> FindGame(string gameId)
        {
            if (Request.Method == "OPTIONS")
            {
                return Ok();
            }
            var document = GetDocument(gameId, Collection, out var notFound);
            if (!(document is GameModel))
            {
                return notFound;
            }

            return ReturnSuccessfully(new FoundGameMessage(gameId));
        }

        [HttpGet("{gameId}/find/{trainerId}")]
        public ActionResult<FoundTrainerMessage> FindTrainerInGame(string gameId, string trainerId)
        {
            if (Request.Method == "OPTIONS")
            {
                return Ok();
            }
            var gameDocument = GetDocument(gameId, Collection, out var notFound);
            if (!(gameDocument is GameModel))
            {
                return notFound;
            }

            var trainerDocument = GetDocument(trainerId, MongoCollection.Trainers, out notFound);
            if (!(trainerDocument is TrainerModel trainer))
            {
                return notFound;
            }

            if (trainer.GameId != gameId)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} Game {gameId} retrieved trainer {trainerId} who had game {trainer.GameId}");
                return BadRequest(new InvalidGameIdMessage(trainer));
            }

            return ReturnSuccessfully(new FoundTrainerMessage(trainer));
        }

        [HttpPost("import")]
        public ActionResult<object> ImportGame()
        {
            if (Request.Method == "OPTIONS")
            {
                return Ok();
            }
            var json = Request.GetJsonFromRequest();
            if (string.IsNullOrEmpty(json))
            {
                return BadRequest(new GenericMessage("empty json file"));
            }

            if (!ExportUtility.TryParseImport(json, out var errors))
            {
                return BadRequest(errors);
            }

            return ReturnSuccessfully(Ok(errors));
        }

        [HttpPost("new")]
        public ActionResult<CreatedGameMessage> CreateNewGame()
        {
            if (Request.Method == "OPTIONS")
            {
                return Ok();
            }
            var game = Request.BuildGame();
            if (!DatabaseUtility.TryAddGame(game, out var error))
            {
                return BadRequest(error);
            }

            var trainer = Request.BuildGM(game.GameId, out var badRequest);
            if (trainer == null)
            {
                return BadRequest(badRequest);
            }

            if (!DatabaseUtility.TryAddTrainer(trainer, out error))
            {
                return BadRequest(error);
            }

            Response.AssignAuthAndToken(trainer.TrainerId);
            return ReturnSuccessfully(new CreatedGameMessage(trainer));
        }

        [HttpPost("{gameId}/new")]
        public ActionResult<FoundTrainerMessage> AddPlayerToGame(string gameId)
        {
            if (Request.Method == "OPTIONS")
            {
                return Ok();
            }
            var gameDocument = GetDocument(gameId, Collection, out var notFound);
            if (!(gameDocument is GameModel))
            {
                return notFound;
            }

            if (!DatabaseUtility.HasGM(gameId, out var noGMError))
            {
                return BadRequest(noGMError);
            }

            var trainer = Request.BuildTrainer(gameId, out var badRequest);
            if (trainer == null)
            {
                return BadRequest(badRequest);
            }

            if (!DatabaseUtility.TryAddTrainer(trainer, out var error))
            {
                return BadRequest(error);
            }

            Response.AssignAuthAndToken(trainer.TrainerId);
            return ReturnSuccessfully(new FoundTrainerMessage(trainer));
        }

        [HttpPost("{gameMasterId}/wild")]
        public ActionResult<PokemonModel> AddPokemon(string gameMasterId)
        {
            if (Request.Method == "OPTIONS")
            {
                return Ok();
            }
            if (!Request.VerifyIdentity(gameMasterId))
            {
                return Unauthorized();
            }

            if (!IsGMOnline(gameMasterId))
            {
                return NotFound(gameMasterId);
            }

            var pokemon = Request.BuildPokemon(gameMasterId, out var error);
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

        [HttpPut("{gameId}/start")]
        public ActionResult<FoundGameMessage> StartGame(string gameId)
        {
            if (Request.Method == "OPTIONS")
            {
                return Ok();
            }
            var gameDocument = GetDocument(gameId, Collection, out var notFound);
            if (!(gameDocument is GameModel game))
            {
                return notFound;
            }

            var (gamePassword, username, password) = Request.GetStartGameCredentials(out var credentialErrors);
            if (credentialErrors.Any())
            {
                return BadRequest(credentialErrors);
            }

            if (!IsGameAuthenticated(gamePassword, game, out var authError))
            {
                return authError;
            }

            if (!IsTrainerAuthenticated(username, password, gameId, true, out authError))
            {
                return authError;
            }

            var trainer = DatabaseUtility.FindTrainerByUsername(username, gameId);
            Response.AssignAuthAndToken(trainer.TrainerId);
            return ReturnSuccessfully(new FoundGameMessage(gameId));
        }

        [HttpPut("{gameId}/end")]
        public ActionResult EndGame(string gameId)
        {
            if (Request.Method == "OPTIONS")
            {
                return Ok();
            }
            var gameMasterId = Request.Query["gameMasterId"];
            if (!Request.VerifyIdentity(gameMasterId))
            {
                return Unauthorized();
            }

            var gameDocument = GetDocument(gameId, Collection, out var notFound);
            if (!(gameDocument is GameModel))
            {
                return notFound;
            }

            if (!IsGMOnline(gameMasterId))
            {
                return NotFound(gameMasterId);
            }

            SetEndGameStatuses(gameId);
            return ReturnSuccessfully(Ok());
        }

        [HttpPut("{gameId}/addNpcs")]
        public ActionResult<UpdatedNpcListMessage> AddNPCsToGame(string gameId)
        {
            if (Request.Method == "OPTIONS")
            {
                return Ok();
            }
            var gameMasterId = Request.Query["gameMasterId"];
            var npcIds = GetNpcs(gameMasterId, out var notFound);
            if (npcIds == null)
            {
                return notFound;
            }

            var gameDocument = GetDocument(gameId, Collection, out notFound);
            if (!(gameDocument is GameModel game))
            {
                return notFound;
            }

            var newNpcList = game.NPCs.Union(npcIds);
            Response.RefreshToken(gameMasterId);
            return UpdateNpcList
            (
                gameId,
                newNpcList
            );
        }

        [HttpPut("{gameId}/removeNpcs")]
        public ActionResult<UpdatedNpcListMessage> RemovesNPCsFromGame(string gameId)
        {
            if (Request.Method == "OPTIONS")
            {
                return Ok();
            }
            var gameMasterId = Request.Query["gameMasterId"];
            var npcIds = GetNpcs(gameMasterId, out var notFound);
            if (npcIds == null)
            {
                return notFound;
            }

            var gameDocument = GetDocument(gameId, Collection, out notFound);
            if (!(gameDocument is GameModel game))
            {
                return notFound;
            }

            var newNpcList = game.NPCs.Except(npcIds);
            Response.RefreshToken(gameMasterId);
            return UpdateNpcList
            (
                gameId,
                newNpcList
            );
        }

        [HttpPut("reset")]
        public ActionResult<object> ChangeTrainerPassword()
        {
            if (Request.Method == "OPTIONS")
            {
                return Ok();
            }
            var returnNotFound = true;
            if (returnNotFound)
            {
                return NotFound();
            }

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
            Response.AssignAuthAndToken(trainer.TrainerId);
            return ReturnSuccessfully(new FoundTrainerMessage(trainer));
        }

        [HttpDelete("{gameId}")]
        public ActionResult<object> DeleteGame(string gameId)
        {
            if (Request.Method == "OPTIONS")
            {
                return Ok();
            }
            var gameDocument = GetDocument(gameId, Collection, out var notFound);
            if (!(gameDocument is GameModel game))
            {
                return notFound;
            }

            var gameMasterId = Request.Query["gameMasterId"];
            var trainerDocument = GetDocument(gameMasterId, MongoCollection.Trainers, out notFound);
            if (!(trainerDocument is TrainerModel trainer && trainer.IsGM))
            {
                return notFound;
            }

            var gamePassword = Request.Query["gameSessionPassword"];
            if (!IsGameAuthenticated(gamePassword, game, out var authError))
            {
                return authError;
            }

            return ReturnSuccessfully(new
            {
                pokemonDeletionResult = GetMassPokemonDeletion(gameId),
                trainerDeletionResult = GetMassTrainerDeletion(gameId),
                gameDeletionResult = GetGameDeletion(gameId)
            });;
        }

        [HttpDelete("{gameId}/export")]
        public ActionResult ExportGame(string gameId)
        {
            if (Request.Method == "OPTIONS")
            {
                return Ok();
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

            var gamePassword = Request.Query["gameSessionPassword"];
            if (!EncryptionUtility.VerifySecret(gamePassword, game.PasswordHash))
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to log in to PTA");
                return Unauthorized(new UnauthorizedMessage(gameId));
            }

            DatabaseUtility.UpdateGameOnlineStatus(gameId, false);
            var exportStream = ExportUtility.GetExportStream(game);

            DeleteGame(gameId);
            return ReturnSuccessfully(File
            (
                exportStream,
                "application/octet-stream",
                $"{game.Nickname}.json"
            ));
        }

        private IEnumerable<string> GetNpcs(
            string gameMasterId,
            out ActionResult notFound)
        {
            if (!Request.VerifyIdentity(gameMasterId))
            {
                notFound = Unauthorized();
                return null;
            }

            var trainerDocument = GetDocument(gameMasterId, MongoCollection.Trainers, out notFound);
            if (!(trainerDocument is TrainerModel trainer && trainer.IsGM))
            {
                return null;
            }

            var npcIds = Request.GetNpcIds(out var error);
            if (!npcIds.Any())
            {
                notFound = NotFound(error);
                return null;
            }

            return npcIds;
        }

        private ActionResult<UpdatedNpcListMessage> UpdateNpcList(
            string gameId,
            IEnumerable<string> newNpcList)
        {
            if (!DatabaseUtility.UpdateGameNpcList(gameId, newNpcList))
            {
                return StatusCode(500);
            }

            return ReturnSuccessfully(new UpdatedNpcListMessage(newNpcList));
        }

        private static void SetEndGameStatuses(string gameId)
        {
            DatabaseUtility.UpdateGameOnlineStatus
            (
                gameId,
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
        }

        private static GenericMessage GetGameDeletion(string gameId)
        {
            string message = DatabaseUtility.DeleteGame(gameId)
                ? $"Successfully deleted game {gameId}"
                : $"Failed to delete {gameId}";
            return new GenericMessage(message);
        }

        private static GenericMessage GetMassTrainerDeletion(string gameId)
        {
            string message;
            if (DatabaseUtility.DeleteTrainersByGameId(gameId) > -1)
            {
                message = $"Successfully deleted all trainers associate with {gameId}";
                LoggerUtility.Info(MongoCollection.Trainers, message);
            }
            else
            {
                message = $"Failed to delete trainers";
                LoggerUtility.Error(MongoCollection.Trainers, message);
            }

            return new GenericMessage(message);
        }

        private static IEnumerable<GenericMessage> GetMassPokemonDeletion(string gameId)
        {
            return DatabaseUtility.FindTrainersByGameId(gameId)
                .Select(trainer => GetPokemonDeletion(trainer.TrainerId))
                .Where(response => response != null);
        }
    }
}
