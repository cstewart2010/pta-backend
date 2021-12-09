using Xunit;
using Xunit.Abstractions;

namespace TheReplacement.PTA.Common.Tests.Utilities
{
    public class TryAddGameTests : TestsBase
    {
        private readonly ITestOutputHelper _logger;
        public override ITestOutputHelper Logger { get => _logger; }

        public TryAddGameTests(ITestOutputHelper logger)
        {
            _logger = logger;
        }

        [Fact, Trait("Category", "smoke")]
        public void TryAddGame_StandardGame_True()
        {
            PerformTryAddGamePassTest(GetTestGame(), Logger);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("aaaaaaaaaaaaaaaaaaaaa")]
        public void TryAddGame_NicknameInvalid_False(string nickname)
        {
            var game = GetTestGame();
            game.Nickname = nickname;
            PerformTryAddGameFailTest(game, Logger);
        }

        [Fact]
        public void TryAddNewGame_NPCsInvalid_False()
        {
            var game = GetTestGame();
            game.NPCs = null;
            PerformTryAddGameFailTest(game, Logger);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void TryAddNewGame_PasswordInvalid_False(string password)
        {
            var game = GetTestGame();
            game.PasswordHash = password;
            PerformTryAddGameFailTest(game, Logger);
        }
    }
}
