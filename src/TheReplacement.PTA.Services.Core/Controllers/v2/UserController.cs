using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.DTOs;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models;
using PokemonTabletopAdventures.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.Controllers.v2;

[ApiController]
[Route("api/v2/user")]
public class UserController(
    IUserService userService,
    ITrainerService trainerService,
    IPokemonService pokemonService,
    IGameService gameService,
    IDexService dexUtility,
    IPokedexService pokedexService,
    IUserMessageThreadService userMessageThreadService,
    IEncryptionService encryptionService,
    ILogger<UserController> logger) : PtaControllerBase(userService, trainerService, pokemonService, gameService, dexUtility, pokedexService, encryptionService)
{
    private readonly ILogger<UserController> _logger = logger;
    private readonly IUserMessageThreadService _userMessageThreadService = userMessageThreadService;
    private readonly IEncryptionService _encryptionService = encryptionService;

    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetUsername(Guid userId)
    {
        var user = await UserService.GetUserById(userId);
        return Ok(user.Username);
    }

    [HttpGet("{adminId}/admin/allUsers")]
    [ProducesResponseType(typeof(IEnumerable<User>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetUsers(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid adminId,
        [FromQuery] int offset,
        [FromQuery] int limit)
    {
        await VerifyIdentity(accessToken, sessionAuth, adminId);
        if (!await IsUserAdmin(adminId))
        {
            return Unauthorized();
        }

        var users = await UserService.GetUsers(offset, limit);
        return Ok(users.Select(user => new User(user)));
    }

    [HttpGet("{adminId}/{messageId}/admin/message")]
    [ProducesResponseType(typeof(UserMessageThreadModel), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> ForceGetMessage(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid adminId,
        Guid messageId)
    {
        await VerifyIdentity(accessToken, sessionAuth, adminId);
        if (!await IsUserAdmin(adminId))
        {
            return Unauthorized();
        }

        var message = await _userMessageThreadService.GetMessageById(messageId);
        return Ok(message);
    }

    [HttpGet("{userId}/{messageId}")]
    [ProducesResponseType(typeof(UserMessageThreadModel), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetMessage(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid userId,
        Guid messageId)
    {
        await VerifyIdentity(accessToken, sessionAuth, userId);
        var user = await UserService.GetUserById(userId);
        if (!user.Messages.Contains(messageId))
        {
            return Conflict();
        }

        var message = await _userMessageThreadService.GetMessageById(messageId);
        return Ok(message);
    }

    [HttpPost]
    [ProducesResponseType(typeof(FoundUserResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> CreateNewUser(
        [FromQuery] PutLoginRequest request)
    {
        var passHash = await _encryptionService.HashSecret(request.Password);
        var user = new UserModel
        {
            UserId = Guid.NewGuid(),
            Username = request.Username,
            SiteRole = UserRoleOnSite.Active,
            PasswordHash = passHash,
        };
        await UserService.PostUser(user);
        await AssignAuthAndToken(user.UserId);
        return Ok(new FoundUserResponse(user));
    }

    [HttpPost("{userId}/{recipientId}/sendMessage")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> SendMessageAsync(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] PutMessageRequest request,
        Guid userId,
        Guid recipientId)
    {
        await VerifyIdentity(accessToken, sessionAuth, userId);
        var user = await UserService.GetUserById(userId);
        var recipient = await UserService.GetUserById(recipientId);
        await AddNewThreadToUsers(user, recipient, request.MessageContent);
        await RefreshToken(userId);
        return Ok();
    }

    [HttpPut("{userId}/{messageId}/replyMessage")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> ReplyMessageAsync(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] PutMessageRequest request,
        Guid userId,
        Guid messageId)
    {
        await VerifyIdentity(accessToken, sessionAuth, userId);
        var user = await UserService.GetUserById(userId);
        var thread = await _userMessageThreadService.GetMessageById(messageId);
        if (!user.Messages.Contains(messageId))
        {
            return Conflict();
        }

        await AddNewReplyToThread(user, thread, request.MessageContent);
        await RefreshToken(userId);
        return Ok();
    }

    [HttpPut("{gameId}/{userId}/refresh")]
    [ProducesResponseType(typeof(FoundGameResponse), 200)]
    [ProducesResponseType(typeof(FoundTrainerResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> RefreshInGame(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid userId,
        Guid gameId,
        [FromQuery] bool isGM)
    {
        if (isGM)
        {
            return await GetUpdatedGM(accessToken, sessionAuth, userId, gameId);
        }

        return await GetUpdatedTrainer(accessToken, sessionAuth, userId, gameId);
    }

    [HttpPut("login")]
    [ProducesResponseType(typeof(FoundUserResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> Login(
        [FromBody] PutLoginRequest request)
    {
        await IsUserAuthenticated(request);
        var user = await UserService.GetUserByUsername(request.Username);
        await AssignAuthAndToken(user.UserId);
        return Ok(new FoundUserResponse(user));
    }

    [HttpPut("{userId}/logout")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> Logout(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid userId)
    {
        await VerifyIdentity(accessToken, sessionAuth, userId);
        await UserService.UpdateUserOnlineStatus(userId, false);
        return Ok();
    }

    [HttpDelete("{userId}")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> DeleteUser(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid userId)
    {
        await VerifyIdentity(accessToken, sessionAuth, userId);
        if (await IsUserAdmin(userId))
        {
            return BadRequest();
        }

        await UserService.DeleteUser(userId);
        return Ok();
    }

    [HttpDelete("{adminId}/{userId}/admin")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> ForceDeleteUser(
        [FromHeader(Name = HeaderNames.AccessToken)] string accessToken,
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        Guid adminId,
        Guid userId)
    {
        await VerifyIdentity(accessToken, sessionAuth, adminId);
        if (!await IsUserAdmin(adminId))
        {
            return Unauthorized();
        }

        if (adminId == userId)
        {
            return BadRequest();
        }
        await UserService.DeleteUser(userId);
        await RefreshToken(adminId);
        return Ok();
    }

    private async Task<OkObjectResult> GetUpdatedTrainer(
        string accessToken,
        string sessionAuth,
        Guid userId,
        Guid gameId)
    {
        await VerifyIdentity(accessToken, sessionAuth, userId);
        await RefreshToken(userId);
        var user = await UserService.GetUserById(userId);
        var trainer = await TrainerService.GetTrainerById(userId, gameId);
        return Ok(await FoundTrainerResponse.ParseFromModel(trainer, user, PokemonService, PokedexService, GameService));
    }

    private async Task<OkObjectResult> GetUpdatedGM(
        string accessToken,
        string sessionAuth,
        Guid userId,
        Guid gameId)
    {
        await VerifyIdentity(accessToken, sessionAuth, userId);
        await RefreshToken(userId);
        var user = await UserService.GetUserById(userId);
        return Ok(await GameMasterResponse.ParseFromModel(user, gameId, TrainerService, PokemonService, PokedexService));
    }

    private async Task AddNewThreadToUsers(UserModel sender, UserModel recipient, string messageContent)
    {
        var message = new UserMessageModel(sender.UserId, messageContent);
        var thread = new UserMessageThreadModel
        {
            MessageId = Guid.NewGuid(),
            Messages = [message]
        };
        await _userMessageThreadService.PostThread(thread);
        sender.Messages = sender.Messages.Append(thread.MessageId);
        recipient.Messages = recipient.Messages.Append(thread.MessageId);
        await UserService.UpdateUser(sender);
        await UserService.UpdateUser(recipient);
    }

    private async Task AddNewReplyToThread(UserModel sender, UserMessageThreadModel thread, string messageContent)
    {
        var message = new UserMessageModel(sender.UserId, messageContent);
        thread.Messages = thread.Messages.Append(message);
        await _userMessageThreadService.UpdateThread(thread);
    }

    private async Task<bool> IsUserAdmin(Guid userId)
    {
        var siteAdmin = await UserService.GetUserById(userId);
        return siteAdmin.SiteRole == UserRoleOnSite.SiteAdmin;
    }
}
