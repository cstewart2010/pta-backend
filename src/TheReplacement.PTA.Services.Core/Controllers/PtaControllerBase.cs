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

        protected IDocument GetDocument(string id, MongoCollection collection, out ActionResult notFound)
        {
            IDocument document = collection switch
            {
                MongoCollection.Games => DatabaseUtility.FindGame(id),
                MongoCollection.Npcs => DatabaseUtility.FindNpc(id),
                MongoCollection.Pokemon => DatabaseUtility.FindPokemonById(id),
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

        protected bool IsUserAuthenticated(
            string username,
            string password,
            out ActionResult authError)
        {
            string message = string.Empty;
            var validationPassed = true;
            var user = DatabaseUtility.FindUserByUsername(username);
            if (user == null)
            {
                message = "No username found with provided";
                validationPassed = false;
            }
            else if (!EncryptionUtility.VerifySecret(password, user.PasswordHash))
            {
                message = "Invalid password";
                validationPassed = false;
            }

            authError = Unauthorized(new
            {
                message,
                username
            });

            if (validationPassed)
            {
                DatabaseUtility.UpdateUserOnlineStatus(user.UserId, true);
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
