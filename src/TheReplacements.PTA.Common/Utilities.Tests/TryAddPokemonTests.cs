using TheReplacements.PTA.Common.Enums;
using Xunit;
using Xunit.Abstractions;

namespace TheReplacements.PTA.Common.Utilities.Tests
{
    public class TryAddPokemonTests : TestsBase
    {
        private readonly ITestOutputHelper _logger;
        protected override ITestOutputHelper Logger { get => _logger; }

        public TryAddPokemonTests(ITestOutputHelper logger)
        {
            _logger = logger;
        }

        [Fact, Trait("Category", "smoke")]
        public void TryAddPokemon_SmokeTest_True()
        {
            PerformTryAddPokemonPassTest(GetTestPokemon(), Logger);
        }

        [Fact]
        public void TryAddPokemon_StatsNull_False()
        {
            var pokemon = GetTestPokemon();
            pokemon.HP = null;
            PerformTryAddPokemonFailTest(pokemon, Logger);
            pokemon.Attack = null;
            PerformTryAddPokemonFailTest(pokemon, Logger);
            pokemon.Defense = null;
            PerformTryAddPokemonFailTest(pokemon, Logger);
            pokemon.SpecialAttack = null;
            PerformTryAddPokemonFailTest(pokemon, Logger);
            pokemon.SpecialDefense = null;
            PerformTryAddPokemonFailTest(pokemon, Logger);
            pokemon.Speed = null;
            PerformTryAddPokemonFailTest(pokemon, Logger);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(255)]
        public void TryAddPokemon_CatchRateInRanges_True(int catchRate)
        {
            var pokemon = GetTestPokemon();
            pokemon.CatchRate = catchRate;
            PerformTryAddPokemonPassTest(pokemon, Logger);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(256)]
        public void TryAddPokemon_CatchRateOutOfRanges_False(int catchRate)
        {
            var pokemon = GetTestPokemon();
            pokemon.CatchRate = catchRate;
            PerformTryAddPokemonFailTest(pokemon, Logger);
        }

        [Fact]
        public void TryAddPokemon_DexNoOutOfRange_False()
        {
            var pokemon = GetTestPokemon();
            pokemon.DexNo = 0;
            PerformTryAddPokemonFailTest(pokemon, Logger);
        }

        [Fact]
        public void TryAddPokemon_ExpYieldOutOfRange_False()
        {
            var pokemon = GetTestPokemon();
            pokemon.ExpYield = 0;
            PerformTryAddPokemonFailTest(pokemon, Logger);
        }

        [Theory]
        [InlineData(Gender.Genderless)]
        [InlineData(Gender.Male)]
        [InlineData(Gender.Female)]
        public void TryAddPokemon_GenderInRanges_True(Gender gender)
        {
            var pokemon = GetTestPokemon();
            pokemon.Gender = gender;
            PerformTryAddPokemonPassTest(pokemon, Logger);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(3)]
        public void TryAddPokemon_GenderOutOfRanges_False(int gender)
        {
            var pokemon = GetTestPokemon();
            pokemon.Gender = (Gender)gender;
            PerformTryAddPokemonFailTest(pokemon, Logger);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(50)]
        [InlineData(100)]
        public void TryAddPokemon_LevelInRanges_True(int level)
        {
            var pokemon = GetTestPokemon();
            pokemon.Level = level;
            PerformTryAddPokemonPassTest(pokemon, Logger);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(101)]
        public void TryAddPokemon_LevelOutOfRanges_False(int level)
        {
            var pokemon = GetTestPokemon();
            pokemon.Level = level;
            PerformTryAddPokemonFailTest(pokemon, Logger);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void TryAddPokemon_MoveCountInRanges_True(int count)
        {
            var moves = new string[count];
            for (int i = 0; i < count; i++)
            {
                moves[i] = $"Move{i}";
            }

            var pokemon = GetTestPokemon();
            pokemon.NaturalMoves = moves;
            PerformTryAddPokemonPassTest(pokemon, Logger);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(5)]
        public void TryAddPokemon_MoveCountOutOfRanges_False(int count)
        {
            var moves = new string[count];
            for (int i = 0; i < count; i++)
            {
                moves[i] = $"Move{i}";
            }

            var pokemon = GetTestPokemon();
            pokemon.NaturalMoves = moves;
            PerformTryAddPokemonFailTest(pokemon, Logger);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(18)]
        [InlineData(35)]
        public void TryAddPokemon_NatureInRanges_True(int nature)
        {
            var pokemon = GetTestPokemon();
            pokemon.Nature = nature;
            PerformTryAddPokemonPassTest(pokemon, Logger);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(36)]
        public void TryAddPokemon_NatureOutOfRanges_False(int nature)
        {
            var pokemon = GetTestPokemon();
            pokemon.Nature = nature;
            PerformTryAddPokemonFailTest(pokemon, Logger);
        }

        [Fact]
        public void TryAddPokemon_NicknameNull_False()
        {
            var pokemon = GetTestPokemon();
            pokemon.Nickname = null;
            PerformTryAddPokemonFailTest(pokemon, Logger);
        }

        [Theory]
        [InlineData("")]
        [InlineData("aaaaaaaaaaaaaaaaaaa")]
        public void TryAddPokemon_NicknameEmptyOrTooLong_False(string nickname)
        {
            var pokemon = GetTestPokemon();
            pokemon.Nickname = nickname;
            PerformTryAddPokemonFailTest(pokemon, Logger);
        }
    }
}
