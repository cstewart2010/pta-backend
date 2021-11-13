using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheReplacements.PTA.Common.Models;
using Xunit;
using Xunit.Abstractions;

namespace TheReplacements.PTA.Common.Utilities.Tests
{
    public class DatabaseUtilityTests
    {
        private static ITestOutputHelper Logger { get; set; }

        public DatabaseUtilityTests(ITestOutputHelper output)
        {
            Logger = output;
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TryAddGame_StandardGame_True(bool isOnline)
        {
            var game = new GameModel
            {
                GameId = Guid.NewGuid().ToString(),
                Nickname = "Test Nickname",
                NPCs = Array.Empty<string>(),
                PasswordHash = "testpassword",
                IsOnline = isOnline
            };

            Logger.WriteLine($"Adding new game with game id {game.GameId}");
            Assert.True(DatabaseUtility.TryAddGame(game, out var error));
            Logger.WriteLine($"Verify that error object is null");
            Assert.Null(error);
            Logger.WriteLine($"Removing game with game id {game.GameId}");
            DatabaseUtility.DeleteGame(game.GameId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
        [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
        public void TryAddGame_GameIdInvalid_False(string gameId)
        {
            var game = new GameModel
            {
                GameId = gameId,
                Nickname = "Test Nickname",
                NPCs = Array.Empty<string>(),
                PasswordHash = "testpassword"
            };

            PerformTryAddGameTest(game);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("aaaaaaaaaaaaaaaaaaaaa")]
        public void TryAddGame_NicknameInvalid_False(string nickname)
        {
            var game = new GameModel
            {
                GameId = Guid.NewGuid().ToString(),
                Nickname = nickname,
                NPCs = Array.Empty<string>(),
                PasswordHash = "testpassword"
            };

            PerformTryAddGameTest(game);
        }

        [Fact]
        public void TryAddNewGame_NPCsInvalid_False()
        {
            var game = new GameModel
            {
                GameId = Guid.NewGuid().ToString(),
                Nickname = "Test Nickname",
                PasswordHash = "testpassword"
            };

            PerformTryAddGameTest(game);
        }

        [Fact]
        public void TryAddNewGame_PasswordInvalid_False()
        {
            var game = new GameModel
            {
                GameId = Guid.NewGuid().ToString(),
                Nickname = "Test Nickname",
                NPCs = Array.Empty<string>()
            };

            PerformTryAddGameTest(game);
        }

        private static void PerformTryAddGameTest(GameModel game)
        {
            Logger.WriteLine($"Adding new game with game id {game.GameId}");
            Assert.False(DatabaseUtility.TryAddGame(game, out var error));
            Logger.WriteLine($"Verify that error object is null");
            Assert.NotNull(error);
            Logger.WriteLine($"Verify no game is found with gameId {game.GameId}");
            Assert.Null(DatabaseUtility.FindGame(game.GameId));
        }
    }
}
