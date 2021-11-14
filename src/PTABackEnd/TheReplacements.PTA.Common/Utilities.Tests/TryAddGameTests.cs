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
