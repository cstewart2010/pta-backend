using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using TheReplacement.PTA.Common.Enums;
using TheReplacement.PTA.Common.Models;
using TheReplacement.PTA.Common.Utilities;
using TheReplacement.PTA.Services.Core.Extensions;
using TheReplacement.PTA.Services.Core.Messages;
using TheReplacement.PTA.Services.Core.Objects;

namespace TheReplacement.PTA.Services.Core.Controllers
{
    [ApiController]
    [Route("api/v1/user")]
    public class UserController : PtaControllerBase
    {
        protected override MongoCollection Collection => throw new NotImplementedException();

        [HttpGet("{userId}")]
        public ActionResult<string> GetUsername(Guid userId)
        {
            var user = DatabaseUtility.FindUserById(userId);
            if (user == null)
            {
                return NotFound(userId);
            }

            return user.Username;
        }

        [HttpGet("{adminId}/admin/allUsers")]
        public ActionResult GetUsers(Guid adminId, [FromQuery] int offset, [FromQuery] int limit)
        {
            if (offset < 0 || limit <= 0)
            {
                return BadRequest();
            }

            if (!IsUserAdmin(adminId))
            {
                return Unauthorized();
            }

            var users = DatabaseUtility.FindUsers();
            var result = new
            {
                previous = GetPrevious(offset, limit),
                next = GetNext(offset, limit, users.Count()),
                users = users.GetSubset(offset, limit).Select(user => new PublicUser(user))
            };

            return Ok(result);
        }

        [HttpGet("{adminId}/{messageId}/admin/message")]
        public ActionResult<UserMessageThreadModel> ForceGetMessage(Guid adminId, Guid messageId)
        {
            if (!IsUserAdmin(adminId))
            {
                return Unauthorized();
            }

            return DatabaseUtility.FindMessageById(messageId);
        }

        [HttpGet("{userId}/{messageId}")]
        public ActionResult<UserMessageThreadModel> GetMessage(Guid userId, Guid messageId)
        {
            if (!Request.VerifyIdentity(userId))
            {
                return Unauthorized();
            }

            var user = DatabaseUtility.FindUserById(userId);
            if (!user.Messages.Contains(messageId))
            {
                return Conflict();
            }

            return DatabaseUtility.FindMessageById(messageId);
        }

        [HttpPost]
        public ActionResult<FoundUserMessage> CreateNewUser([FromQuery] string username, [FromQuery] string password)
        {
            if (username.Length < 6 || password.Length < 6)
            {
                return BadRequest();
            }

            if (DatabaseUtility.FindUserByUsername(username) != null)
            {
                return Conflict(username);
            }

            var user = new UserModel(username, password);
            if (DatabaseUtility.TryAddUser(user, out var error))
            {
                Response.AssignAuthAndToken(user.UserId);
                return new FoundUserMessage(user);
            }

            return BadRequest(error);
        }

        [HttpPost("{userId}/{recipientId}/sendMessage")]
        public async Task<ActionResult> SendMessageAsync(Guid userId, Guid recipientId)
        {
            if (!Request.VerifyIdentity(userId))
            {
                return Unauthorized();
            }

            var user = DatabaseUtility.FindUserById(userId);
            var recipient = DatabaseUtility.FindUserById(recipientId);

            if (recipient == null)
            {
                return NotFound(recipientId);
            }

            var body = await Request.GetRequestBody();
            var messageContent = body["messageContent"]?.ToString();
            if (string.IsNullOrEmpty(messageContent))
            {
                return BadRequest();
            }

            var error = AddNewThreadToUsers(user, recipient, messageContent);
            if (error != null)
            {
                return BadRequest(error);
            }

            Response.RefreshToken(userId);
            return Ok();
        }

        [HttpPut("{userId}/{messageId}/replyMessage")]
        public async Task<ActionResult> ReplyMessageAsync(Guid userId, Guid messageId)
        {
            if (!Request.VerifyIdentity(userId))
            {
                return Unauthorized();
            }

            var user = DatabaseUtility.FindUserById(userId);
            var thread = DatabaseUtility.FindMessageById(messageId);
            if (!user.Messages.Contains(messageId))
            {
                return Conflict();
            }

            var body = await Request.GetRequestBody();
            var messageContent = body["messageContent"]?.ToString();
            if (string.IsNullOrEmpty(messageContent))
            {
                return BadRequest();
            }

            AddNewReplyToThread(user, thread, messageContent);
            Response.RefreshToken(userId);
            return Ok();
        }

        [HttpPut("{gameId}/{userId}/refresh")]
        public ActionResult RefreshInGame(Guid userId, Guid gameId, [FromQuery] bool isGM)
        {
            if (isGM)
            {
                return GetUpdatedGM(userId, gameId);
            }

            return GetUpdatedTrainer(userId, gameId);
        }

        [HttpPut("login")]
        public async Task<ActionResult<FoundUserMessage>> Login()
        {
            var (username, password, credentialErrors) = await Request.GetUserCredentials();
            if (credentialErrors.Any())
            {
                return BadRequest(credentialErrors);
            }

            if (!IsUserAuthenticated(username, password, out var authError))
            {
                return authError;
            }

            var user = DatabaseUtility.FindUserByUsername(username);
            Response.AssignAuthAndToken(user.UserId);
            return new FoundUserMessage(user);
        }

        [HttpPut("{userId}/logout")]
        public ActionResult Logout(Guid userId)
        {
            if (!Request.VerifyIdentity(userId))
            {
                return Unauthorized();
            }

            DatabaseUtility.UpdateUserOnlineStatus(userId, false);
            return Ok();
        }

        [HttpDelete("{userId}")]
        public ActionResult DeleteUser(Guid userId)
        {
            if (!Request.VerifyIdentity(userId))
            {
                return Unauthorized();
            }

            if (IsUserAdmin(userId))
            {
                return BadRequest();
            }

            if (DatabaseUtility.DeleteUser(userId))
            {
                return Ok();
            }

            return BadRequest();
        }

        [HttpDelete("{adminId}/{userId}/admin")]
        public ActionResult ForceDeleteUser(Guid adminId, Guid userId)
        {
            if (!IsUserAdmin(adminId))
            {
                return Unauthorized();
            }

            if (adminId == userId)
            {
                return BadRequest();
            }

            if (DatabaseUtility.DeleteUser(userId))
            {
                return Ok();
            }

            Response.RefreshToken(adminId);
            return BadRequest();
        }

        private ActionResult GetUpdatedTrainer(Guid userId, Guid gameId)
        {
            if (!Request.VerifyIdentity(userId))
            {
                return Unauthorized();
            }

            Response.RefreshToken(userId);
            return Ok(new FoundTrainerMessage(userId, gameId));
        }

        private ActionResult GetUpdatedGM(Guid userId, Guid gameId)
        {
            if (!Request.IsUserGM(userId, gameId))
            {
                return Unauthorized();
            }

            Response.RefreshToken(userId);
            return Ok(new GameMasterMessage(userId, gameId));
        }

        private static MongoWriteError AddNewThreadToUsers(UserModel sender, UserModel recipient, string messageContent)
        {
            var message = new UserMessageModel(sender.UserId, messageContent);
            var thread = new UserMessageThreadModel
            {
                MessageId = Guid.NewGuid(),
                Messages = new[] { message }
            };

            if (DatabaseUtility.TryAddThread(thread, out var error))
            {
                sender.Messages = sender.Messages.Append(thread.MessageId);
                recipient.Messages = recipient.Messages.Append(thread.MessageId);
                DatabaseUtility.UpdateUser(sender);
                DatabaseUtility.UpdateUser(recipient);
            }

            return error;
        }

        private static void AddNewReplyToThread(UserModel sender, UserMessageThreadModel thread, string messageContent)
        {
            var message = new UserMessageModel(sender.UserId, messageContent);
            thread.Messages = thread.Messages.Append(message);
            DatabaseUtility.UpdateThread(thread);
        }

        public static bool IsUserAdmin(Guid userId)
        {
            var siteAdmin = DatabaseUtility.FindUserById(userId);
            return Enum.TryParse<UserRoleOnSite>(siteAdmin.SiteRole, out var role) && role == UserRoleOnSite.SiteAdmin;
        }

        private static object GetPrevious(int offset, int limit)
        {
            int previousOffset = Math.Max(0, offset - limit);
            int previousLimit = offset - limit < 0 ? offset : limit;
            if (previousOffset == offset)
            {
                return new
                {
                    offset,
                    limit
                };
            }

            return new
            {
                offset = previousOffset,
                limit = previousLimit
            };
        }

        private static object GetNext(int offset, int limit, int count)
        {
            int nextOffset = Math.Min(offset + limit, count - limit);
            if (nextOffset <= offset)
            {
                return new
                {
                    offset,
                    limit
                };
            }


            return new
            {
                offset = nextOffset,
                limit
            };
        }
    }
}
