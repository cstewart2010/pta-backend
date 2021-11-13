using System;
using TheReplacements.PTA.Common.Models;
using Xunit;
using Xunit.Abstractions;

namespace TheReplacements.PTA.Common.Utilities.Tests
{
    public class TryAddGameTests : TestsBase
    {
        public TryAddGameTests(ITestOutputHelper output)
        {
            Logger = output;
        }

        [Fact, Trait("Category", "smoke")]
        public void TryAddGame_StandardGame_True()
        {
            PerformTryAddGamePassTest(TestGame);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("aaaaaaaaaaaaaaaaaaaaa")]
        public void TryAddGame_NicknameInvalid_False(string nickname)
        {
            var game = TestGame;
            game.Nickname = nickname;
            PerformTryAddGameFailTest(game);
        }

        [Fact]
        public void TryAddNewGame_NPCsInvalid_False()
        {
            var game = TestGame;
            game.NPCs = null;
            PerformTryAddGameFailTest(game);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void TryAddNewGame_PasswordInvalid_False(string password)
        {
            var game = TestGame;
            game.PasswordHash = password;
            PerformTryAddGameFailTest(game);
        }
    }
}
