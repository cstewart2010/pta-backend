using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace TheReplacements.PTA.Common.Utilities.Tests
{
    public class OtherTests : TestsBase
    {
        public OtherTests(ITestOutputHelper output)
        {
            Logger = output;
        }

        [Theory, Trait("Category", "smoke")]
        [InlineData(true)]
        [InlineData(false)]
        public void HasGM_SmokeTest_HasGM(bool hasGM)
        {
            Logger.WriteLine($"Adding a new game");
            var game = GetTestGame();
            DatabaseUtility.TryAddGame(game, out _);
            if (hasGM)
            {
                Logger.WriteLine($"Adding a gm for newly created game");
                var trainer = GetTestTrainer();
                trainer.GameId = game.GameId;
                trainer.IsGM = true;
                DatabaseUtility.TryAddTrainer(trainer, out _);
            }

            Logger.WriteLine($"Verifying whether the game has a GM");
            Assert.Equal(hasGM, DatabaseUtility.HasGM(game.GameId));
            DatabaseUtility.DeleteGame(game.GameId);
            if (hasGM)
            {
                DatabaseUtility.DeleteTrainersByGameId(game.GameId);
            }
        }
    }
}
