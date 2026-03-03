using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PokemonTabletopAdventures.CoreApi.Controllers.v2;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.Models.Enums;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Controllers;

public class OriginsControllerTests() : BaseIndexControllerTests<OriginDto>(DexType.Origins)
{
    private readonly ILogger<OriginsController> _mockLogger = Substitute.For<ILogger<OriginsController>>();

    [Test]
    public async Task GetOrigins_Valid_ReturnsOrigins()
    {
        var sut = BuildSut();
        var response = await sut.GetOrigins(0, 100);
        MultipleItemsTest(response);
    }

    [Test]
    public async Task GetOrigin_Valid_ReturnsOrigin()
    {
        var sut = BuildSut();
        var response = await sut.GetOrigin("Item 1");
        SingleItemTest(response);
    }
    
    private OriginsController BuildSut()
    {
        return new OriginsController(DexService, _mockLogger)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }
}