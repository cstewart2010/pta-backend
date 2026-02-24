using Microsoft.Extensions.Logging;
using NSubstitute;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Domain;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.CoreApi.UnitTests.Implementations;
using PokemonTabletopAdventures.Models.Enums;
using PokemonTabletopAdventures.Models.Users;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Services;

public class UserServiceTests
{
    private UserService _sut;
    private UserCollectionImpl _userCollection;

    [OneTimeSetUp]
    public void Setup()
    {
        var mockRepository = Substitute.For<IRepositoryService>();
        var mockLogger = Substitute.For<ILogger<UserService>>();
        var trainerCollection = new TrainerCollectionImpl();
        var gameCollection = new GameCollectionImpl();
        _userCollection = new UserCollectionImpl();
        var pokemonCollection = new PokemonCollectionImpl();
        var pokedexCollection = new PokedexCollectionImpl();
        var npcCollection = new NpcCollectionImpl();
        var shopCollection = new ShopCollectionImpl();
        var settingCollection = new SettingCollectionImpl(trainerCollection,  pokemonCollection, npcCollection, shopCollection);
        mockRepository.GetCollection<ShopDto>(MongoCollection.Shops).Returns(shopCollection);
        mockRepository.GetCollection<SettingDto>(MongoCollection.Settings).Returns(settingCollection);
        mockRepository.GetCollection<GameDto>(MongoCollection.Games).Returns(gameCollection);
        mockRepository.GetCollection<UserDto>(MongoCollection.Users).Returns(_userCollection);
        mockRepository.GetCollection<PokeDexItemDto>(MongoCollection.Pokedex).Returns(pokedexCollection);
        mockRepository.GetCollection<PokemonDto>(MongoCollection.Pokemon).Returns(pokemonCollection);
        mockRepository.GetCollection<TrainerDto>(MongoCollection.Trainers).Returns(trainerCollection);
        mockRepository.GetCollection<NpcDto>(MongoCollection.NPCs).Returns(npcCollection);
        var pokedexService = new PokedexService(mockRepository, Shared.DtoToModelMapper, Substitute.For<ILogger<PokedexService>>());
        var pokemonService = new PokemonService(mockRepository, pokedexService, Shared.DtoToModelMapper, Shared.ModelToDtoMapper, Substitute.For<ILogger<PokemonService>>());
        var shopService = new ShopService(mockRepository, Shared.DtoToModelMapper, Shared.ModelToDtoMapper, Substitute.For<ILogger<ShopService>>());
        var settingService = new SettingService(mockRepository, shopService, Shared.DtoToModelMapper, Shared.ModelToDtoMapper, Substitute.For<ILogger<SettingService>>());
        var trainerService = new TrainerService(mockRepository, pokemonService, pokedexService, settingService, Shared.DtoToModelMapper, Shared.ModelToDtoMapper, Substitute.For<ILogger<TrainerService>>());
        _sut = new UserService(mockRepository, trainerService, Shared.DtoToModelMapper, Shared.ModelToDtoMapper, mockLogger);
    }

    [Test]
    public async Task GetUserById_Valid_ReturnsUser()
    {
        var userId = Shared.UserIds.First();
        var expected = _userCollection.Collection.First(x => x.UserId == userId);
        var user = await _sut.GetUserById(userId);
        Assert.Multiple(() =>
        {
            Assert.That(user.Games, Is.EquivalentTo(expected.Games));
            Assert.That(user.Messages, Is.EquivalentTo(expected.Messages));
            Assert.That(user.SiteRole, Is.EqualTo(expected.SiteRole));
            Assert.That(user.UserId, Is.EqualTo(expected.UserId));
            Assert.That(user.Username, Is.EqualTo(expected.Username));
            Assert.That(user.ActivityToken, Is.EqualTo(expected.ActivityToken));
            Assert.That(user.DateCreated, Is.EqualTo(expected.DateCreated));
        });
    }

    [Test]
    public async Task GetUserByUsername_Valid_ReturnsUser()
    {
        var userId = Shared.UserIds.First();
        var expected = _userCollection.Collection.First(x => x.UserId == userId);
        var user = await _sut.GetUserByUsername(expected.Username);
        Assert.Multiple(() =>
        {
            Assert.That(user.Games, Is.EquivalentTo(expected.Games));
            Assert.That(user.Messages, Is.EquivalentTo(expected.Messages));
            Assert.That(user.SiteRole, Is.EqualTo(expected.SiteRole));
            Assert.That(user.UserId, Is.EqualTo(expected.UserId));
            Assert.That(user.Username, Is.EqualTo(expected.Username));
            Assert.That(user.ActivityToken, Is.EqualTo(expected.ActivityToken));
            Assert.That(user.DateCreated, Is.EqualTo(expected.DateCreated));
        });
    }

    [Test]
    public async Task GetUsers_ReturnsUsers()
    {
        var expected = _userCollection.Collection;
        var users = await _sut.GetUsers();
        Assert.That(users.Select(x => x.UserId), Is.EquivalentTo(expected.Select(x => x.UserId)));
    }

    [Test]
    public async Task GetUsers_WithOptions_ReturnsUsers()
    {
        var expected = _userCollection.Collection.Skip(1).Take(1);
        var users = await _sut.GetUsers(1, 1);
        Assert.That(users.Select(x => x.UserId), Is.EquivalentTo(expected.Select(x => x.UserId)));
    }

    [Test]
    public async Task PostUser_Valid_AddsUser()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            ActivityToken = userId.ToString(),
            DateCreated = DateTime.Now,
            Games = [],
            Messages = [],
            SiteRole = UserRoleOnSite.IpBanned,
            UserId = userId,
            Username = userId.ToString()
        };
        
        var passwordHash = Guid.NewGuid().ToString();
        
        await _sut.PostUser(user, passwordHash);
        var actual = _userCollection.Collection.First(x => x.UserId == userId);
        Assert.Multiple(() =>
            {
                Assert.That(actual.Games, Is.EquivalentTo(user.Games));
                Assert.That(actual.Messages, Is.EquivalentTo(user.Messages));
                Assert.That(actual.SiteRole, Is.EqualTo(UserRoleOnSite.IpBanned));
                Assert.That(actual.UserId, Is.EqualTo(userId));
                Assert.That(actual.Username, Is.EqualTo(user.Username));
                Assert.That(actual.ActivityToken, Is.Empty);
                Assert.That(actual.DateCreated, Is.EqualTo(user.DateCreated));
                Assert.That(actual.PasswordHash, Is.EqualTo(passwordHash));
            }
        );
    }

    [Test]
    public async Task UpdateUser_Null_DoesNotUpdatesUser()
    {
        var userId = Shared.UserIds.First();
        var expected = await _sut.GetUserById(userId);
        var user = new User
        {
            UserId = userId,
            Username = null!,
            DateCreated = DateTime.Now,
            Games = null!,
            Messages = null!,
            SiteRole = UserRoleOnSite.Deactivated,
            ActivityToken = null!
        };
        
        var actual = await _sut.UpdateUser(user);
        Assert.Multiple(() =>
        {
            Assert.That(actual.Games, Is.EquivalentTo(expected.Games));
            Assert.That(actual.Messages, Is.EquivalentTo(expected.Messages));
            Assert.That(actual.Username, Is.EqualTo(expected.Username));
            Assert.That(actual.ActivityToken, Is.EqualTo(expected.ActivityToken));
            Assert.That(actual.DateCreated, Is.EqualTo(expected.DateCreated));
            Assert.That(actual.SiteRole, Is.EqualTo(user.SiteRole));
        });
    }

    [Test]
    public async Task UpdateUser_NotNull_UpdatesUser()
    {
        var userId = Shared.UserIds.Last();
        var expected = await _sut.GetUserById(userId);
        var user = new User
        {
            UserId = userId,
            Username = Guid.NewGuid().ToString(),
            DateCreated = DateTime.Now,
            Games = [Guid.NewGuid()],
            Messages = [Guid.NewGuid()],
            SiteRole = UserRoleOnSite.Deactivated,
            ActivityToken = Guid.NewGuid().ToString()
        };
        
        var actual = await _sut.UpdateUser(user);
        Assert.Multiple(() =>
        {
            Assert.That(actual.Games, Is.EquivalentTo(user.Games));
            Assert.That(actual.Messages, Is.EquivalentTo(user.Messages));
            Assert.That(actual.Username, Is.EqualTo(user.Username));
            Assert.That(actual.ActivityToken, Is.EqualTo(expected.ActivityToken));
            Assert.That(actual.DateCreated, Is.EqualTo(expected.DateCreated));
            Assert.That(actual.SiteRole, Is.EqualTo(user.SiteRole));
        });
    }

    [Test]
    public void UpdateUser_Invalid_ThrowsUserNotFoundException()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            UserId = userId,
            Username = null!,
            DateCreated = DateTime.Now,
            Games = null!,
            Messages = null!,
            SiteRole = UserRoleOnSite.Deactivated,
            ActivityToken = null!
        };
        
        var expected = Assert.Throws<AggregateException>(_sut.UpdateUser(user).Wait);
        Assert.That(expected.InnerException, Is.TypeOf<UserNotFoundException>());
    }

    [Test]
    public async Task UpdateUserActivityToken_Valid_Updates()
    {
        var userId = Shared.UserIds.First();
        var token =  Guid.NewGuid().ToString();
        var actual = await _sut.UpdateUserActivityToken(userId, token);
        Assert.That(actual.ActivityToken, Is.EqualTo(token));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task UpdateUserOnlineStatus_Valid_Updates(bool isOnline)
    {
        var userId = Shared.UserIds.First();
        await _sut.UpdateUserOnlineStatus(userId, isOnline);
        var actual = _userCollection.Collection.First(x => x.UserId == userId);
        Assert.That(actual.IsOnline, Is.EqualTo(isOnline));
    }
    
    [Test, NonParallelizable]
    public async Task DeleteUser_Valid_Deletes()
    {
        var userId = Shared.UserIds.First();
        await _sut.DeleteUser(userId);
        Assert.That(_userCollection.Collection.Where(x => x.UserId == userId), Is.Empty);
    }
}