using MongoDB.Driver;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace TheReplacements.PTA.Common.Utilities.Tests
{
    public class UpdateGameTests : TestsBase
    {
        private readonly ITestOutputHelper _logger;
        public override ITestOutputHelper Logger { get => _logger; }

        public UpdateGameTests(ITestOutputHelper logger)
        {
            _logger = logger;
        }

        [Theory, Trait("Category", "smoke")]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        public void UpdateGameNpcList_SmokeTest_True(int npcCount)
        {
            var game = GetTestGame();
            Logger.WriteLine($"Adding game id {game.GameId}");
            DatabaseUtility.TryAddGame(game, out _);

            Logger.WriteLine($"Adding {npcCount} npcs to game id {game.GameId}");
            var npcIds = new List<string>();
            for (int i = 0; i < npcCount; i++)
            {
                npcIds.Add(GetTestNpc().NPCId);
            }
            Assert.True(DatabaseUtility.UpdateGameNpcList(game.GameId, npcIds));

            Logger.WriteLine($"Verifying game npc list has been changed to {string.Join(",", npcIds)}");
            game = DatabaseUtility.FindGame(game.GameId);
            DatabaseUtility.DeleteGame(game.GameId);
            Assert.Equal(npcIds, game.NPCs);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("A game that doesn't exist")]
        public void UpdateGameNpcList_InvalidGameId_False(string gameId)
        {
            var game = GetTestGame();
            var npcCount = 0;
            Logger.WriteLine($"Adding game id {game.GameId}");
            DatabaseUtility.TryAddGame(game, out _);

            Logger.WriteLine($"Adding {npcCount} npcs to game id {gameId}");
            var npcIds = new List<string>();
            for (int i = 0; i < npcCount; i++)
            {
                npcIds.Add(GetTestNpc().NPCId);
            }
            Assert.False(DatabaseUtility.UpdateGameNpcList(gameId, npcIds));

            Logger.WriteLine($"Verifying game id {gameId} was not found and updated");
            Assert.Null(DatabaseUtility.FindGame(gameId));
            DatabaseUtility.DeleteGame(game.GameId);
        }

        [Fact]
        public void UpdateGameNpcList_NullIds_False()
        {
            var game = GetTestGame();
            Logger.WriteLine($"Adding game id {game.GameId}");
            DatabaseUtility.TryAddGame(game, out _);

            Logger.WriteLine($"Adding a null reference npcs to game id {game.GameId}");
            Assert.Throws<MongoCommandException>(() => DatabaseUtility.UpdateGameNpcList(game.GameId, null));

            Logger.WriteLine($"Verifying game npc list was not changed");
            game = DatabaseUtility.FindGame(game.GameId);
            DatabaseUtility.DeleteGame(game.GameId);
            Assert.NotNull(game.NPCs);
        }

        [Theory, Trait("Category", "smoke")]
        [InlineData(true)]
        [InlineData(false)]
        public void UpdateGameOnlineStatus_SmokeTest_True(bool isOnline)
        {
            var game = GetTestGame();
            Logger.WriteLine($"Adding game id {game.GameId}");
            DatabaseUtility.TryAddGame(game, out _);

            Logger.WriteLine($"Updating game online status to {isOnline}");
            Assert.True(DatabaseUtility.UpdateGameOnlineStatus(game.GameId, isOnline));

            Logger.WriteLine($"Verifying game online status has been changed to {isOnline}");
            game = DatabaseUtility.FindGame(game.GameId);
            DatabaseUtility.DeleteGame(game.GameId);
            Assert.Equal(isOnline, game.IsOnline);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("A game that doesn't exist")]
        public void UpdateGameOnlineStatus_InvalidGameid_False(string gameId)
        {
            var game = GetTestGame();
            Logger.WriteLine($"Adding game id {game.GameId}");
            DatabaseUtility.TryAddGame(game, out _);

            Logger.WriteLine($"Updating game id {gameId} online status to true");
            Assert.False(DatabaseUtility.UpdateGameOnlineStatus(gameId, true));

            Logger.WriteLine($"Verifying game online status has not been changed to true");
            DatabaseUtility.DeleteGame(game.GameId);
            game = DatabaseUtility.FindGame(gameId);
            Assert.Null(game);
        }
    }
}
