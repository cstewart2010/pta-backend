using TheReplacement.PTA.Common.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace TheReplacement.PTA.Common.Tests.Utilities
{
    public class OtherTests : TestsBase
    {
        private readonly ITestOutputHelper _logger;
        public override ITestOutputHelper Logger { get => _logger; }

        public OtherTests(ITestOutputHelper logger)
        {
            _logger = logger;
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
            Assert.Equal(hasGM, DatabaseUtility.HasGM(game.GameId, out _));
            DatabaseUtility.DeleteGame(game.GameId);
            if (hasGM)
            {
                DatabaseUtility.DeleteTrainersByGameId(game.GameId);
            }
        }
    }
}
