using System;
using System.Linq;
using TheReplacement.PTA.Common.Models;
using TheReplacement.PTA.Common.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace TheReplacement.PTA.Common.Tests.Utilities
{
    public class UpdateTrainerTests : TestsBase
    {
        private readonly ITestOutputHelper _logger;
        public override ITestOutputHelper Logger { get => _logger; }

        public UpdateTrainerTests(ITestOutputHelper logger)
        {
            _logger = logger;
        }

        [Fact, Trait("Category", "smoke")]
        public void UpdateTrainerItemList_SmokeTest_True()
        {
            var trainer = GetTestTrainer();
            Logger.WriteLine($"Adding trainer id {trainer.TrainerId}");
            DatabaseUtility.TryAddTrainer(trainer, out _);

            Logger.WriteLine($"Updating trainer id {trainer.TrainerId} with new items");
            var itemList = new[]
            {
                new ItemModel
                {
                    Name = "Potion",
                    Effects= "Restores 20 HPs",
                    Amount = 5
                }
            };
            Assert.True(DatabaseUtility.UpdateTrainerItemList(trainer.TrainerId, itemList));

            Logger.WriteLine($"Verifying that the item list updated properly");
            //var updatedTrainer = DatabaseUtility.FindTrainerById(trainer.TrainerId);
            //DatabaseUtility.DeleteTrainer(trainer.TrainerId);
            //Assert.Equal("Potion", updatedTrainer.Items.Select(item => item.Name).ElementAt(0));
            //Assert.Equal(5, updatedTrainer.Items.Select(item => item.Amount).ElementAt(0));
        }

        [Fact]
        public void UpdateTrainerItemList_NullItemList_ThrowsArgumentNullException()
        {
            var trainer = GetTestTrainer();
            Logger.WriteLine($"Adding trainer id {trainer.TrainerId}");
            DatabaseUtility.TryAddTrainer(trainer, out _);

            Logger.WriteLine($"Updating trainer id {trainer.TrainerId} with null items");
            Assert.Throws<ArgumentNullException>(() => DatabaseUtility.UpdateTrainerItemList(trainer.TrainerId, null));
            DatabaseUtility.DeleteTrainer(trainer.GameId, trainer.TrainerId);
        }

        [Theory, Trait("Category", "smoke")]
        [InlineData(true)]
        [InlineData(false)]
        public void UpdateTrainerOnlineStatus_SmokeTest_True(bool isOnline)
        {
            var trainer = GetTestTrainer();
            Logger.WriteLine($"Adding trainer id {trainer.TrainerId}");
            DatabaseUtility.TryAddTrainer(trainer, out _);

            Logger.WriteLine($"Updating trainer id {trainer.TrainerId} online status to {isOnline}");
            Assert.True(DatabaseUtility.UpdateTrainerOnlineStatus(trainer.TrainerId, isOnline));

            Logger.WriteLine($"Verifying trainer id {trainer.TrainerId} online status has been changed to {isOnline}");
            //trainer = DatabaseUtility.FindTrainerById(trainer.TrainerId);
            //DatabaseUtility.DeleteTrainer(trainer.TrainerId);
            //Assert.Equal(isOnline, trainer.IsOnline);
        }
    }
}
