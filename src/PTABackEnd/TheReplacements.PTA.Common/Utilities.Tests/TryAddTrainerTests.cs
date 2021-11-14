using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace TheReplacements.PTA.Common.Utilities.Tests
{
    public class TryAddTrainerTests : TestsBase
    {
        public TryAddTrainerTests(ITestOutputHelper output)
        {
            Logger = output;
        }

        [Fact, Trait("Category", "smoke")]
        public void TryAddTrainer_SmokeTest_True()
        {
            PerformTryAddTrainerPassTest(GetTestTrainer());
        }

        [Fact]
        public void TryAddTrainer_LevelOutOfRange_False()
        {
            var trainer = GetTestTrainer();
            trainer.Level = -1;
            PerformTryAddTrainerFailTest(trainer);
        }

        [Fact]
        public void TryAddTrainer_StatsNull_False()
        {
            var trainer = GetTestTrainer();
            trainer.TrainerStats = null;
            PerformTryAddTrainerFailTest(trainer);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(36)]
        public void TryAddTrainer_FeatsCountInRange_True(int count)
        {
            var trainer = GetTestTrainer();
            var feats = new string[count];
            for (int i = 0; i < count; i++)
            {
                feats[i] = $"Class{i}";
            }

            trainer.Feats = feats;
            PerformTryAddTrainerPassTest(trainer);
        }

        [Fact]
        public void TryAddTrainer_FeatsCountOutOfRange_False()
        {
            var trainer = GetTestTrainer();
            var feats = new string[37];
            for (int i = 0; i < 5; i++)
            {
                feats[i] = $"Class{i}";
            }

            trainer.Feats = feats;
            PerformTryAddTrainerFailTest(trainer);
        }

        [Fact]
        public void TryAddTrainer_FeatsNull_False()
        {
            var trainer = GetTestTrainer();
            trainer.Feats = null;
            PerformTryAddTrainerFailTest(trainer);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(4)]
        public void TryAddTrainer_ClassesCountInRange_True(int count)
        {
            var trainer = GetTestTrainer();
            var classes = new string[count];
            for (int i = 0; i < count; i++)
            {
                classes[i] = $"Class{i}";
            }

            trainer.TrainerClasses = classes;
            PerformTryAddTrainerPassTest(trainer);
        }

        [Fact]
        public void TryAddTrainer_ClassesCountOutOfRange_False()
        {
            var trainer = GetTestTrainer();
            var classes = new string[5];
            for (int i = 0; i < 5; i++)
            {
                classes[i] = $"Class{i}";
            }

            trainer.TrainerClasses = classes;
            PerformTryAddTrainerFailTest(trainer);
        }

        [Fact]
        public void TryAddTrainer_ClassesNull_False()
        {
            var trainer = GetTestTrainer();
            trainer.TrainerClasses = null;
            PerformTryAddTrainerFailTest(trainer);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void TryAddTrainer_PasswordInvalid_False(string password)
        {
            var trainer = GetTestTrainer();
            trainer.PasswordHash = password;
            PerformTryAddTrainerFailTest(trainer);
        }

        [Fact]
        public void TryAddTrainer_ItemValid_True()
        {
            var trainer = GetTestTrainer();
            trainer.Items.Add(new Models.ItemModel
            {
                Name = "Test Item",
                Amount = 5
            });
            PerformTryAddTrainerPassTest(trainer);
        }

        [Theory]
        [InlineData("", 5)]
        [InlineData(null, 5)]
        [InlineData("Test Item", 0)]
        [InlineData("", 0)]
        [InlineData(null, 0)]
        public void TryAddTrainer_ItemInvalid_False(
            string name,
            int amount)
        {
            var trainer = GetTestTrainer();
            trainer.Items.Add(new Models.ItemModel
            {
                Name = name,
                Amount = amount
            });
            PerformTryAddTrainerFailTest(trainer);
        }
    }
}
