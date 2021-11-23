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
                    Amount = 5
                }
            };
            Assert.True(DatabaseUtility.UpdateTrainerItemList(trainer.TrainerId, itemList));

            Logger.WriteLine($"Verifying that the item list updated properly");
            var updatedTrainer = DatabaseUtility.FindTrainerById(trainer.TrainerId);
            DatabaseUtility.DeleteTrainer(trainer.TrainerId);
            Assert.Equal("Potion", updatedTrainer.Items.Select(item => item.Name).ElementAt(0));
            Assert.Equal(5, updatedTrainer.Items.Select(item => item.Amount).ElementAt(0));
        }

        [Fact]
        public void UpdateTrainerItemList_NullItemList_ThrowsArgumentNullException()
        {
            var trainer = GetTestTrainer();
            Logger.WriteLine($"Adding trainer id {trainer.TrainerId}");
            DatabaseUtility.TryAddTrainer(trainer, out _);

            Logger.WriteLine($"Updating trainer id {trainer.TrainerId} with null items");
            Assert.Throws<ArgumentNullException>(() => DatabaseUtility.UpdateTrainerItemList(trainer.TrainerId, null));
            DatabaseUtility.DeleteTrainer(trainer.TrainerId);
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
            trainer = DatabaseUtility.FindTrainerById(trainer.TrainerId);
            DatabaseUtility.DeleteTrainer(trainer.TrainerId);
            Assert.Equal(isOnline, trainer.IsOnline);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("A game that doesn't exist")]
        public void UpdateTrainerOnlineStatus_InvalidGameid_False(string trainerId)
        {
            var trainer = GetTestTrainer();
            Logger.WriteLine($"Adding trainer id {trainer.TrainerId}");
            DatabaseUtility.TryAddTrainer(trainer, out _);

            Logger.WriteLine($"Updating trainer id {trainer.TrainerId} online status to true");
            Assert.False(DatabaseUtility.UpdateTrainerOnlineStatus(trainerId, true));

            Logger.WriteLine($"Verifying trainer id {trainer.TrainerId} online status has not been changed to true");
            DatabaseUtility.DeleteTrainer(trainer.TrainerId);
            trainer = DatabaseUtility.FindTrainerById(trainerId);
            Assert.Null(trainer);
        }

        [Fact, Trait("Category", "smoke")]
        public void UpdateTrainerPassword_SmokeTest_NotNull()
        {
            var trainer = GetTestTrainer();
            Logger.WriteLine($"Adding trainer id {trainer.TrainerId}");
            DatabaseUtility.TryAddTrainer(trainer, out _);

            var updatedPassword = "updatedPassword";
            Logger.WriteLine($"Updating trainer id {trainer.TrainerId} password");
            Assert.True(DatabaseUtility.UpdateTrainerPassword(trainer.TrainerId, updatedPassword));
            var updatedTrainer = DatabaseUtility.FindTrainerById(trainer.TrainerId);
            DatabaseUtility.DeleteTrainer(trainer.TrainerId);

            Logger.WriteLine($"Verifying trainer id {trainer.TrainerId} password with {updatedPassword}");
            Assert.True(EncryptionUtility.VerifySecret(updatedPassword, updatedTrainer.PasswordHash));
        }
    }
}
