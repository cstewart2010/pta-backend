using System.Net;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Domain;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.CoreApi.UnitTests.Implementations;
using PokemonTabletopAdventures.Models.Games;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Services;

public class SpriteServiceTests
{
    private SpriteService _sut;
    private SpriteCollectionImpl  _spriteCollection;

    [OneTimeSetUp]
    public void Setup()
    {
        _spriteCollection = new SpriteCollectionImpl();
        var mockRepository = Substitute.For<IRepositoryService>();
        mockRepository.GetCollection<SpriteDto>(MongoCollection.Sprites).Returns(_spriteCollection);
        _sut = new SpriteService(mockRepository, Substitute.For<ILogger<SpriteService>>());
    }

    [Test]
    public async Task GetAllSprites_ReturnsAllSprites()
    {
        var actual = await _sut.GetAllSprites();
        Assert.That(actual, Is.Not.Null.And.Count.EqualTo(_spriteCollection.Collection.Count));
        Assert.Multiple(() =>
        {
            foreach (var expected in _spriteCollection.Collection)
            {
                Assert.That(
                    actual.SingleOrDefault(x => x.FriendlyText == expected.FriendlyText && x.Value == expected.Value),
                    Is.Not.Null);
            }
        });
    }

    [Test]
    public async Task PostSprite_Valid_AddsToCollection()
    {
        var expected = new Sprite
        {
            FriendlyText = Guid.NewGuid().ToString(),
            Value = Guid.NewGuid().ToString()
        };
        var count = _spriteCollection.Collection.Count;
        await _sut.PostSprite(expected);
        Assert.That(_spriteCollection.Collection, Has.Count.EqualTo(count + 1));
        Assert.That(_spriteCollection.Collection.SingleOrDefault(x => x.FriendlyText == expected.FriendlyText && x.Value == expected.Value), Is.Not.Null);
    }

    [Test]
    [TestCase(null, "valid")]
    [TestCase("valid", null)]
    public void PostSprite_Invalid_ThrowsException(string? friendlyText, string? value)
    {
        var expected = new Sprite
        {
            FriendlyText = friendlyText!,
            Value = value!
        };
        var count = _spriteCollection.Collection.Count;
        var aggregateException = Assert.Throws<AggregateException>(_sut.PostSprite(expected).Wait);
        Assert.Multiple(() =>
        {
            Assert.That(_spriteCollection.Collection, Has.Count.EqualTo(count));
            Assert.That(aggregateException.InnerException, Is.InstanceOf<PtaException>());
        });
        Assert.Multiple(() =>
        {
            var exception = (PtaException)aggregateException.InnerException!;
            Assert.That(exception.Message, Is.EqualTo("Cannot add sprite with empty field"));
            Assert.That(exception.Title, Is.EqualTo("Invalid Sprite"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        });
    }
}