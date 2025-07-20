using Microsoft.Extensions.Logging;
using NSubstitute;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Domain;
using PokemonTabletopAdventures.CoreApi.Domain.Mappers;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.CoreApi.UnitTests.Implementations;
using PokemonTabletopAdventures.Models.Pokedex;
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
        sut = new PokedexService(mockRepository, new DtoToModelMapper(), mockLogger);
    }

    [Test]
    public async Task GetPokedexItem_Valid_ReturnsItem()
    {
        var expectItem = pokedexCollection.PokedexItems.First();
        var actualItem = await sut.GetPokedexItem(expectItem.TrainerId, expectItem.GameId, expectItem.DexNo);
        Assert.That(actualItem, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(actualItem.IsCaught, Is.EqualTo(expectItem.IsCaught));
            Assert.That(actualItem.IsSeen, Is.EqualTo(expectItem.IsSeen));
            Assert.That(actualItem.DexNo, Is.EqualTo(expectItem.DexNo));
            Assert.That(actualItem.GameId, Is.EqualTo(expectItem.GameId));
            Assert.That(actualItem.TrainerId, Is.EqualTo(expectItem.TrainerId));
        });
    }

    [Test]
    [TestCaseSource(nameof(GetSearchItems))]
    public void GetPokedexItem_Invalid_Throws(SearchItem item)
    {
        var expectItem = pokedexCollection.PokedexItems.First();
        var aggregateException = Assert.Throws<AggregateException>(() =>
        {
            var task = sut.GetPokedexItem(item.TrainerId ?? expectItem.TrainerId, item.GameId ?? expectItem.GameId, item.DexNo ?? expectItem.DexNo);
            task.Wait();
        });

        Assert.That(aggregateException.InnerException, Is.TypeOf<UnknownEntityException<PokedexItem>>());
        var exception = aggregateException.InnerException as UnknownEntityException<PokedexItem>;
        Assert.Multiple(() =>
        {
            Assert.That(exception!.Title, Is.EqualTo(PtaExceptionParts.UnknownEntityTitle));
            Assert.That(exception.Message, Is.EqualTo($"Could not find a {typeof(PokedexItem).Name} using {PropertyNames.DexNo}={item.DexNo ?? expectItem.DexNo}"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        });
    }

    private static SearchItem[] GetSearchItems()
    {
        return
        [
            new SearchItem{ TrainerId = Guid.NewGuid()},
            new SearchItem{ GameId = Guid.NewGuid()},
            new SearchItem{ DexNo = -1}
        ];
    }

    public class SearchItem
    {
        public Guid? TrainerId { get; set; }
        public Guid? GameId { get; set; }
        public int? DexNo { get; set; }
    }
}
