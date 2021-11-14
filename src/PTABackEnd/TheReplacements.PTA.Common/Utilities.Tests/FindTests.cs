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
