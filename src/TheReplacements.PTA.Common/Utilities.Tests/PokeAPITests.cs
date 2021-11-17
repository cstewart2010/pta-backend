using System;
using System.Net;
using TheReplacements.PTA.Common.Enums;
using Xunit;
using Xunit.Abstractions;

namespace TheReplacements.PTA.Common.Utilities.Tests
{
    public class PokeAPITests : TestsBase
    {
        private readonly ITestOutputHelper _logger;
        public override ITestOutputHelper Logger { get => _logger; }

        public PokeAPITests(ITestOutputHelper logger)
        {
            _logger = logger;
        }

        [Fact, Trait("Category", "smoke")]
        public void GetEvolved_SmokeTest_NotNull()
        {
            var flabebe = PokeAPIUtility.GetPokemon("Flabebe", "Modest");

            Logger.WriteLine("Evolving flabebe to floette");
            var floette = PokeAPIUtility.GetEvolved(flabebe, "Floette");
            Assert.NotNull(floette);
            Assert.Equal(670, floette.DexNo);

            Logger.WriteLine("Evolving floette to florges");
            var florges = PokeAPIUtility.GetEvolved(floette, "Florges");
            Assert.NotNull(florges);
            Assert.Equal(671, florges.DexNo);
        }

        [Fact]
        public void GetEvolved_DistantEvolution_Null()
        {
            var flabebe = PokeAPIUtility.GetPokemon("Flabebe", "Modest");

            Logger.WriteLine("Evolving flabebe to florges");
            Assert.Null(PokeAPIUtility.GetEvolved(flabebe, "Florges"));
        }

        [Fact]
        public void GetEvolved_NullArguments_ThrowsArgumentNullException()
        {
            var flabebe = PokeAPIUtility.GetPokemon("Flabebe", "Modest");

            Logger.WriteLine("Evolving flabebe to a null reference");
            Assert.Throws<ArgumentNullException>(() => PokeAPIUtility.GetEvolved(flabebe, null));

            Logger.WriteLine("Evolving a null reference to floette");
            Assert.Throws<ArgumentNullException>(() => PokeAPIUtility.GetEvolved(null, "floette"));
        }

        [Theory, Trait("Category", "smoke")]
        [InlineData("flabebe", 669, "Modest")]
        [InlineData("vivillon", 666, "Timid")]
        [InlineData("snorlax", 143, "Adamant")]
        public void GetPokemon_SmokeTest_NotNull(string pokemonName, int dexNo, string nature)
        {
            var pokemon = PokeAPIUtility.GetPokemon(pokemonName, nature);
            Assert.Equal(pokemonName.ToUpper(), pokemon.Nickname);
            Assert.Equal(dexNo, pokemon.DexNo);
            Assert.Equal(nature, ((Nature)pokemon.Nature).ToString());
        }

        [Fact]
        public void GetPokemon_InvalidPokemon_ThrowsWebException()
        {
            Logger.WriteLine($"Creating a gorilla with a jolly");
            Assert.Throws<WebException>(() => PokeAPIUtility.GetPokemon("gorilla", "jolly"));
        }

        [Fact]
        public void GetPokemon_InvalidNature_Null()
        {
            Logger.WriteLine($"Creating a flabebe with a gorilla");
            Assert.Null(PokeAPIUtility.GetPokemon("flabebe", "gorilla"));
        }

        [Theory]
        [InlineData("flabebe", null)]
        [InlineData(null, "modest")]
        public void GetPokemon_NullArguments_ThrowsArgumentNullException(string pokemon, string nature)
        {
            Logger.WriteLine($"Creating a {pokemon} with a {nature}");
            Assert.Throws<ArgumentNullException>(() => PokeAPIUtility.GetPokemon(pokemon, nature));
        }
    }
}
