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

internal class PokedexServiceTests
{
    private PokedexService sut;
    private PokedexCollectionImpl pokedexCollection;

    [OneTimeSetUp]
    public void Setup()
    {
        var mockRepository = Substitute.For<IRepositoryService>();
        pokedexCollection = new PokedexCollectionImpl();
        mockRepository.GetCollection<PokeDexItemDto>(MongoCollection.Pokedex).Returns(pokedexCollection);
        var mockLogger = Substitute.For<ILogger<PokedexService>>();
        sut = new PokedexService(mockRepository, Shared.DtoToModelMapper, mockLogger);
    }

    [Test]
    public async Task GetPokedexItem_Valid_ReturnsItem()
    {
        foreach (var expectedItem in pokedexCollection.Collection)
        {
            var actualItem = await sut.GetPokedexItem(expectedItem.TrainerId, expectedItem.GameId, expectedItem.DexNo);
            Assert.That(actualItem, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(actualItem.IsCaught, Is.EqualTo(expectedItem.IsCaught));
                Assert.That(actualItem.IsSeen, Is.EqualTo(expectedItem.IsSeen));
                Assert.That(actualItem.DexNo, Is.EqualTo(expectedItem.DexNo));
                Assert.That(actualItem.GameId, Is.EqualTo(expectedItem.GameId));
                Assert.That(actualItem.TrainerId, Is.EqualTo(expectedItem.TrainerId));
            });
        }
    }

    [Test]
    [TestCaseSource(nameof(GetSearchItems), new object[] { 3 })]
    public void GetPokedexItem_Invalid_Throws(SearchItem item)
    {
        var expectItem = pokedexCollection.Collection.First();
        var trainerId = item.TrainerId ?? expectItem.TrainerId;
        var gameId = item.GameId ?? expectItem.GameId;
        var dexNo = item.DexNo ?? expectItem.DexNo;
        var aggregateException = Assert.Throws<AggregateException>(() =>
        {
            var task = sut.GetPokedexItem(trainerId, gameId, dexNo);
            task.Wait();
        });

        Assert.That(aggregateException.InnerException, Is.TypeOf<UnknownEntityException<PokeDexItemDto>>());
        var exception = aggregateException.InnerException as UnknownEntityException<PokeDexItemDto>;
        Assert.Multiple(() =>
        {
            Assert.That(exception!.Title, Is.EqualTo(PtaExceptionParts.UnknownEntityTitle));
            Assert.That(
                exception.Message,
                Is.EqualTo($"Could not find a {typeof(PokeDexItemDto).Name} using {PropertyNames.TrainerId} {PropertyNames.GameId} {PropertyNames.DexNo}={(trainerId, gameId, dexNo)}"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        });
    }

    [Test]
    public async Task GetTrainerDex_Valid_ReturnsItems()
    {
        var expectItem = pokedexCollection.Collection.First();
        var actualItem = await sut.GetTrainerPokeDex(expectItem.TrainerId, expectItem.GameId);
        Assert.That(actualItem, Is.Not.Null.Or.Empty);
        Assert.That(actualItem, Has.Count.EqualTo(3));
        Assert.Multiple(() =>
        {
            foreach (var item in actualItem)
            {
                Assert.That(item.GameId, Is.EqualTo(expectItem.GameId));
                Assert.That(item.TrainerId, Is.EqualTo(expectItem.TrainerId));
            }
        });
    }

    [Test]
    public async Task PostDexItem_Valid_UpdatesCollection()
    {
        var count = pokedexCollection.Collection.Count;
        var trainerId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var dexNo = Random.Shared.Next(0, 901);
        var isSeen = Random.Shared.Next(2) == 0;
        var isCaught = isSeen && Random.Shared.Next(2) == 1;
        await sut.PostDexItem(trainerId, gameId, dexNo, isSeen, isCaught);
        var actualItem = await sut.GetPokedexItem(trainerId, gameId, dexNo);
        Assert.That(actualItem, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(actualItem.IsCaught, Is.EqualTo(isCaught));
            Assert.That(actualItem.IsSeen, Is.EqualTo(isSeen));
            Assert.That(actualItem.DexNo, Is.EqualTo(dexNo));
            Assert.That(actualItem.GameId, Is.EqualTo(gameId));
            Assert.That(actualItem.TrainerId, Is.EqualTo(trainerId));
            Assert.That(pokedexCollection.Collection, Has.Count.EqualTo(count + 1));
        });
    }

    [Test]
    public void PostDexItem_Duplicate_Throws()
    {
        var count = pokedexCollection.Collection.Count;
        var item = pokedexCollection.Collection.First();
        var aggregateException = Assert.Throws<AggregateException>(() =>
        {
            var task = sut.PostDexItem(item.TrainerId, item.GameId, item.DexNo, true, true);
            task.Wait();
        });

        Assert.That(aggregateException.InnerException, Is.TypeOf<DuplicateEntryException>());
        var exception = aggregateException.InnerException as DuplicateEntryException;
        Assert.Multiple(() =>
        {
            Assert.That(exception!.Title, Is.EqualTo(PtaExceptionParts.DuplicateEntryTitle));
            Assert.That(exception.Message, Is.EqualTo($"Duplicate entry of type {typeof(PokeDexItemDto).Name}"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(pokedexCollection.Collection, Has.Count.EqualTo(count));
        });
    }

    [Test]
    public async Task UpdateDexItemIsSeen_Valid_DoesNotThrow()
    {
        var trainerId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var dexNo = Random.Shared.Next(0, 901);
        var isSeen = false;
        var isCaught = false;
        await sut.PostDexItem(trainerId, gameId, dexNo, isSeen, isCaught);
        var updatedItem = await sut.UpdateDexItemIsSeen(trainerId, gameId, dexNo);
        var retrievedItem = await sut.GetPokedexItem(trainerId, gameId, dexNo);
        Assert.Multiple(() =>
        {
            Assert.That(updatedItem.IsSeen, Is.True);
            Assert.That(retrievedItem.IsSeen, Is.True);
            Assert.That(updatedItem.IsCaught, Is.False);
            Assert.That(retrievedItem.IsCaught, Is.False);
        });
    }

    [Test]
    public void UpdateDexItemIsSeen_NotFound_Throws()
    {
        var trainerId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var aggregateException = Assert.Throws<AggregateException>(() =>
        {
            var task = sut.UpdateDexItemIsSeen(trainerId, gameId, 0);
            task.Wait();
        });

        Assert.That(aggregateException.InnerException, Is.TypeOf<UpdateException>());
        var exception = aggregateException.InnerException as UpdateException;
        Assert.Multiple(() =>
        {
            Assert.That(exception!.Title, Is.EqualTo(PtaExceptionParts.UpdateErrorTitle));
            Assert.That(exception.Message, Is.EqualTo($"Failed to update {typeof(PokeDexItemDto).Name} {(trainerId, gameId)}"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        });
    }

    [Test]
    public async Task UpdateDexItemIsCaught_Valid_DoesNotThrow()
    {
        var trainerId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var dexNo = Random.Shared.Next(0, 901);
        var isSeen = false;
        var isCaught = false;
        await sut.PostDexItem(trainerId, gameId, dexNo, isSeen, isCaught);
        var updatedItem = await sut.UpdateDexItemIsCaught(trainerId, gameId, dexNo);
        var retrievedItem = await sut.GetPokedexItem(trainerId, gameId, dexNo);
        Assert.Multiple(() =>
        {
            Assert.That(updatedItem.IsSeen, Is.True);
            Assert.That(retrievedItem.IsSeen, Is.True);
            Assert.That(updatedItem.IsCaught, Is.True);
            Assert.That(retrievedItem.IsCaught, Is.True);
        });
    }

    [Test]
    public void UpdateDexItemIsCaught_NotFound_Throws()
    {
        var trainerId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        var aggregateException = Assert.Throws<AggregateException>(() =>
        {
            var task = sut.UpdateDexItemIsCaught(trainerId, gameId, 0);
            task.Wait();
        });

        Assert.That(aggregateException.InnerException, Is.TypeOf<UpdateException>());
        var exception = aggregateException.InnerException as UpdateException;
        Assert.Multiple(() =>
        {
            Assert.That(exception!.Title, Is.EqualTo(PtaExceptionParts.UpdateErrorTitle));
            Assert.That(exception.Message, Is.EqualTo($"Failed to update {typeof(PokeDexItemDto).Name} {(trainerId, gameId)}"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        });
    }

    [Test]
    public async Task DeleteDexItemForTrainer_Valid_RemoveItems()
    {
        var count = pokedexCollection.Collection.Count;
        var item = pokedexCollection.Collection.First();
        var trainerId = Guid.NewGuid();
        for (int i =0; i< 3; i++)
        {
            await sut.PostDexItem(trainerId, item.GameId, i, true, true);
        }

        await sut.DeleteDexItemForTrainer(trainerId, item.GameId);
        Assert.That(pokedexCollection.Collection, Has.Count.EqualTo(count));
    }

    [Test]
    [TestCaseSource(nameof(GetSearchItems), new object[] { 2 })]
    public async Task DeleteDexItemForTrainer_Invalid_DoesNotUpdateCollection(SearchItem item)
    {
        var count = pokedexCollection.Collection.Count;
        var pokedexItem = pokedexCollection.Collection.First();
        var trainerId = item.TrainerId ?? pokedexItem.TrainerId;
        var gameId = item.GameId ?? pokedexItem.GameId;
        await sut.DeleteDexItemForTrainer(trainerId, gameId);
        Assert.That(pokedexCollection.Collection, Has.Count.EqualTo(count));
    }

    private static SearchItem[] GetSearchItems(int count)
    {
        SearchItem[] items = 
        [
            new SearchItem{ TrainerId = Guid.NewGuid()},
            new SearchItem{ GameId = Guid.NewGuid()},
            new SearchItem{ DexNo = -1}
        ];

        return [.. items.Take(count)];
    }
}
