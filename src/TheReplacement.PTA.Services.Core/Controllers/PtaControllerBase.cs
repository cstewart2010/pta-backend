using Microsoft.AspNetCore.Mvc;
using System;
using TheReplacement.PTA.Common.Enums;
using TheReplacement.PTA.Common.Interfaces;
using TheReplacement.PTA.Common.Models;
using TheReplacement.PTA.Common.Utilities;
using TheReplacement.PTA.Services.Core.Messages;

namespace TheReplacement.PTA.Services.Core.Controllers
{
    public abstract class PtaControllerBase : ControllerBase
    {
        protected abstract MongoCollection Collection { get; }

        protected string ClientIp => Request.HttpContext.Connection.RemoteIpAddress.ToString();

        protected bool IsGMOnline(string gameMasterId)
        {
            var gameMaster = DatabaseUtility.FindTrainerById(gameMasterId);
            return gameMaster?.IsGM == true && gameMaster.IsOnline;
        }

        protected IDocument GetDocument(string id, MongoCollection collection, out ActionResult notFound)
        {
            IDocument document = collection switch
            {
                MongoCollection.Games => DatabaseUtility.FindGame(id),
                MongoCollection.Npcs => DatabaseUtility.FindNpc(id),
                MongoCollection.Pokemon => DatabaseUtility.FindPokemonById(id),
                MongoCollection.Trainers => DatabaseUtility.FindTrainerById(id),
                _ => throw new ArgumentOutOfRangeException(nameof(collection)),
            };

            notFound = null;
            if (document == null)
            {
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
            if (!EncryptionUtility.VerifySecret(gamePassword, game.PasswordHash))
            {
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
                message = "No username found with provided";
                validationPassed = false;
            }
            else if (!EncryptionUtility.VerifySecret(password, trainer.PasswordHash))
            {
                message = "Invalid password";
                validationPassed = false;
            }
            else if (isGM && !trainer.IsGM)
            {
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

        internal static GenericMessage GetPokemonDeletion(string trainerId)
        {
            string message = DatabaseUtility.DeletePokemonByTrainerId(trainerId) > -1
                ? $"Successfully deleted all pokemon associated with {trainerId}"
                : $"No pokemon found for trainer {trainerId}";

            return new GenericMessage(message);
        }
    }
}
