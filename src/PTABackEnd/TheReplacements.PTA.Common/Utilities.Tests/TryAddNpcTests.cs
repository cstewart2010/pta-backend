using Xunit;
using Xunit.Abstractions;

namespace TheReplacements.PTA.Common.Utilities.Tests
{
    public class TryAddNpcTests : TestsBase
    {
        public TryAddNpcTests(ITestOutputHelper output)
        {
            Logger = output;
        }

        [Fact, Trait("Category", "smoke")]
        public void TryAddNpc_SmokeTest_True()
        {
            PerformTryAddNpcPassTest(GetTestNpc());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(36)]
        public void TryAddNpc_FeatsCountInRange_True(int count)
        {
            var npc = GetTestNpc();
            var feats = new string[count];
            for (int i = 0; i < count; i++)
            {
                feats[i] = $"Class{i}";
            }

            npc.Feats = feats;
            PerformTryAddNpcPassTest(npc);
        }

        [Fact]
        public void TryAddNpc_FeatsCountOutOfRange_False()
        {
            var npc = GetTestNpc();
            var feats = new string[37];
            for (int i = 0; i < 5; i++)
            {
                feats[i] = $"Class{i}";
            }

            npc.Feats = feats;
            PerformTryAddNpcFailTest(npc);
        }
        
        [Fact]
        public void TryAddNpc_FeatsNull_False()
        {
            var npc = GetTestNpc();
            npc.Feats = null;
            PerformTryAddNpcFailTest(npc);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(4)]
        public void TryAddNpc_ClassesCountInRange_True(int count)
        {
            var npc = GetTestNpc();
            var classes = new string[count];
            for (int i = 0; i < count; i++)
            {
                classes[i] = $"Class{i}";
            }

            npc.TrainerClasses = classes;
            PerformTryAddNpcPassTest(npc);
        }

        [Fact]
        public void TryAddNpc_ClassesCountOutOfRange_False()
        {
            var npc = GetTestNpc();
            var classes = new string[5];
            for (int i = 0; i < 5; i++)
            {
                classes[i] = $"Class{i}";
            }

            npc.TrainerClasses = classes;
            PerformTryAddNpcFailTest(npc);
        }

        [Fact]
        public void TryAddNpc_ClassesNull_False()
        {
            var npc = GetTestNpc();
            npc.TrainerClasses = null;
            PerformTryAddNpcFailTest(npc);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void TryAddNpc_NameNull_False(string name)
        {
            var npc = GetTestNpc();
            npc.TrainerName = name;
            PerformTryAddNpcFailTest(npc);
        }

        [Fact]
        public void TryAddNpc_StatsNull_False()
        {
            var npc = GetTestNpc();
            npc.TrainerStats = null;
            PerformTryAddNpcFailTest(npc);
        }
    }
}
