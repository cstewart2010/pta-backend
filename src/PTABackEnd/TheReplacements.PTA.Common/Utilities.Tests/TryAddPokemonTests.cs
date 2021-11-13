using TheReplacements.PTA.Common.Enums;
using Xunit;
using Xunit.Abstractions;

namespace TheReplacements.PTA.Common.Utilities.Tests
{
    public class TryAddPokemonTests : TestsBase
    {
        public TryAddPokemonTests(ITestOutputHelper output)
        {
            Logger = output;
        }

        [Fact, Trait("Category", "smoke")]
        public void TryAddPokemon_SmokeTest_True()
        {
            PerformTryAddPokemonPassTest(TestPokemon);
        }

        [Fact]
        public void TryAddPokemon_StatsNull_False()
        {
            var pokemon = TestPokemon;
            pokemon.HP = null;
            PerformTryAddPokemonFailTest(pokemon);
            pokemon.Attack = null;
            PerformTryAddPokemonFailTest(pokemon);
            pokemon.Defense = null;
            PerformTryAddPokemonFailTest(pokemon);
            pokemon.SpecialAttack = null;
            PerformTryAddPokemonFailTest(pokemon);
            pokemon.SpecialDefense = null;
            PerformTryAddPokemonFailTest(pokemon);
            pokemon.Speed = null;
            PerformTryAddPokemonFailTest(pokemon);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(255)]
        public void TryAddPokemon_CatchRateInRanges_True(int catchRate)
        {
            var pokemon = TestPokemon;
            pokemon.CatchRate = catchRate;
            PerformTryAddPokemonPassTest(pokemon);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(256)]
        public void TryAddPokemon_CatchRateOutOfRanges_False(int catchRate)
        {
            var pokemon = TestPokemon;
            pokemon.CatchRate = catchRate;
            PerformTryAddPokemonFailTest(pokemon);
        }

        [Fact]
        public void TryAddPokemon_DexNoOutOfRange_False()
        {
            var pokemon = TestPokemon;
            pokemon.DexNo = 0;
            PerformTryAddPokemonFailTest(pokemon);
        }

        [Fact]
        public void TryAddPokemon_ExpYieldOutOfRange_False()
        {
            var pokemon = TestPokemon;
            pokemon.ExpYield = 0;
            PerformTryAddPokemonFailTest(pokemon);
        }

        [Theory]
        [InlineData(Gender.Genderless)]
        [InlineData(Gender.Male)]
        [InlineData(Gender.Female)]
        public void TryAddPokemon_GenderInRanges_True(Gender gender)
        {
            var pokemon = TestPokemon;
            pokemon.Gender = gender;
            PerformTryAddPokemonPassTest(pokemon);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(3)]
        public void TryAddPokemon_GenderOutOfRanges_False(int gender)
        {
            var pokemon = TestPokemon;
            pokemon.Gender = (Gender)gender;
            PerformTryAddPokemonFailTest(pokemon);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(50)]
        [InlineData(100)]
        public void TryAddPokemon_LevelInRanges_True(int level)
        {
            var pokemon = TestPokemon;
            pokemon.Level = level;
            PerformTryAddPokemonPassTest(pokemon);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(101)]
        public void TryAddPokemon_LevelOutOfRanges_False(int level)
        {
            var pokemon = TestPokemon;
            pokemon.Level = level;
            PerformTryAddPokemonFailTest(pokemon);
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

            var pokemon = TestPokemon;
            pokemon.NaturalMoves = moves;
            PerformTryAddPokemonPassTest(pokemon);
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

            var pokemon = TestPokemon;
            pokemon.NaturalMoves = moves;
            PerformTryAddPokemonFailTest(pokemon);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(18)]
        [InlineData(35)]
        public void TryAddPokemon_NatureInRanges_True(int nature)
        {
            var pokemon = TestPokemon;
            pokemon.Nature = nature;
            PerformTryAddPokemonPassTest(pokemon);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(36)]
        public void TryAddPokemon_NatureOutOfRanges_False(int nature)
        {
            var pokemon = TestPokemon;
            pokemon.Nature = nature;
            PerformTryAddPokemonFailTest(pokemon);
        }

        [Fact]
        public void TryAddPokemon_NicknameNull_False()
        {
            var pokemon = TestPokemon;
            pokemon.Nickname = null;
            PerformTryAddPokemonFailTest(pokemon);
        }

        [Theory]
        [InlineData("")]
        [InlineData("aaaaaaaaaaaaaaaaaaa")]
        public void TryAddPokemon_NicknameEmptyOrTooLong_False(string nickname)
        {
            var pokemon = TestPokemon;
            pokemon.Nickname = nickname;
            PerformTryAddPokemonFailTest(pokemon);
        }
    }
}
