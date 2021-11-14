using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace TheReplacements.PTA.Common.Utilities.Tests
{
    public class FindTests : TestsBase
    {
        public FindTests(ITestOutputHelper output)
        {
            Logger = output;
        }

        [Fact]
        public void FindGame_ValidId_NotNull()
        {
            var game = GetTestGame();
            DatabaseUtility.TryAddGame(game, out _);
            Logger.WriteLine($"Retrieving game id {game.GameId}");
            Assert.NotNull(DatabaseUtility.FindGame(game.GameId));
            DatabaseUtility.DeleteGame(game.GameId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("Invalid Id")]
        public void FindGame_InvalidId_Null(string id)
        {
            Logger.WriteLine($"Retrieving game id {id}");
            Assert.Null(DatabaseUtility.FindGame(id));
        }

        [Fact]
        public void FindNpc_ValidId_NotNull()
        {
            var npc = GetTestNpc();
            DatabaseUtility.TryAddNpc(npc, out _);
            Logger.WriteLine($"Retrieving npc id {npc.NPCId}");
            Assert.NotNull(DatabaseUtility.FindNpc(npc.NPCId));
            DatabaseUtility.DeleteNpc(npc.NPCId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("Invalid Id")]
        public void FindNpc_InvalidId_Null(string id)
        {
            Logger.WriteLine($"Retrieving npc id {id}");
            Assert.Null(DatabaseUtility.FindNpc(id));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        public void FindNpcs_ValidIds_NpcCount(int npcCount)
        {
            Logger.WriteLine($"Adding {npcCount} npcs");
            var npcIds = new List<string>();
            for (int i = 0; i < npcCount; i++)
            {
                var npc = GetTestNpc();
                DatabaseUtility.TryAddNpc(npc, out _);
                npcIds.Add(npc.NPCId);
            }

            Logger.WriteLine($"Retrieving npc ids {string.Join(",", npcIds)}");
            var retrievedNpcs = DatabaseUtility.FindNpcs(npcIds);
            foreach (var npc in retrievedNpcs)
            {
                DatabaseUtility.DeleteNpc(npc.NPCId);
            }
            Logger.WriteLine($"Verifying retrieved list of npcs are from the original set");
            Assert.True(retrievedNpcs.All(npc => npcIds.Contains(npc.NPCId)));
            Assert.Equal(npcCount, retrievedNpcs.Count());
        }

        [Fact]
        public void FindPokemonById_ValidId_NotNull()
        {
            var pokemon = GetTestPokemon();
            DatabaseUtility.TryAddPokemon(pokemon, out _);
            Logger.WriteLine($"Retrieving npc id {pokemon.PokemonId}");
            Assert.NotNull(DatabaseUtility.FindPokemonById(pokemon.PokemonId));
            DatabaseUtility.DeletePokemon(pokemon.PokemonId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("Invalid Id")]
        public void FindPokemonById_InvalidId_Null(string id)
        {
            Logger.WriteLine($"Retrieving npc id {id}");
            Assert.Null(DatabaseUtility.FindPokemonById(id));
        }

        [Fact]
        public void FindTrainerById_ValidId_NotNull()
        {
            var trainer = GetTestTrainer();
            DatabaseUtility.TryAddTrainer(trainer, out _);
            Logger.WriteLine($"Retrieving npc id {trainer.TrainerId}");
            Assert.NotNull(DatabaseUtility.FindTrainerById(trainer.TrainerId));
            DatabaseUtility.DeleteTrainer(trainer.TrainerId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("Invalid Id")]
        public void FindTrainerById_InvalidId_Null(string id)
        {
            Logger.WriteLine($"Retrieving npc id {id}");
            Assert.Null(DatabaseUtility.FindTrainerById(id));
        }
    }
}
