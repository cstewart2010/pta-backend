using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheReplacements.PTA.Common.Enums;
using TheReplacements.PTA.Common.Interfaces;
using TheReplacements.PTA.Common.Models;
using TheReplacements.PTA.Common.Utilities;

namespace TheReplacements.PTA.Services.Core.Controllers
{
    public abstract class PtaControllerBase : ControllerBase
    {
        protected abstract MongoCollection Collection { get; }

        protected string ClientIp => Request.HttpContext.Connection.RemoteIpAddress.ToString();

        protected bool IsGMOnline(string gameMasterId)
        {
            var gameMaster = DatabaseUtility.FindTrainerById(gameMasterId);
            if (!(gameMaster?.IsGM == true && gameMaster.IsOnline))
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve the online game master with {gameMasterId}");
                return false;
            }

            return true;
        }

        protected IDocument GetDocument(string id, MongoCollection collection, out ActionResult notFound)
        {
            IDocument document = Collection switch
            {
                MongoCollection.Game => DatabaseUtility.FindGame(id),
                MongoCollection.Npc => DatabaseUtility.FindNpc(id),
                MongoCollection.Pokemon => DatabaseUtility.FindPokemonById(id),
                MongoCollection.Trainer => DatabaseUtility.FindTrainerById(id),
                _ => throw new ArgumentOutOfRangeException(),
            };

            notFound = null;
            if (document == null)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to retrieve {collection} {id}");
                notFound = NotFound(id);
            }

            return document;
        }

        protected bool IsGameAuthenticated(
            string gamePassword,
            GameModel game,
            out ActionResult authError)
        {
            var message = string.Empty;
            var validationPassed = true;
            if (game.IsOnline)
            {
                message = "This game is already online";
                validationPassed = false;
            }
            else if (!EncryptionUtility.VerifySecret(gamePassword, game.PasswordHash))
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to log in to PTA");
                message = "Could not login in to game with provided password";
                validationPassed = false;
            }

            authError = Unauthorized(new
            {
                message,
                game.GameId
            });

            return validationPassed;
        }

        protected bool IsTrainerAuthenticated(
            string username,
            string password,
            string gameId,
            bool isGM,
            out ActionResult authError)
        {
            string message = string.Empty;
            var validationPassed = true;
            var trainer = DatabaseUtility.FindTrainerByUsername(username, gameId);
            if (trainer == null)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to log in to PTA");
                message = "No username found with provided";
                validationPassed = false;
            }
            else if (!EncryptionUtility.VerifySecret(password, trainer.PasswordHash))
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to log in to PTA");
                message = "Invalid password";
                validationPassed = false;
            }
            else if (isGM && trainer.IsGM)
            {
                LoggerUtility.Error(Collection, $"Client {ClientIp} failed to log in to PTA");
                message = $"This user is not the GM for {gameId}";
                validationPassed = false;
            }

            authError = Unauthorized(new
            {
                message,
                username,
                gameId
            });

            if (validationPassed)
            {
                DatabaseUtility.UpdateTrainerOnlineStatus(trainer.TrainerId, true);
                if (isGM)
                {
                    DatabaseUtility.UpdateGameOnlineStatus(gameId, true);
                }
            }

            return validationPassed;
        }

        internal static object GetPokemonDeletion(string trainerId)
        {
            if (DatabaseUtility.DeletePokemonByTrainerId(trainerId) > -1)
            {
                return new
                {
                    message = $"Successfully deleted all pokemon associated with {trainerId}"
                };
            }
            return null;
        }

        protected T ReturnSuccessfully<T>(T reward)
        {
            LoggerUtility.Info(Collection, $"Client {ClientIp} successfully hit {Request.Path.Value} {Request.Method} endpoint");
            return reward;
        }
    }
}
