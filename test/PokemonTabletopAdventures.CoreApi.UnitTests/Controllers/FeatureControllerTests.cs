using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PokemonTabletopAdventures.CoreApi.Controllers.v2;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.Models.Enums;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Controllers;

public class FeatureControllerTests() : BaseIndexControllerTests<FeatureDto>(DexType.Features)
{
    private readonly ILogger<FeatureController> _mockLogger = Substitute.For<ILogger<FeatureController>>();

    [Test]
    public async Task GetFeatures_Valid_ReturnsBerries()
    {
        var sut = BuildSut();
        var response = await sut.GetFeatures(0, 100);
        MultipleItemsTest(response);
    }

    [Test]
    public async Task GetFeature_Valid_ReturnsBerry()
    {
        var sut = BuildSut();
        var response = await sut.GetFeature("Item 1");
        SingleItemTest(response);
    }

    private FeatureController BuildSut()
    {
        return new FeatureController(DexService, _mockLogger)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }
}