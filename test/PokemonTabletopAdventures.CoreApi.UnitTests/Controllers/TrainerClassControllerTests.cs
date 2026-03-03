using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PokemonTabletopAdventures.CoreApi.Controllers.v2;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.Models.Enums;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Controllers;

public class TrainerClassControllerTests() : BaseIndexControllerTests<TrainerClassDto>(DexType.TrainerClasses)
{
    private readonly ILogger<TrainerClassController> _mockLogger = Substitute.For<ILogger<TrainerClassController>>();

    [Test]
    public async Task GetClasses_Valid_ReturnsClasses()
    {
        var sut = BuildSut();
        var response = await sut.GetClasses(0, 100);
        MultipleItemsTest(response);
    }

    [Test]
    public async Task GetClass_Valid_ReturnsClass()
    {
        var sut = BuildSut();
        var response = await sut.GetClass("Item 1");
        SingleItemTest(response);
    }
    
    private TrainerClassController BuildSut()
    {
        return new TrainerClassController(DexService, _mockLogger)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }
}