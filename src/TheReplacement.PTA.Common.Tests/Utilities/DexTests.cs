using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheReplacement.PTA.Common.Enums;
using TheReplacement.PTA.Common.Models;
using TheReplacement.PTA.Common.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace TheReplacement.PTA.Common.Tests.Utilities
{
    public class DexTests : TestsBase
    {
        private readonly ITestOutputHelper _logger;
        public override ITestOutputHelper Logger => _logger;

        public DexTests(ITestOutputHelper logger)
        {
            _logger = logger;
        }

        [Fact, Trait("Category", "smoke")]
        public void GetEvolved_SmokeTest_NotNull()
        {
            var flabebe = GetTestPokemon();
            Logger.WriteLine($"Generating pokemon {flabebe.SpeciesName}");
            Logger.WriteLine($"Attempting to evolve {flabebe.SpeciesName}");
            var floette = DexUtility.GetEvolved(flabebe, flabebe.Moves, "Floette", Array.Empty<string>());
            Logger.WriteLine($"Verifying the evolution was a success");
            Assert.NotNull(floette);
            Logger.WriteLine($"Verifying the evolved form is accurate");
            Assert.Equal(flabebe.SpeciesName, floette.EvolvedFrom, true);
            Assert.Equal(flabebe.Moves, floette.Moves);
        }

        [Fact]
        public void GetEvolved_UpdateMoves_NotNull()
        {
            var flabebe = GetTestPokemon();
            Logger.WriteLine($"Generating pokemon {flabebe.SpeciesName}");
            Logger.WriteLine($"Attempting to evolve {flabebe.SpeciesName}");
            var baseFloette = DexUtility.GetDexEntry<BasePokemonModel>(DexType.BasePokemon, "floette");
            var floette = DexUtility.GetEvolved(flabebe, flabebe.Moves, "Floette", baseFloette.Moves);
            Logger.WriteLine($"Verifying the evolution was a success");
            Assert.NotNull(floette);
            Logger.WriteLine($"Verifying the evolved form is accurate");
            Assert.Equal(flabebe.SpeciesName, floette.EvolvedFrom, true);
            Assert.Equal(flabebe.Moves.Union(baseFloette.Moves), floette.Moves);
        }
    }
}
