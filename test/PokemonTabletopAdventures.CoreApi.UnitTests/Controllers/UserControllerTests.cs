using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PokemonTabletopAdventures.CoreApi.Controllers.v2;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.Models.Users;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Controllers;

public class UserControllerTests : BasePtaControllerTests
{
    private UserController _sut;

    [OneTimeSetUp]
    public void SetUp()
    {
        _sut = new UserController(
            UserService,
            TrainerService,
            PokemonService,
            GameService,
            DexService,
            PokedexService,
            UserMessageThreadService,
            EncryptionService,
            DtoToModelMapper,
            ModelToDtoMapper,
            Substitute.For<ILogger<UserController>>())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Test]
    public async Task GetUsername_Valid_ReturnsUsername()
    {
        // arrange
        var request = new RetrieveUserRequest
        {
            UserId = Shared.AdminIds.First(),
        };
        
        // act
        var response = await _sut.GetUsername(request);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<RetrieveUserResponse>());
        }
    }

    [Test]
    public async Task GetUsers_Valid_ReturnsUsers()
    {
        // arrange
        var request = new RetrieveUserRequest
        {
            UserId = Shared.AdminIds.First(),
            Offset = 0,
            Limit = 1
        };
        
        // act
        var response = await _sut.GetUsers(string.Empty, request);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<RetrieveUserResponse>());
        }
    }

    [Test]
    public void GetUsers_NotAdmin_ReturnsUnauthorized()
    {
        // arrange
        var request = new RetrieveUserRequest
        {
            UserId = Shared.UserIds.First(),
            Offset = 0,
            Limit = 1
        };
        
        // act
        Assert.ThrowsAsync<UserNotAdminException>(() => _sut.GetUsers(string.Empty, request));
    }

    [Test]
    public async Task ForceGetMessage_Valid_ReturnsThread()
    {
        // arrange
        var user = await UserService.GetUserById(Shared.UserIds.First());
        var request = new RetrieveThreadRequest
        {
            UserId = Shared.AdminIds.First(),
            MessageId = user.Messages.First()
        };
        
        // act
        var response = await _sut.ForceGetMessage(string.Empty, request);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<RetrieveThreadResponse>());
        }
    }

    [Test]
    public async Task ForceGetMessage_NotAdmin_ReturnsUnauthorized()
    {
        // arrange
        var user = await UserService.GetUserById(Shared.UserIds.Last());
        var request = new RetrieveThreadRequest
        {
            UserId = Shared.UserIds.First(),
            MessageId = user.Messages.First()
        };
        
        // act
        Assert.ThrowsAsync<UserNotAdminException>(() =>  _sut.ForceGetMessage(string.Empty, request));
    }

    [Test]
    public async Task GetMessage_Valid_ReturnsThread()
    {
        // arrange
        var user = await UserService.GetUserById(Shared.UserIds.First());
        var request = new RetrieveThreadRequest
        {
            UserId = Shared.UserIds.First(),
            MessageId = user.Messages.First()
        };
        
        // act
        var response = await _sut.GetMessage(string.Empty, request);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<RetrieveThreadResponse>());
        }
    }

    [Test]
    public async Task GetMessage_Admin_ReturnsThread()
    {
        // arrange
        var user = await UserService.GetUserById(Shared.UserIds.First());
        var request = new RetrieveThreadRequest
        {
            UserId = Shared.AdminIds.First(),
            MessageId = user.Messages.First()
        };
        
        // act
        var response = await _sut.GetMessage(string.Empty, request);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<RetrieveThreadResponse>());
        }
    }

    [Test]
    public async Task GetMessage_NotValid_ReturnsConflict()
    {
        // arrange
        var user = await UserService.GetUserById(Shared.UserIds.Last());
        var request = new RetrieveThreadRequest
        {
            UserId = Shared.UserIds.First(),
            MessageId = user.Messages.First()
        };
        
        // act
        Assert.ThrowsAsync<ConflictInDataException>(() => _sut.GetMessage(string.Empty, request));
    }

    [Test]
    public async Task CreateNewUser_Valid_AddsUserToCollection()
    {
        // arrange
        var request = new CreateuUserRequest
        {
            Username = "test-user",
            Password = "password",
        };
        
        // act
        var response = await _sut.CreateNewUser(request);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<CreateUserResponse>());
        }
    }

    [Test]
    public async Task SendMessageAsync_Valid_AddsThreadToCollection()
    {
        // arrange
        var request = new SendMessageRequest
        {
            MessageContent = "test-message",
            UserId = Shared.UserIds.First(),
            RecipientId = Shared.UserIds.Last()
        };
        
        // act
        var response = await _sut.SendMessageAsync(string.Empty, request);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<SendMessageResponse>());
        });
    }

    [Test]
    public async Task ReplyMessageAsync_Valid_ReturnsThread()
    {
        // arrange
        var user = await UserService.GetUserById(Shared.UserIds.First());
        var request = new SendMessageRequest
        {
            UserId = Shared.UserIds.First(),
            MessageId = user.Messages.First(),
            MessageContent = "test-message",
        };
        
        // act
        var response = await _sut.ReplyMessageAsync(string.Empty, request);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<SendMessageResponse>());
        }
    }

    [Test]
    public async Task ReplyMessageAsync_Admin_ReturnsThread()
    {
        // arrange
        var user = await UserService.GetUserById(Shared.UserIds.First());
        var request = new SendMessageRequest
        {
            UserId = Shared.AdminIds.First(),
            MessageId = user.Messages.First(),
            MessageContent = "test-message",
        };
        
        // act
        var response = await _sut.ReplyMessageAsync(string.Empty, request);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<SendMessageResponse>());
        }
    }

    [Test]
    public async Task ReplyMessageAsync_NotValid_ReturnsConflict()
    {
        // arrange
        var user = await UserService.GetUserById(Shared.UserIds.First());
        var request = new SendMessageRequest
        {
            UserId = Shared.UserIds.Last(),
            MessageId = user.Messages.First(),
            MessageContent = "test-message"
        };
        
        // act
        var response = await _sut.ReplyMessageAsync(string.Empty, request);
        
        // assert
        Assert.That(response, Is.InstanceOf<ConflictResult>());
    }

    [Test]
    public async Task Login_Valid_UpdatesUser()
    {
        // arrange
        var user = await UserService.GetUserById(Shared.UserIds.First());
        var request = new LoginRequest
        {
            Username = user.Username,
            Password = string.Empty,
        };
        
        // act
        var response = await _sut.Login(request);
        
        // assert
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<LoginResponse>());
        }
    }

    [Test]
    public async Task Logout_Valid_UpdatesUser()
    {
        // arrange
        var request = new LoginRequest
        {
            UserId = Shared.UserIds.First()
        };
        
        // act
        var response = await _sut.Logout(string.Empty, request);
        
        // assert
        Assert.That(response, Is.InstanceOf<OkResult>());
    }

    [Test]
    public async Task DeleteUser_Valid_DeletesUser()
    {
        // arrange
        var createRequest = new CreateuUserRequest
        {
            Username = "test-user",
            Password = "password",
        };
        
        var createResponse = await _sut.CreateNewUser(createRequest);
        var result = ((createResponse as OkObjectResult)!.Value as CreateUserResponse)!;
        var userId = result.User.UserId;
        var request = new DeleteUserRequest
        {
            UserId = userId
        };
        
        // act
        var response = await _sut.DeleteUser(string.Empty, request);
        
        // assert
        Assert.That(response, Is.InstanceOf<OkResult>());
    }

    [Test]
    public void DeleteUser_Admin_ReturnsBadRequest()
    {
        // arrange
        var request = new DeleteUserRequest
        {
            UserId = Shared.AdminIds.First()
        };
        
        // act
        Assert.ThrowsAsync<UserNotAdminException>(() => _sut.DeleteUser(string.Empty, request));
    }

    [Test]
    public async Task ForceDeleteUser_Valid_DeletesUser()
    {
        // arrange
        var createRequest = new CreateuUserRequest
        {
            Username = "test-user",
            Password = "password",
        };
        
        var createResponse = await _sut.CreateNewUser(createRequest);
        var result = ((createResponse as OkObjectResult)!.Value as CreateUserResponse)!;
        var userId = result.User.UserId;
        var request = new DeleteUserRequest
        {
            AdminId = Shared.AdminIds.First(),
            UserId = userId
        };
        
        // act
        var response = await _sut.ForceDeleteUser(string.Empty, request);
        
        // assert
        Assert.That(response, Is.InstanceOf<OkResult>());
    }

    [Test]
    public void ForceDeleteUser_NotAdmin_ReturnsUnauthorized()
    {
        // arrange
        var request = new DeleteUserRequest
        {
            UserId = Shared.UserIds.First(),
            AdminId = Shared.UserIds.Last()
        };
        
        // act
        Assert.ThrowsAsync<UserNotAdminException>(() => _sut.ForceDeleteUser(string.Empty, request));
    }

    [Test]
    public void ForceDeleteUser_SameAdmin_ReturnsBadRequest()
    {
        // arrange
        var request = new DeleteUserRequest
        {
            UserId = Shared.AdminIds.First(),
            AdminId = Shared.AdminIds.First()
        };
        
        // act
        Assert.ThrowsAsync<PtaUnauthorizedException>(() => _sut.ForceDeleteUser(string.Empty, request));
    }

    [Test]
    public void ForceDeleteUser_DifferentAdmin_ReturnsBadRequest()
    {
        // arrange
        var request = new DeleteUserRequest
        {
            UserId = Shared.AdminIds.First(),
            AdminId = Shared.AdminIds.Last()
        };
        
        // act
        Assert.ThrowsAsync<PtaUnauthorizedException>(() => _sut.ForceDeleteUser(string.Empty, request));
    }
}