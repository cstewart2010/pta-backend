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
using PokemonTabletopAdventures.Models.Enums;
using PokemonTabletopAdventures.Models.Indicies;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Controllers;

public class BerryControllerTests() : BaseIndexControllerTests<BerryDto>(DexType.Berries)
{
    private readonly ILogger<BerryController> _mockLogger = Substitute.For<ILogger<BerryController>>();

    [Test]
    public async Task GetItems_Valid_ReturnsBerries()
    {
        var sut = BuildSut();
        var response = await sut.GetBerries(0, 100);
        MultipleItemsTest(response);
    }

    [Test]
    public async Task GetItem_Valid_ReturnsBerry()
    {
        var sut = BuildSut();
        var response = await sut.GetBerry("Item 1");
        SingleItemTest(response);
    }

    private BerryController BuildSut()
    {
        return new BerryController(DexService, _mockLogger)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }
}