﻿using TheReplacement.PTA.Common.Models;
using Xunit;
using Xunit.Abstractions;

namespace TheReplacement.PTA.Common.Tests.Utilities
{
    public class TryAddTrainerTests : TestsBase
    {
        private readonly ITestOutputHelper _logger;
        public override ITestOutputHelper Logger { get => _logger; }

        public TryAddTrainerTests(ITestOutputHelper output)
        {
            _logger = output;
        }

        [Fact, Trait("Category", "smoke")]
        public void TryAddTrainer_SmokeTest_True()
        {
            PerformTryAddTrainerPassTest(GetTestTrainer(), Logger);
        }

        [Fact]
        public void TryAddTrainer_StatsNull_False()
        {
            var trainer = GetTestTrainer();
            trainer.TrainerStats = null;
            PerformTryAddTrainerFailTest(trainer, Logger);
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
            PerformTryAddTrainerPassTest(trainer, Logger);
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
            PerformTryAddTrainerFailTest(trainer, Logger);
        }

        [Fact]
        public void TryAddTrainer_FeatsNull_False()
        {
            var trainer = GetTestTrainer();
            trainer.Feats = null;
            PerformTryAddTrainerFailTest(trainer, Logger);
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
            PerformTryAddTrainerPassTest(trainer, Logger);
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
            PerformTryAddTrainerFailTest(trainer, Logger);
        }

        [Fact]
        public void TryAddTrainer_ClassesNull_False()
        {
            var trainer = GetTestTrainer();
            trainer.TrainerClasses = null;
            PerformTryAddTrainerFailTest(trainer, Logger);
        }

        [Fact]
        public void TryAddTrainer_ItemValid_True()
        {
            var trainer = GetTestTrainer();
            trainer.Items.Add(new ItemModel
            {
                Name = "Test Item",
                Effects = "",
                Amount = 5
            });
            PerformTryAddTrainerPassTest(trainer, Logger);
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
            trainer.Items.Add(new ItemModel
            {
                Name = name,
                Amount = amount
            });
            PerformTryAddTrainerFailTest(trainer, Logger);
        }
    }
}
