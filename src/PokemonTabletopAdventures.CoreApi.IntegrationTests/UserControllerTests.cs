using Microsoft.AspNetCore.Mvc;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.IntegrationTests;

[TestFixture]
internal class UserControllerTests
{

    private static readonly HttpClient HttpClient = new HttpClient();

    [OneTimeSetUp]
    public static void OneTimeSetup()
    {
        HttpClient.BaseAddress = new Uri(Constants.ApiRootUrl);
    }

    [Test]
    public async Task RetrieveNameTest()
    {
        RetrieveUserRequest retrieveUserRequestPayload = new RetrieveUserRequest();
        retrieveUserRequestPayload.UserId = Guid.Parse("4991a050-783c-4a83-9c5e-9ff9612d9191");
        var message = new HttpRequestMessageBuilder(
            HttpMethod.Post,
            HttpClient.BaseAddress!,
            "api/v2/user/retrieve/name"
            ).WithPayload("application/json", retrieveUserRequestPayload).Build();

        var responseMessage = await HttpClient.SendAsync(message);
        var content = await responseMessage.Content.ReadAsStringAsync();
        var response = new Response<RetrieveUserResponse>(responseMessage, content);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccessful, Is.True);
            Assert.That(response.Data, Is.Not.Null);
        });

        Assert.That(response.Data.Users, Is.Not.Null);
        var user = response.Data.Users.SingleOrDefault();

        Assert.Multiple(() =>
        {
            Assert.That(user.Username.ToLower, Is.EqualTo("victor"));
        });
    }

    [Test]
    public async Task RetrieveNameValidationTest() 
    {
        RetrieveUserRequest retrieveUserRequestPayload = new RetrieveUserRequest();
        retrieveUserRequestPayload.UserId = Guid.Parse("00000000-0000-0000-0000-000000000000");
        
        var message = new HttpRequestMessageBuilder(
            HttpMethod.Post,
            HttpClient.BaseAddress!,
            "api/v2/user/retrieve/name"
            ).WithPayload("application/json", retrieveUserRequestPayload).Build();

        var responseMessage = await HttpClient.SendAsync(message);
        var content = await responseMessage.Content.ReadAsStringAsync();
        var response = new Response<ProblemDetails>(responseMessage, content);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccessful, Is.False);
            Assert.That(response.Data, Is.Null);
            Assert.That(response.ProblemDetails, Is.Not.Null);
        });

        Assert.Multiple(() =>
        {
            Assert.That(response.ProblemDetails.Title, Is.EqualTo("Unknown Entity"));
            Assert.That(response.ProblemDetails.Detail, Is.EqualTo(
                "Could not find a UserDto using UserId=00000000-0000-0000-0000-000000000000"));
            Assert.That(response.ProblemDetails.Status, Is.EqualTo(404));
        });
    }

    [Test]
    public async Task retrieveUsersTest() 
    {
        RetrieveUserRequest retrieveUserRequestPayload = new RetrieveUserRequest();
        retrieveUserRequestPayload.UserId = Guid.Parse("4b95e8ef-dd67-4aed-a62e-4fc7bf0a2b2f");
        retrieveUserRequestPayload.Limit = 100;
        var message = new HttpRequestMessageBuilder(
            HttpMethod.Post,HttpClient.BaseAddress!,"api/v2/user/retrieve/users")
            .WithPayload("application/json", retrieveUserRequestPayload)
            .WithHeader("pta-session-auth", "123")
            .WithHeader("pta-activity-token", "123")
            .Build();

        var responseMessage = await HttpClient.SendAsync(message);
        var content = await responseMessage.Content.ReadAsStringAsync();
        var response = new Response<RetrieveUserResponse>(responseMessage, content);

        Assert.Multiple(() => 
        {
            Assert.That(response.IsSuccessful, Is.True);
            Assert.That(response.Data, Is.Not.Null);
        });

        Assert.That(response.Data.Users.Count, Is.LessThanOrEqualTo(retrieveUserRequestPayload.Limit));
    }

    [Test]
    public async Task retrieveUsersValidationTest()
    {
        RetrieveUserRequest retrieveUserRequestPayload = new RetrieveUserRequest();
        retrieveUserRequestPayload.UserId = Guid.Parse("4991a050-783c-4a83-9c5e-9ff9612d9191"); 
        retrieveUserRequestPayload.Limit = 100;
        var message = new HttpRequestMessageBuilder(
            HttpMethod.Post, HttpClient.BaseAddress!, "api/v2/user/retrieve/users")
            .WithPayload("application/json", retrieveUserRequestPayload)
            .WithHeader("pta-session-auth", "123")
            .WithHeader("pta-activity-token", "123")
            .Build();

        var responseMessage = await HttpClient.SendAsync(message);
        var content = await responseMessage.Content.ReadAsStringAsync();
        var response = new Response<RetrieveUserResponse>(responseMessage, content);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccessful, Is.False);
            Assert.That(response.ProblemDetails, Is.Not.Null);
        });

        Assert.That(response.ProblemDetails.Status, Is.EqualTo(401));
        Assert.That(response.ProblemDetails.Title, Is.EqualTo("Authentication Failed"));
        Assert.That(response.ProblemDetails.Detail, Is.EqualTo("User 4991a050-783c-4a83-9c5e-9ff9612d9191 is not listed as an admin"));
    }
    [Test]
    public async Task sendMessageTest()
    {
        var userId = Guid.Parse("4991a050-783c-4a83-9c5e-9ff9612d9191");
        SendMessageRequest sendMessageRequestPayload = new SendMessageRequest
        {
            UserId = userId,
            MessageContent = "hey can you see this",
            RecipientId = Guid.Parse("4b95e8ef-dd67-4aed-a62e-4fc7bf0a2b2f")
        };
        var message = new HttpRequestMessageBuilder(
            HttpMethod.Post, HttpClient.BaseAddress!, "api/v2/user/sendMessage")
            .WithPayload("application/json", sendMessageRequestPayload)
            .WithHeader("pta-session-auth", "123")
            .WithHeader("pta-activity-token", "123")
            .Build();

        var responseMessage = await HttpClient.SendAsync(message);
        var content = await responseMessage.Content.ReadAsStringAsync();
        var response = new Response<SendMessageResponse>(responseMessage, content);

        RetrieveUserRequest retrieveUserRequestPayload = new RetrieveUserRequest();
        retrieveUserRequestPayload.UserId = Guid.Parse("4b95e8ef-dd67-4aed-a62e-4fc7bf0a2b2f");
        retrieveUserRequestPayload.Offset = 0;
        retrieveUserRequestPayload.Limit = 0;
        var userMessage = new HttpRequestMessageBuilder(
            HttpMethod.Post,
            HttpClient.BaseAddress!,
            "api/v2/user/retrieve/users"
            ).WithPayload("application/json", retrieveUserRequestPayload)
            .WithHeader("pta-session-auth", "123")
            .WithHeader("pta-activity-token", "123")
            .Build();

        var userResponseMessage = await HttpClient.SendAsync(userMessage);
        var userContent = await userResponseMessage.Content.ReadAsStringAsync();
        var userResponse = new Response<RetrieveUserResponse>(userResponseMessage, userContent);

        RetrieveUserResponse? data = userResponse.Data;
        var user = data.Users
            .Where(x => x.UserId.Equals(userId)).First();
        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccessful, Is.True);
            Assert.That(response.ProblemDetails, Is.Null);
        });

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccessful, Is.True);
            Assert.That(response.ProblemDetails, Is.Null);
        });

        // Assert.That(userResponse)

        //Assert.That(response.Data.)
    }

    [Test]
    public async Task sendMessageValidationTest() //using non admin user
    {
        SendMessageRequest sendMessageRequestPayload = new SendMessageRequest
        {
            UserId = Guid.Parse("4b95e8ef-dd67-4aed-a62e-4fc7bf0a2b2f"),
            MessageContent = "hey can you see this",
            RecipientId = Guid.Parse("4991a050-783c-4a83-9c5e-9ff9612d9191")
        };
        var message = new HttpRequestMessageBuilder(
            HttpMethod.Post, HttpClient.BaseAddress!, "api/v2/user/sendMessage")
            .WithPayload("application/json", sendMessageRequestPayload)
            .WithHeader("pta-session-auth", "123")
            .WithHeader("pta-activity-token", "123")
            .Build();

        var responseMessage = await HttpClient.SendAsync(message);
        var content = await responseMessage.Content.ReadAsStringAsync();
        var response = new Response<SendMessageResponse>(responseMessage, content);


        RetrieveUserRequest retrieveUserRequestPayload = new RetrieveUserRequest();
        retrieveUserRequestPayload.UserId = Guid.Parse("4991a050-783c-4a83-9c5e-9ff9612d9191");
        var userMessage = new HttpRequestMessageBuilder(
            HttpMethod.Post,
            HttpClient.BaseAddress!,
            "api/v2/user/retrieve/users"
            ).WithPayload("application/json", retrieveUserRequestPayload)
            .WithHeader("pta-session-auth", "123")
            .WithHeader("pta-activity-token", "123")
            .Build();

        var userResponseMessage = await HttpClient.SendAsync(userMessage);
        var userContent = await userResponseMessage.Content.ReadAsStringAsync();
        var userResponse = new Response<RetrieveUserResponse>(userResponseMessage, userContent);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccessful, Is.True);
            Assert.That(response.ProblemDetails, Is.Null);
        });

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccessful, Is.True);
            Assert.That(response.ProblemDetails, Is.Null);
        });
    }
    [Test]
    public async Task replyTest() 
    {
        SendMessageRequest retrieveUserRequestPayload = new SendMessageRequest
        {
            UserId = Guid.Parse("4b95e8ef-dd67-4aed-a62e-4fc7bf0a2b2f"),
            MessageContent = "hey can you see this",
            RecipientId = Guid.Parse("4991a050-783c-4a83-9c5e-9ff9612d9191")
        };
        var message = new HttpRequestMessageBuilder(
            HttpMethod.Post, HttpClient.BaseAddress!, "api/v2/user/sendMessage")
            .WithPayload("application/json", retrieveUserRequestPayload)
            .WithHeader("pta-session-auth", "123")
            .WithHeader("pta-activity-token", "123")
            .Build();

        var responseMessage = await HttpClient.SendAsync(message);
        var content = await responseMessage.Content.ReadAsStringAsync();
        var response = new Response<RetrieveUserResponse>(responseMessage, content);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccessful, Is.True);
            Assert.That(response.ProblemDetails, Is.Null);
        });

       
       //Assert.That(response.Data.)
    }

    [Test]
    public async Task replyValidationTest()
    { 
    
    }

    [Test]
    public async Task loginTest()
    {
        LoginRequest retrieveUserRequestPayload = new LoginRequest
        {
            UserId = Guid.Parse("4b95e8ef-dd67-4aed-a62e-4fc7bf0a2b2f"),
            Password = "",
            Username = ""
        };
        var message = new HttpRequestMessageBuilder(
            HttpMethod.Post, HttpClient.BaseAddress!, "api/v2/user/login")
            .WithPayload("application/json", retrieveUserRequestPayload)
            .WithHeader("pta-session-auth", "123")
            .WithHeader("pta-activity-token", "123")
            .Build();

        var responseMessage = await HttpClient.SendAsync(message);
        var content = await responseMessage.Content.ReadAsStringAsync();
        var response = new Response<RetrieveUserResponse>(responseMessage, content);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccessful, Is.True);
            Assert.That(response.ProblemDetails, Is.Null);
        });


    }

    [Test]
    public async Task loginValidationTest() 
    {

    }

    [Test]
    public async Task logoutTest()
    {
        LoginRequest retrieveUserRequestPayload = new LoginRequest
        {
            UserId = Guid.Parse("4b95e8ef-dd67-4aed-a62e-4fc7bf0a2b2f"),
            Password = "",
            Username = ""
        };
        var message = new HttpRequestMessageBuilder(
            HttpMethod.Post, HttpClient.BaseAddress!, "api/v2/user/login")
            .WithPayload("application/json", retrieveUserRequestPayload)
            .WithHeader("pta-session-auth", "123")
            .WithHeader("pta-activity-token", "123")
            .Build();

        var responseMessage = await HttpClient.SendAsync(message);
        var content = await responseMessage.Content.ReadAsStringAsync();
        var response = new Response<RetrieveUserResponse>(responseMessage, content);

        Assert.Multiple(() =>
        {
            Assert.That(response.IsSuccessful, Is.True);
            Assert.That(response.ProblemDetails, Is.Null);
        });


    }

    [Test]
    public async Task logoutValidationTest()
    {

    }

    [OneTimeTearDown]
    public static void OneTimeTearDown()
    {
        HttpClient.Dispose();
    }
}
