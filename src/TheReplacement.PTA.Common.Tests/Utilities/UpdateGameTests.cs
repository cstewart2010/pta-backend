using MongoDB.Driver;
using System;
using System.Collections.Generic;
using TheReplacement.PTA.Common.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace TheReplacement.PTA.Common.Tests.Utilities
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
            var npcIds = new List<Guid>();
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
    }
}
