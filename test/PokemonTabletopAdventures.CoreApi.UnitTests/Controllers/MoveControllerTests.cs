using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PokemonTabletopAdventures.CoreApi.Controllers.v2;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.Models.Enums;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Controllers;

public class MoveControllerTests() : BaseIndexControllerTests<MoveDto>(DexType.Moves)
{
    private readonly ILogger<MoveController> _mockLogger = Substitute.For<ILogger<MoveController>>();

    [Test]
    public async Task GetMoves_Valid_ReturnsMoves()
    {
        var sut = BuildSut();
        var response = await sut.GetMoves(0, 100);
        MultipleItemsTest(response);
    }

    [Test]
    public async Task GetMove_Valid_ReturnsMove()
    {
        var sut = BuildSut();
        var response = await sut.GetMove("Item 1");
        SingleItemTest(response);
    }
    
    private MoveController BuildSut()
    {
        return new MoveController(DexService, _mockLogger)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }
}