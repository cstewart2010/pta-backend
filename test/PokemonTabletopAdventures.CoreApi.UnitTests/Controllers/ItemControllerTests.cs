using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PokemonTabletopAdventures.CoreApi.Controllers.v2;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.Models.Enums;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Controllers;

public class ItemControllerTests() : BaseIndexControllerTests<BaseItemDto>(DexType.KeyItems, DexType.MedicalItems, DexType.Pokeballs, DexType.PokemonItems, DexType.TrainerEquipment)
{
    private readonly ILogger<ItemController> _mockLogger = Substitute.For<ILogger<ItemController>>();

    [Test]
    public async Task GetKeyItems_Valid_ReturnsItems()
    {
        var sut = BuildSut();
        var response = await sut.GetKeyItems(0, 100);
        MultipleItemsTest(response);
    }

    [Test]
    public async Task GetKeyItem_Valid_ReturnsItems()
    {
        var sut = BuildSut();
        var response = await sut.GetKeyItem("Item 1");
        SingleItemTest(response);
    }

    [Test]
    public async Task GetMedicalItems_Valid_ReturnsItems()
    {
        var sut = BuildSut();
        var response = await sut.GetMedicalItems(0, 100);
        MultipleItemsTest(response);
    }

    [Test]
    public async Task GetMedicalItem_Valid_ReturnsItems()
    {
        var sut = BuildSut();
        var response = await sut.GetMedicalItem("Item 1");
        SingleItemTest(response);
    }

    [Test]
    public async Task GetPokeballs_Valid_ReturnsItems()
    {
        var sut = BuildSut();
        var response = await sut.GetPokeballs(0, 100);
        MultipleItemsTest(response);
    }

    [Test]
    public async Task GetPokeball_Valid_ReturnsItems()
    {
        var sut = BuildSut();
        var response = await sut.GetPokeball("Item 1");
        SingleItemTest(response);
    }

    [Test]
    public async Task GetPokemonItems_Valid_ReturnsItems()
    {
        var sut = BuildSut();
        var response = await sut.GetPokemonItems(0, 100);
        MultipleItemsTest(response);
    }

    [Test]
    public async Task GetPokemonItem_Valid_ReturnsItems()
    {
        var sut = BuildSut();
        var response = await sut.GetPokemonItem("Item 1");
        SingleItemTest(response);
    }

    [Test]
    public async Task GetTrainerItems_Valid_ReturnsItems()
    {
        var sut = BuildSut();
        var response = await sut.GetTrainerItems(0, 100);
        MultipleItemsTest(response);
    }

    [Test]
    public async Task GetTrainerItem_Valid_ReturnsItems()
    {
        var sut = BuildSut();
        var response = await sut.GetTrainerItem("Item 1");
        SingleItemTest(response);
    }

    private ItemController BuildSut()
    {
        return new ItemController(DexService, _mockLogger)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }
}