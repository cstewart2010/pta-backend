using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Newtonsoft.Json;
using NSubstitute;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Domain;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.CoreApi.UnitTests.Implementations;
using PokemonTabletopAdventures.Models.Enums;
using PokemonTabletopAdventures.Models.Pokemons;
using System.Net;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Services;

[TestFixture]
public class DexServiceTests
{
    private DexService sut;
    private Pokemon pokemon;
    private Pokemon pokemon2;
    private BasePokemonCollectionImpl basePokemonCollection;
    [OneTimeSetUp]
    public void SetUp()
    {
        var mockRepository = Substitute.For<IRepositoryService>();
        basePokemonCollection = new BasePokemonCollectionImpl();
        mockRepository.GetCollection<BasePokemonDto>(MongoCollection.BasePokemon).Returns(basePokemonCollection);
        var mockLogger = Substitute.For<ILogger<DexService>>();
        sut = new DexService(mockRepository, Shared.DtoToModelMapper, Shared.ModelToDtoMapper, mockLogger);
        pokemon = JsonConvert.DeserializeObject<Pokemon>("{\r\n    \"speciesName\": \"Ivysaur\",\r\n    \"dexNo\": 2,\r\n    \"form\": \"Base\",\r\n    \"normalPortrait\": \"ivysaur\",\r\n    \"shinyPortrait\": \"ivysaur\",\r\n    \"pokemonStats\": {\r\n      \"hp\": 36,\r\n      \"attack\": 6,\r\n      \"defense\": 6,\r\n      \"specialAttack\": 8,\r\n      \"specialDefense\": 8,\r\n      \"speed\": 6\r\n    },\r\n    \"type\": \"Grass/Poison\",\r\n    \"size\": \"Medium\",\r\n    \"weight\": \"Medium\",\r\n    \"moves\": [\r\n      \"Poison Powder\",\r\n      \"Sleep Powder\",\r\n      \"Razor Leaf\"\r\n    ],\r\n    \"skills\": [\r\n      \"Sprouter\",\r\n      \"Threaded\"\r\n    ],\r\n    \"passives\": [\r\n      \"Growth\",\r\n      \"Growl\",\r\n      \"Overgrow\"\r\n    ],\r\n    \"proficiencies\": [\r\n      \"Grass\",\r\n      \"Poison\",\r\n      \"Floral\",\r\n      \"Vine Whip\"\r\n    ],\r\n    \"eggGroups\": [\r\n      \"Monster\",\r\n      \"Grass\"\r\n    ],\r\n    \"eggHatchRate\": \"10 Days\",\r\n    \"habitats\": [\r\n      \"Forest\",\r\n      \"Jungle\"\r\n    ],\r\n    \"diet\": \"Phototroph\",\r\n    \"rarity\": \"Rare\",\r\n    \"stage\": 2,\r\n    \"specialFormName\": \"\",\r\n    \"baseFormName\": \"\",\r\n    \"gMaxMove\": \"\",\r\n    \"evolvesFrom\": \"Bulbasaur\",\r\n    \"legendaryStats\": {\r\n      \"hp\": 0,\r\n      \"moves\": [],\r\n      \"legendaryMoves\": [],\r\n      \"passives\": [],\r\n      \"features\": []\r\n    }\r\n  }")!;
        pokemon2 = JsonConvert.DeserializeObject<Pokemon>("{\r\n    \"speciesName\": \"Ivysaur1\",\r\n    \"dexNo\": 2,\r\n    \"form\": \"Base\",\r\n    \"normalPortrait\": \"ivysaur\",\r\n    \"shinyPortrait\": \"ivysaur\",\r\n    \"pokemonStats\": {\r\n      \"hp\": 36,\r\n      \"attack\": 6,\r\n      \"defense\": 6,\r\n      \"specialAttack\": 8,\r\n      \"specialDefense\": 8,\r\n      \"speed\": 6\r\n    },\r\n    \"type\": \"Grass/Poison\",\r\n    \"size\": \"Medium\",\r\n    \"weight\": \"Medium\",\r\n    \"moves\": [\r\n      \"Poison Powder\",\r\n      \"Sleep Powder\",\r\n      \"Razor Leaf\"\r\n    ],\r\n    \"skills\": [\r\n      \"Sprouter\",\r\n      \"Threaded\"\r\n    ],\r\n    \"passives\": [\r\n      \"Growth\",\r\n      \"Growl\",\r\n      \"Overgrow\"\r\n    ],\r\n    \"proficiencies\": [\r\n      \"Grass\",\r\n      \"Poison\",\r\n      \"Floral\",\r\n      \"Vine Whip\"\r\n    ],\r\n    \"eggGroups\": [\r\n      \"Monster\",\r\n      \"Grass\"\r\n    ],\r\n    \"eggHatchRate\": \"10 Days\",\r\n    \"habitats\": [\r\n      \"Forest\",\r\n      \"Jungle\"\r\n    ],\r\n    \"diet\": \"Phototroph\",\r\n    \"rarity\": \"Rare\",\r\n    \"stage\": 2,\r\n    \"specialFormName\": \"\",\r\n    \"baseFormName\": \"\",\r\n    \"gMaxMove\": \"\",\r\n    \"evolvesFrom\": \"Bulbasaur\",\r\n    \"legendaryStats\": {\r\n      \"hp\": 0,\r\n      \"moves\": [],\r\n      \"legendaryMoves\": [],\r\n      \"passives\": [],\r\n      \"features\": []\r\n    }\r\n  }")!;
    }

    [Test]
    [TestCase("Venusaur", "Base", 3)]
    public async Task GetDexEntry_Valid_ReturnIndexResponse(string name, string form, int dexNo)
    {
        var response = await sut.GetDexEntry<BasePokemonDto>(DexType.BasePokemon, name);
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Data, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(response.Data.Name, Is.EqualTo(name));
            Assert.That(response.Data.Form, Is.EqualTo(form));
            Assert.That(response.Data.DexNo, Is.EqualTo(dexNo));
        });
    }

    [Test]
    [TestCase("Venusaur1")]
    [TestCase("Venusau")]
    public void GetDexEntry_Invalid_ThrowsItemNotFoundException(string itemName)
    {
        var aggregateException = Assert.Throws<AggregateException>(() =>
        {
            var task = sut.GetDexEntry<BasePokemonDto>(DexType.BasePokemon, itemName);
            task.Wait();
        });

        Assert.That(aggregateException.InnerException, Is.TypeOf<ItemNotFoundException>());
        var exception = aggregateException.InnerException as ItemNotFoundException;
        Assert.Multiple(() =>
        {
            Assert.That(exception!.Title, Is.EqualTo(PtaExceptionParts.ItemNotFoundTitle));
            Assert.That(exception.Message, Is.EqualTo($"Could not find {itemName}"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        });
    }

    [Test]
    public async Task GetDexEntries_Valid_ReturnIndexResponse()
    {
        var pokemon = await sut.GetDexEntries<BasePokemonDto>(DexType.BasePokemon);
        Assert.That(pokemon, Is.Not.Null.Or.Empty);
        Assert.That(pokemon.Count(), Is.EqualTo(5));
    }

    [Test]
    [TestCaseSource(nameof(GetEvolvedMoves))]
    public async Task GetEvolved_Valid_ReturnsPokemon(string[] keptMoves, string[] newMoves)
    {
        var evolved = await sut.GetEvolved(pokemon, keptMoves, "Venusaur", newMoves);
        string[] updatedMoves = [.. keptMoves, .. newMoves];
        Assert.Multiple(() =>
        {
            Assert.That(evolved.SpeciesName, Is.EqualTo("Venusaur"));
            Assert.That(evolved.DexNo, Is.EqualTo(3));
            Assert.That(evolved.Moves.Count(), Is.EqualTo(updatedMoves.Length));
            foreach (var move in updatedMoves)
            {
                Assert.That(evolved.Moves, Contains.Item(move));
            }
        });
    }

    [Test]
    [TestCase("Bulbasaur")]
    public void GetEvolved_InvalidName_Throws(string evolvedName)
    {
        var aggregateException = Assert.Throws<AggregateException>(() =>
        {
            var task = sut.GetEvolved(pokemon, [], evolvedName, []);
            task.Wait();
        });

        Assert.That(aggregateException.InnerException, Is.TypeOf<InvalidEvolutionException>());
        var exception = aggregateException.InnerException as InvalidEvolutionException;
        Assert.Multiple(() =>
        {
            Assert.That(exception!.Title, Is.EqualTo(PtaExceptionParts.EvolutionErrorTitle));
            Assert.That(exception.Message, Is.EqualTo($"{pokemon.SpeciesName} cannot evolve into {evolvedName}"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        });
    }

    [Test]
    [TestCase("Move 1")]
    [TestCase("Move 1", "A second move")]
    public void GetEvolved_InvalidKeptMoves_Throws(params string[] moves)
    {
        var aggregateException = Assert.Throws<AggregateException>(() =>
        {
            var task = sut.GetEvolved(pokemon, moves, "Venusaur", []);
            task.Wait();
        });

        Assert.That(aggregateException.InnerException, Is.TypeOf<InvalidEvolutionException>());
        var exception = aggregateException.InnerException as InvalidEvolutionException;
        Assert.Multiple(() =>
        {
            Assert.That(exception!.Title, Is.EqualTo(PtaExceptionParts.EvolutionErrorTitle));
            Assert.That(exception.Message, Is.EqualTo($"{pokemon.SpeciesName} does not know {string.Join(", ", moves)}"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        });
    }

    [Test]
    [TestCase("Move 1")]
    [TestCase("Move 1", "A second move")]
    public void GetEvolved_InvalidNewMoves_ReturnsPokemon(params string[] moves)
    {
        var aggregateException = Assert.Throws<AggregateException>(() =>
        {
            var task = sut.GetEvolved(pokemon, [], "Venusaur", moves);
            task.Wait();
        });

        Assert.That(aggregateException.InnerException, Is.TypeOf<InvalidEvolutionException>());
        var exception = aggregateException.InnerException as InvalidEvolutionException;
        Assert.Multiple(() =>
        {
            Assert.That(exception!.Title, Is.EqualTo(PtaExceptionParts.EvolutionErrorTitle));
            Assert.That(exception.Message, Is.EqualTo($"Venusaur cannot learn {string.Join(", ", moves)}"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        });
    }

    [Test]
    [TestCase(0,1,1)]
    [TestCase(0, 10, 5)]
    [TestCase(5, 1, 0)]
    [TestCase(5, 10, 0)]
    public async Task GetIndexCollectionResponse_Valid_ReturnResponse(int offset, int limit, int expectedCount)
    {
        var response = await sut.GetIndexCollectionResponse<BasePokemonDto>(DexType.BasePokemon, offset, limit);
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Count, Is.LessThanOrEqualTo(expectedCount));
    }

    [Test]
    public async Task GetOrderedIndexCollectionResponse_ReturnsResponse()
    {
        var response = await sut.GetOrderedIndexCollectionResponse();
        Assert.That(response, Is.Not.Null);
        Assert.That(response, Has.Count.LessThanOrEqualTo(basePokemonCollection.Collection.Count));
    }

    [Test]
    public async Task GetPossibleEvolutions_CanEvolve_NotEmpty()
    {
        var evolutions = await sut.GetPossibleEvolutions(pokemon);
        Assert.That(evolutions.Count(), Is.EqualTo(1));
        var mon = evolutions.First();
        Assert.That(mon.Name, Is.EqualTo("Venusaur"));
    }

    [Test]
    public async Task GetPossibleEvolutions_CantEvolve_Empty()
    {
        var evolutions = await sut.GetPossibleEvolutions(pokemon2);
        Assert.That(evolutions, Is.Empty);
    }

    [Test]
    [TestCase("Bulbasaur", "Base", "NewGuy")]
    [TestCase("Ivysaur", "Base", "NewGuy")]
    [TestCase("Venusaur", "Base", "NewGuy")]
    [TestCase("Venusaur", "Gigantamax", "NewGuy")]
    [TestCase("Venusaur", "Mega", "NewGuy")]
    [TestCase("Venusaur", "Base", null)]
    [TestCase("Venusaur", "Gigantamax", null)]
    [TestCase("Venusaur", "Mega", null)]
    public async Task GetNewPokemon_Valid_ReturnNewPokemon(string name, string form, string? nickname)
    {
        var newPokemon = await sut.GetNewPokemon(name, nickname, form);
        Assert.Multiple(() =>
        {
            Assert.That(newPokemon.SpeciesName, Is.EqualTo(name));
            Assert.That(newPokemon.Nickname, Is.EqualTo(nickname ?? name));
            Assert.That(newPokemon.Form, Is.EqualTo(form));
            Assert.That(newPokemon.AlternateForms, Does.Not.Contain(form));
        });
    }

    [Test]
    [TestCase("Venusaur1", "Base", "NewGuy")]
    [TestCase("Venusaur2", "Gigantamax", "NewGuy")]
    [TestCase("Venusaur3", "Mega", "NewGuy")]
    public void GetNewPokemon_InvalidName_ReturnNewPokemon(string name, string form, string nickname)
    {
        var aggregateException = Assert.Throws<AggregateException>(() =>
        {
            var task = sut.GetNewPokemon(name, nickname, form);
            task.Wait();
        });

        Assert.That(aggregateException.InnerException, Is.TypeOf<ItemNotFoundException>());
        var exception = aggregateException.InnerException as ItemNotFoundException;
        Assert.Multiple(() =>
        {
            Assert.That(exception!.Title, Is.EqualTo(PtaExceptionParts.ItemNotFoundTitle));
            Assert.That(exception.Message, Is.EqualTo($"Could not find {name}"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        });
    }

    [Test]
    [TestCase("Venusaur", "Base1", "NewGuy")]
    [TestCase("Venusaur", "Gigantamax1", "NewGuy")]
    [TestCase("Venusaur", "Mega1", "NewGuy")]
    public void GetNewPokemon_InvalidForm_ReturnNewPokemon(string name, string form, string nickname)
    {
        var aggregateException = Assert.Throws<AggregateException>(() =>
        {
            var task = sut.GetNewPokemon(name, nickname, form);
            task.Wait();
        });

        Assert.That(aggregateException.InnerException, Is.TypeOf<ItemNotFoundException>());
        var exception = aggregateException.InnerException as ItemNotFoundException;
        Assert.Multiple(() =>
        {
            Assert.That(exception!.Title, Is.EqualTo(PtaExceptionParts.ItemNotFoundTitle));
            Assert.That(exception.Message, Is.EqualTo($"Could not find {form}"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        });
    }

    [Test]
    [TestCase("Venusaur", "Base", "NewGuy", Nature.Sassy, Gender.Male, Status.Normal)]
    [TestCase("Venusaur", "Mega", "NewGuy", Nature.Timid, Gender.Genderless, Status.Burned)]
    public async Task GetNewPokemon_Valid_ReturnNewPokemon(string name, string form, string nickname, Nature nature, Gender gender, Status status)
    {
        var newPokemon = await sut.GetNewPokemon(name, nature, gender, status, nickname, form);
        Assert.Multiple(() =>
        {
            Assert.That(newPokemon.SpeciesName, Is.EqualTo(name));
            Assert.That(newPokemon.Nickname, Is.EqualTo(nickname));
            Assert.That(newPokemon.Form, Is.EqualTo(form));
            Assert.That(newPokemon.AlternateForms, Does.Not.Contain(form));
            Assert.That(newPokemon.Nature, Is.EqualTo(nature));
            Assert.That(newPokemon.Gender, Is.EqualTo(gender));
            Assert.That(newPokemon.PokemonStatus, Is.EqualTo(status));
        });
    }

    [Test]
    [TestCase("Venusaur", "Base", "Gigantamax", "Mega")]
    [TestCase("Venusaur", "Gigantamax", "Base", "Mega")]
    [TestCase("Venusaur", "Mega", "Base", "Gigantamax")]
    public async Task GetPokedexEntry_Valid_ReturnPokemonForm(string name, string selectedForm, string altForm1, string altForm2)
    {
        var response = await sut.GetPokedexEntry(name, selectedForm);
        Assert.That(response, Is.Not.Null);
        Assert.That(response.Pokemon, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(response.Pokemon.Name, Is.EqualTo(name));
            Assert.That(response.Pokemon.Form, Is.EqualTo(selectedForm));
            Assert.That(response.Pokemon.DexNo, Is.EqualTo(3));
            Assert.That(response.AlternateForms, Has.Count.EqualTo(2));
        });
        Assert.That(response.AlternateForms, Contains.Item(altForm1));
        Assert.That(response.AlternateForms, Contains.Item(altForm2));
    }

    [Test]
    [TestCase("Venusaur1", "Base")]
    [TestCase("Venusaur2", "Gigantamax")]
    [TestCase("Venusaur3", "Mega")]
    public void GetPokedexEntry_InvalidName_ReturnPokemonForm(string name, string selectedForm)
    {
        var aggregateException = Assert.Throws<AggregateException>(() =>
        {
            var task = sut.GetPokedexEntry(name, selectedForm);
            task.Wait();
        });

        Assert.That(aggregateException.InnerException, Is.TypeOf<ItemNotFoundException>());
        var exception = aggregateException.InnerException as ItemNotFoundException;
        Assert.Multiple(() =>
        {
            Assert.That(exception!.Title, Is.EqualTo(PtaExceptionParts.ItemNotFoundTitle));
            Assert.That(exception.Message, Is.EqualTo($"Could not find {name}"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        });
    }

    [Test]
    [TestCase("Venusaur", "Base1")]
    [TestCase("Venusaur", "Gigantamax1")]
    [TestCase("Venusaur", "Mega1")]
    public void GetPokedexEntry_InvalidForm_ReturnPokemonForm(string name, string selectedForm)
    {
        var aggregateException = Assert.Throws<AggregateException>(() =>
        {
            var task = sut.GetPokedexEntry(name, selectedForm);
            task.Wait();
        });

        Assert.That(aggregateException.InnerException, Is.TypeOf<ItemNotFoundException>());
        var exception = aggregateException.InnerException as ItemNotFoundException;
        Assert.Multiple(() =>
        {
            Assert.That(exception!.Title, Is.EqualTo(PtaExceptionParts.ItemNotFoundTitle));
            Assert.That(exception.Message, Is.EqualTo($"Could not find {selectedForm}"));
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        });
    }

    [Test]
    public async Task PostPokedexEntries_New_AddToCollection()
    {
        var oldCount = basePokemonCollection.Collection.Count;
        var test = new PokemonForm
        {
            DexNo = 4,
            Form = "Base",
            Name = "Charmander"
        };

        await sut.PostPokedexEntries([test]);
        await GetDexEntry_Valid_ReturnIndexResponse(test.Name, test.Form, test.DexNo);
        var newCount = basePokemonCollection.Collection.Count;
        Assert.That(newCount, Is.EqualTo(oldCount + 1));
    }

    [Test]
    public async Task PostPokedexEntries_Old_NochangeToCollection()
    {
        var oldCount = basePokemonCollection.Collection.Count;
        var test = new PokemonForm
        {
            DexNo = 2,
            Form = "Base",
            Name = "Venusaur"
        };

        await sut.PostPokedexEntries([test]);
        var newCount = basePokemonCollection.Collection.Count;
        Assert.That(newCount, Is.EqualTo(oldCount));
    }

    private static string[][][] GetEvolvedMoves()
    {
        return
        [
            [[], []],
            [["Poison Powder"], []],
            [[], ["Move 3"]],
            [["Poison Powder"], ["Move 3"]],
        ];
    }
}
