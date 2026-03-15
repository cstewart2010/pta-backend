using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Domain;
using PokemonTabletopAdventures.CoreApi.DTOs;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.CoreApi.UnitTests.Implementations;
using PokemonTabletopAdventures.Models.Enums;
using PokemonTabletopAdventures.Models.Indicies;
using PokemonTabletopAdventures.Models.Interfaces;

namespace PokemonTabletopAdventures.CoreApi.UnitTests;

public abstract class BaseIndexControllerTests<TIndex> 
    where TIndex : class, IDocument, IDexDocument, new()
{
    protected BaseIndexControllerTests(params DexType[] types)
    {
        var mockRepository = Substitute.For<IRepositoryService>();
        var basePokemonCollection = new BasePokemonCollectionImpl();
        ICollection<TIndex> mockIndexCollection =
        [
            new TIndex
            {
                Name = "Item 1"
            },
            new TIndex
            {
                Name = "Item 2"
            }
        ];
        var indexCollection = new IndexCollectionImpl<TIndex>(mockIndexCollection);
        mockRepository.GetCollection<BasePokemonDto>(MongoCollection.BasePokemon).Returns(basePokemonCollection);
        foreach (var type in types)
        {
            mockRepository.GetCollection<TIndex>(type.ToString()).Returns(indexCollection);
        }
        DexService = new DexService(mockRepository, Shared.DtoToModelMapper, Shared.ModelToDtoMapper, Substitute.For<ILogger<DexService>>());
    }

    protected void MultipleItemsTest(IActionResult response)
    {
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<IndexCollectionResponse>());
        }
    }

    protected void SingleItemTest(IActionResult response)
    {
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<IndexResponse<TIndex>>());
        }
    }
    
    protected DexService DexService { get; }
}