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
            PerformTryAddTrainerPassTest(TestTrainer);
        }

        [Fact]
        public void TryAddTrainer_LevelOutOfRange_False()
        {
            var trainer = TestTrainer;
            trainer.Level = -1;
            PerformTryAddTrainerFailTest(trainer);
        }

        [Fact]
        public void TryAddTrainer_StatsNull_False()
        {
            var trainer = TestTrainer;
            trainer.TrainerStats = null;
            PerformTryAddTrainerFailTest(trainer);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(36)]
        public void TryAddTrainer_FeatsCountInRange_True(int count)
        {
            var trainer = TestTrainer;
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
            var trainer = TestTrainer;
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
            var trainer = TestTrainer;
            trainer.Feats = null;
            PerformTryAddTrainerFailTest(trainer);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(4)]
        public void TryAddTrainer_ClassesCountInRange_True(int count)
        {
            var trainer = TestTrainer;
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
            var trainer = TestTrainer;
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
            var trainer = TestTrainer;
            trainer.TrainerClasses = null;
            PerformTryAddTrainerFailTest(trainer);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void TryAddTrainer_PasswordInvalid_False(string password)
        {
            var trainer = TestTrainer;
            trainer.PasswordHash = password;
            PerformTryAddTrainerFailTest(trainer);
        }
    }
}
