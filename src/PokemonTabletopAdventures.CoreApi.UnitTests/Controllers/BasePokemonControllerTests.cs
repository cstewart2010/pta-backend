using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Controllers.v2;
using PokemonTabletopAdventures.CoreApi.Domain;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.CoreApi.UnitTests.Implementations;
using PokemonTabletopAdventures.Models.Indicies;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Controllers;

public class BasePokemonControllerTests
{
    private IRepositoryService _mockRepository;
    private ILogger<BasePokemonController> _mockLogger;
    private BasePokemonCollectionImpl _basePokemonCollection;

    [OneTimeSetUp]
    public void Setup()
    {
        _mockRepository = Substitute.For<IRepositoryService>();
        _basePokemonCollection = new BasePokemonCollectionImpl();
        _mockRepository.GetCollection<BasePokemonDto>(MongoCollection.BasePokemon).Returns(_basePokemonCollection);
        _mockLogger = Substitute.For<ILogger<BasePokemonController>>();
    }

    [Test]
    public async Task GetAllPokemon_Test()
    {
        var sut = BuildSut();
        var response = await sut.GetAllPokemon();
        var result = response as ObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(result.Value, Is.TypeOf<IndexCollectionResponse>());
        });
    }

    private BasePokemonController BuildSut()
    {
        var dexService = new DexService(_mockRepository, Shared.DtoToModelMapper, Shared.ModelToDtoMapper, Substitute.For<ILogger<DexService>>());
        return new BasePokemonController(dexService, _mockLogger)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }
}