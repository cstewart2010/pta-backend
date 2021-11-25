using TheReplacement.PTA.Common.Enums;
using Xunit;
using Xunit.Abstractions;

namespace TheReplacement.PTA.Common.Tests.Utilities
{
    public class TryAddPokemonTests : TestsBase
    {
        private readonly ITestOutputHelper _logger;
        public override ITestOutputHelper Logger { get => _logger; }

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
        public void TryAddPokemon_Stats0_False()
        {
            var pokemon = GetTestPokemon();
            pokemon.PokemonStats.HP = 0;
            pokemon.PokemonStats.Attack = 0;
            pokemon.PokemonStats.Defense = 0;
            pokemon.PokemonStats.SpecialAttack = 0;
            pokemon.PokemonStats.SpecialDefense = 0;
            pokemon.PokemonStats.Speed = 0;
            PerformTryAddPokemonFailTest(pokemon, Logger);
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

        [Theory]
        [InlineData(Gender.Genderless)]
        [InlineData(Gender.Male)]
        [InlineData(Gender.Female)]
        public void TryAddPokemon_GenderInRanges_True(Gender gender)
        {
            var pokemon = GetTestPokemon();
            pokemon.Gender = gender.ToString();
            PerformTryAddPokemonPassTest(pokemon, Logger);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(3)]
        public void TryAddPokemon_GenderOutOfRanges_False(int gender)
        {
            var pokemon = GetTestPokemon();
            pokemon.Gender = ((Gender)gender).ToString();
            PerformTryAddPokemonFailTest(pokemon, Logger);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(6)]
        public void TryAddPokemon_MoveCountInRanges_True(int count)
        {
            var moves = new string[count];
            for (int i = 0; i < count; i++)
            {
                moves[i] = $"Move{i}";
            }

            var pokemon = GetTestPokemon();
            pokemon.Moves = moves;
            PerformTryAddPokemonPassTest(pokemon, Logger);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(7)]
        public void TryAddPokemon_MoveCountOutOfRanges_False(int count)
        {
            var moves = new string[count];
            for (int i = 0; i < count; i++)
            {
                moves[i] = $"Move{i}";
            }

            var pokemon = GetTestPokemon();
            pokemon.Moves = moves;
            PerformTryAddPokemonFailTest(pokemon, Logger);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(20)]
        public void TryAddPokemon_NatureInRanges_True(int nature)
        {
            var pokemon = GetTestPokemon();
            pokemon.Nature = ((Nature)nature).ToString();
            PerformTryAddPokemonPassTest(pokemon, Logger);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(36)]
        public void TryAddPokemon_NatureOutOfRanges_False(int nature)
        {
            var pokemon = GetTestPokemon();
            pokemon.Nature = ((Nature)nature).ToString();
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
