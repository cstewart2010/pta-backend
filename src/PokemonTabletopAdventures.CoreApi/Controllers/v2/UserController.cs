using Microsoft.AspNetCore.Mvc;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Enums;
using PokemonTabletopAdventures.Models.Users;

namespace PokemonTabletopAdventures.CoreApi.Controllers.v2;

[ApiController]
[Route(Routes.UserRoute)]
public class UserController(
    IUserService userService,
    ITrainerService trainerService,
    IPokemonService pokemonService,
    IGameService gameService,
    IDexService dexUtility,
    IPokedexService pokedexService,
    IUserMessageThreadService userMessageThreadService,
    IEncryptionService encryptionService,
    IDtoToModelMapper dtoToModelMapper,
    IModelToDtoMapper modelToDtoMapper,
    ILogger<UserController> logger) : PtaControllerBase(userService, trainerService, pokemonService, gameService, dexUtility, pokedexService, encryptionService, dtoToModelMapper, modelToDtoMapper)
{
    private readonly ILogger<UserController> _logger = logger;
    private readonly IEncryptionService _encryptionService = encryptionService;

    [HttpPost("retrieve/user/name")]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetUsername([FromBody] RetrieveUserRequest request)
    {
        var user = await UserService.GetUserById(request.UserId);
        user.Games.Clear();
        user.Messages.Clear();
        return Ok(new RetrieveUserResponse { Users = [user] });
    }

    [HttpPost("retrieve/users")]
    [ProducesResponseType(typeof(IEnumerable<User>), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetUsers(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] RetrieveUserRequest request)
    {
        await VerifyIdentity(sessionAuth, request.UserId);
        if (!await IsUserAdmin(request.UserId))
        {
            return Unauthorized();
        }

        var users = await UserService.GetUsers(request.Offset, request.Limit);
        return Ok(new RetrieveUserResponse { Users = [.. users] });
    }

    [HttpPost("retrieve/message/admin")]
    [ProducesResponseType(typeof(RetrieveThreadResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> ForceGetMessage(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] RetrieveThreadRequest request)
    {
        await VerifyIdentity(sessionAuth, request.UserId);
        if (!await IsUserAdmin(request.UserId))
        {
            return Unauthorized();
        }

        var message = await userMessageThreadService.GetMessageById(request.MessageId);
        var response = new RetrieveThreadResponse
        {
            MessageThread = message
        };
        return Ok(response);
    }

    [HttpPost("retrieve/message")]
    [ProducesResponseType(typeof(RetrieveThreadResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetMessage(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] RetrieveThreadRequest request)
    {
        await VerifyIdentity(sessionAuth, request.UserId);
        var user = await UserService.GetUserById(request.UserId);
        if (!(user.Messages.Contains(request.MessageId) || await IsUserAdmin(request.UserId)))
        {
            return Conflict();
        }

        var message = await userMessageThreadService.GetMessageById(request.MessageId);
        var response = new RetrieveThreadResponse
        {
            MessageThread = message
        };
        return Ok(response);
    }

    [HttpPost("create")]
    [ProducesResponseType(typeof(CreateUserResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> CreateNewUser(
        [FromBody] CreateuUserRequest request)
    {
        var passHash = await _encryptionService.HashSecret(request.Password);
        var user = new User
        {
            UserId = Guid.NewGuid(),
            Username = request.Username,
            SiteRole = UserRoleOnSite.Active,
            DateCreated = DateTimeOffset.Now,
            Games = [],
            Messages = [],
            ActivityToken = ""
        };
        await UserService.PostUser(user, passHash);
        await AssignAuthAndToken(user.UserId);
        return Ok(new CreateUserResponse { User = user});
    }

    [HttpPost("create/message")]
    [ProducesResponseType(typeof(SendMessageResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> SendMessageAsync(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] SendMessageRequest request)
    {
        await VerifyIdentity(sessionAuth, request.UserId);
        var user = await UserService.GetUserById(request.UserId);
        var recipient = await UserService.GetUserById(request.RecipientId);
        var threadId = await AddNewThreadToUsers(user, recipient, request.MessageContent);
        var thread = await userMessageThreadService.GetMessageById(threadId);
        return Ok(new SendMessageResponse { MessageThread = thread });
    }

    [HttpPut("reply")]
    [ProducesResponseType(typeof(SendMessageResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> ReplyMessageAsync(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] SendMessageRequest request)
    {
        await VerifyIdentity(sessionAuth, request.UserId);
        var user = await UserService.GetUserById(request.UserId);
        if (!(user.Messages.Contains(request.MessageId) || await IsUserAdmin(request.UserId)))
        {
            return Conflict();
        }

        var thread = await userMessageThreadService.GetMessageById(request.MessageId);
        await AddNewReplyToThread(user, thread, request.MessageContent);
        var updatedThread = await userMessageThreadService.GetMessageById(request.MessageId);
        return Ok(new SendMessageResponse { MessageThread = updatedThread });
    }

    [HttpPatch("login")]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request)
    {
        await IsUserAuthenticated(request);
        var user = await UserService.GetUserByUsername(request.Username);
        await UserService.UpdateUserOnlineStatus(user.UserId, true);
        await AssignAuthAndToken(user.UserId);
        return Ok(new LoginResponse { User = user });
    }

    [HttpPut("logout")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> Logout(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] LoginRequest request)
    {
        await VerifyIdentity(sessionAuth, request.UserId);
        await UserService.UpdateUserOnlineStatus(request.UserId, false);
        return Ok();
    }

    [HttpDelete("delete/user")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> DeleteUser(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] DeleteUserRequest request)
    {
        await VerifyIdentity(sessionAuth, request.UserId);
        if (await IsUserAdmin(request.UserId))
        {
            return BadRequest();
        }

        await UserService.DeleteUser(request.UserId);
        return Ok();
    }

    [HttpDelete("delete/user/elevated")]
    [ProducesResponseType(typeof(void), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> ForceDeleteUser(
        [FromHeader(Name = HeaderNames.SessionAuth)] string sessionAuth,
        [FromBody] DeleteUserRequest request)
    {
        await VerifyIdentity(sessionAuth, request.AdminId);
        if (!await IsUserAdmin(request.AdminId))
        {
            return Unauthorized();
        }

        if (request.AdminId == request.UserId || await IsUserAdmin(request.UserId))
        {
            return BadRequest();
        }
        await UserService.DeleteUser(request.UserId);
        return Ok();
    }

    private async Task<Guid> AddNewThreadToUsers(User sender, User recipient, string messageContent)
    {
        var message = new UserMessage { Message = messageContent, User = sender.UserId, Timestamp = DateTimeOffset.Now};
        var thread = new UserMessageThread
        {
            MessageId = Guid.NewGuid(),
            Messages = [message]
        };
        await userMessageThreadService.PostThread(thread);
        sender.Messages.Add(thread.MessageId);
        recipient.Messages.Add(thread.MessageId);
        await UserService.UpdateUser(sender);
        await UserService.UpdateUser(recipient);
        return thread.MessageId;
    }

    private async Task AddNewReplyToThread(User sender, UserMessageThread thread, string messageContent)
    {
        var message = new UserMessage { Message = messageContent, User = sender.UserId, Timestamp = DateTimeOffset.Now };
        thread.Messages = [..thread.Messages.Append(message)];
        await userMessageThreadService.UpdateThread(thread);
    }

    private async Task<bool> IsUserAdmin(Guid userId)
    {
        var siteAdmin = await UserService.GetUserById(userId);
        return siteAdmin.SiteRole == UserRoleOnSite.SiteAdmin;
    }
}
