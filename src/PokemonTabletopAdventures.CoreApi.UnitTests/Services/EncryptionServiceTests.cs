using Microsoft.Extensions.Logging;
using NSubstitute;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Domain;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.CoreApi.UnitTests.Implementations;
using System.Net;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Services;

internal class EncryptionServiceTests
{
    private EncryptionService sut;
    private GameCollectionImpl gameCollection;
    private UserCollectionImpl userCollection;

    [OneTimeSetUp]
    public void SetUp()
    {
        var mockRepository = Substitute.For<IRepositoryService>();
        gameCollection = new GameCollectionImpl();
        userCollection = new UserCollectionImpl();
        mockRepository.GetCollection<GameDto>(MongoCollection.Games).Returns(gameCollection);
        mockRepository.GetCollection<UserDto>(MongoCollection.Users).Returns(userCollection);
        var mockLogger = Substitute.For<ILogger<EncryptionService>>();
        sut = new EncryptionService(mockRepository, mockLogger);
    }

    [Test]
    [TestCase("01/01/1990", "AEBNO0HstQg=")]
    [TestCase("01/01/2020", "AAB8i02O1wg=")]
    public async Task GenerateToken_ReturnsValid(string date, string expected)
    {
        var dateTime = DateTime.Parse(date);
        var token = await sut.GenerateToken(dateTime);
        Assert.That(token, Is.EqualTo(expected));
    }

    [Test]
    [TestCase("secure_password")]
    [TestCase("unsecure_password")]
    public async Task HashSecret_ReturnsValid(string secret)
    {
        var hash = await sut.HashSecret(secret);
        Assert.That(hash, Is.Not.Null);
        Assert.That(hash, Has.Length.EqualTo(60));
    }

    [Test]
    [TestCase("01/01/1990", "AEBNO0HstQg=")]
    [TestCase("01/01/2020", "AAB8i02O1wg=")]
    public void ValidateToken_ValidInputs_DoesNotThrow(string date, string token)
    {
        var dateTime = DateTime.Parse(date).AddMinutes(30);
        Assert.DoesNotThrow(() =>
        {
            var task = sut.ValidateToken(token, dateTime);
            task.Wait();
        });
    }

    [Test]
    [TestCase("01/01/1990", "")]
    [TestCase("01/01/2020", null)]
    public void ValidateToken_NullOrEmpty_Throws(string date, string? token)
    {
        var dateTime = DateTime.Parse(date).AddMinutes(30);
        var aggregateException = Assert.Throws<AggregateException>(() =>
        {
            var task = sut.ValidateToken(token!, dateTime);
            task.Wait();
        });

        Assert.That(aggregateException.InnerException, Is.TypeOf<PtaUnauthorizedException>());
        var exception = aggregateException.InnerException as PtaUnauthorizedException;
        Assert.Multiple(() =>
        {
            Assert.That(exception!.Title, Is.EqualTo(PtaExceptionParts.AuthenticationErrorTitle));
            Assert.That(exception.Message, Is.EqualTo(PtaExceptionParts.EmptyTokenMessage));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        });
    }

    [Test]
    [TestCase("01/01/1990", "a")]
    [TestCase("01/01/2020", "ab")]
    public void ValidateToken_InvalidToken_Throws(string date, string? token)
    {
        var dateTime = DateTime.Parse(date).AddMinutes(30);
        var aggregateException = Assert.Throws<AggregateException>(() =>
        {
            var task = sut.ValidateToken(token!, dateTime);
            task.Wait();
        });

        Assert.That(aggregateException.InnerException, Is.TypeOf<PtaUnauthorizedException>());
        var exception = aggregateException.InnerException as PtaUnauthorizedException;
        Assert.Multiple(() =>
        {
            Assert.That(exception!.Title, Is.EqualTo(PtaExceptionParts.AuthenticationErrorTitle));
            Assert.That(exception.Message, Is.EqualTo("The input is not a valid Base-64 string as it contains a non-base 64 character, more than two padding characters, or an illegal character among the padding characters."));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        });
    }

    [Test]
    [TestCase("01/01/1990", "AEBNO0HstQg=", 90)]
    [TestCase("01/01/2020", "AAB8i02O1wg=", -90)]
    public void ValidateToken_ExpiredToken_Throws(string date, string? token, int timeChange)
    {
        var dateTime = DateTime.Parse(date).AddMinutes(timeChange);
        var aggregateException = Assert.Throws<AggregateException>(() =>
        {
            var task = sut.ValidateToken(token!, dateTime);
            task.Wait();
        });

        Assert.That(aggregateException.InnerException, Is.TypeOf<PtaUnauthorizedException>());
        var exception = aggregateException.InnerException as PtaUnauthorizedException;
        Assert.Multiple(() =>
        {
            Assert.That(exception!.Title, Is.EqualTo(PtaExceptionParts.AuthenticationErrorTitle));
            Assert.That(exception.Message, Is.EqualTo(PtaExceptionParts.ExpiredTokenMessage));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        });
    }

    [Test]
    [TestCase("a")]
    [TestCase("aa")]
    [TestCase("secure_password")]
    public async Task VerifySecret_Game_Valid_DoesNotThrow(string secret)
    {
        var game = gameCollection.Collection.First();
        game.PasswordHash = await sut.HashSecret(secret);
        Assert.DoesNotThrow(() =>
        {
            var task = sut.VerifySecret(secret, game.GameId);
            task.Wait();
        });
    }

    [Test]
    public void VerifySecret_Game_Unknown_Throw()
    {
        var id = Guid.NewGuid();
        var aggregateException = Assert.Throws<AggregateException>(() =>
        {
            var task = sut.VerifySecret(string.Empty, id);
            task.Wait();
        });

        Assert.That(aggregateException.InnerException, Is.TypeOf<UnknownEntityException<GameDto>>());
        var exception = aggregateException.InnerException as UnknownEntityException<GameDto>;
        Assert.Multiple(() =>
        {
            Assert.That(exception!.Title, Is.EqualTo(PtaExceptionParts.UnknownEntityTitle));
            Assert.That(exception.Message, Is.EqualTo($"Could not find a {typeof(GameDto).Name} using {PropertyNames.GameId}={id}"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        });
    }

    [Test]
    [TestCase("a", "aa")]
    [TestCase("aa", "secure_password")]
    [TestCase("secure_password", "a")]
    public async Task VerifySecret_Game_Invalid_Throws(string secret, string invalid)
    {
        var game = gameCollection.Collection.First();
        game.PasswordHash = await sut.HashSecret(secret);
        var aggregateException = Assert.Throws<AggregateException>(() =>
        {
            var task = sut.VerifySecret(invalid, game.GameId);
            task.Wait();
        });

        Assert.That(aggregateException.InnerException, Is.TypeOf<PtaUnauthorizedException>());
        var exception = aggregateException.InnerException as PtaUnauthorizedException;
        Assert.Multiple(() =>
        {
            Assert.That(exception!.Title, Is.EqualTo(PtaExceptionParts.AuthenticationErrorTitle));
            Assert.That(exception.Message, Is.EqualTo(PtaExceptionParts.InvalidSecretMessage));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        });
    }

    [Test]
    [TestCase("a")]
    [TestCase("aa")]
    [TestCase("secure_password")]
    public async Task VerifySecret_User_Valid_DoesNotThrow(string secret)
    {
        var user = userCollection.Collection.First();
        user.PasswordHash = await sut.HashSecret(secret);
        Assert.DoesNotThrow(() =>
        {
            var task = sut.VerifySecret(secret, user.Username);
            task.Wait();
        });
    }

    [Test]
    public void VerifySecret_User_Unknown_Throw()
    {
        var username = "invalid_user";
        var aggregateException = Assert.Throws<AggregateException>(() =>
        {
            var task = sut.VerifySecret(string.Empty, username);
            task.Wait();
        });

        Assert.That(aggregateException.InnerException, Is.TypeOf<PtaUnauthorizedException>());
        var exception = aggregateException.InnerException as PtaUnauthorizedException;
        Assert.Multiple(() =>
        {
            Assert.That(exception!.Title, Is.EqualTo(PtaExceptionParts.AuthenticationErrorTitle));
            Assert.That(exception.Message, Is.EqualTo(PtaExceptionParts.NoUserFoundMessage));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        });
    }

    [Test]
    [TestCase("a", "aa")]
    [TestCase("aa", "secure_password")]
    [TestCase("secure_password", "a")]
    public async Task VerifySecret_User_Invalid_Throws(string secret, string invalid)
    {
        var user = userCollection.Collection.First();
        user.PasswordHash = await sut.HashSecret(secret);
        var aggregateException = Assert.Throws<AggregateException>(() =>
        {
            var task = sut.VerifySecret(invalid, user.Username);
            task.Wait();
        });

        Assert.That(aggregateException.InnerException, Is.TypeOf<PtaUnauthorizedException>());
        var exception = aggregateException.InnerException as PtaUnauthorizedException;
        Assert.Multiple(() =>
        {
            Assert.That(exception!.Title, Is.EqualTo(PtaExceptionParts.AuthenticationErrorTitle));
            Assert.That(exception.Message, Is.EqualTo(PtaExceptionParts.InvalidSecretMessage));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        });
    }
}
