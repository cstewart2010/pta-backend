using Xunit;
using Xunit.Abstractions;

namespace TheReplacements.PTA.Common.Utilities.Tests
{
    public class DeleteTests : TestsBase
    {
        public DeleteTests(ITestOutputHelper output)
        {
            Logger = output;
        }

        [Fact]
        public void DeleteGame_ValidId_True()
        {
            var game = GetTestGame();
            Logger.WriteLine($"Inserting game with game id {game.GameId}");
            DatabaseUtility.TryAddGame(game, out _);
            Logger.WriteLine($"Attempting to delete game with game id {game.GameId}");
            Assert.True(DatabaseUtility.DeleteGame(game.GameId));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("Invalid id")]
        public void DeleteGame_InvalidId_False(string id)
        {
            Logger.WriteLine($"Attempting to delete game with game id {id}");
            Assert.False(DatabaseUtility.DeleteGame(id));
        }

        [Fact]
        public void DeleteNpc_ValidId_True()
        {
            var npc = GetTestNpc();
            Logger.WriteLine($"Inserting npc with npc id {npc.NPCId}");
            DatabaseUtility.TryAddNpc(npc, out _);
            Logger.WriteLine($"Attempting to delete npc with npc id {npc.NPCId}");
            Assert.True(DatabaseUtility.DeleteNpc(npc.NPCId));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("Invalid id")]
        public void DeleteNpc_InvalidId_False(string id)
        {
            Logger.WriteLine($"Attempting to delete npc with npc id {id}");
            Assert.False(DatabaseUtility.DeleteNpc(id));
        }

        [Fact]
        public void DeletePokemon_ValidId_True()
        {
            var pokemon = GetTestPokemon();
            Logger.WriteLine($"Inserting pokemon with pokemon id {pokemon.PokemonId}");
            DatabaseUtility.TryAddPokemon(pokemon, out _);
            Logger.WriteLine($"Attempting to delete pokemon with pokemon id {pokemon.PokemonId}");
            Assert.True(DatabaseUtility.DeletePokemon(pokemon.PokemonId));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("Invalid id")]
        public void DeletePokemon_InvalidId_False(string id)
        {
            Logger.WriteLine($"Attempting to delete pokemon with pokemon id {id}");
            Assert.False(DatabaseUtility.DeletePokemon(id));
        }

        [Fact]
        public void DeletePokemonByTrainerId_ValidTrainerIdNoTrainers_Zero()
        {
            var trainer = GetTestTrainer();
            Logger.WriteLine($"Adding trainer with trainer id {trainer.TrainerId}");
            DatabaseUtility.TryAddTrainer(trainer, out _);
            Logger.WriteLine($"Attempting to delete all pokemon with trainer id {trainer.TrainerId}");
            Assert.Equal(0, DatabaseUtility.DeletePokemonByTrainerId(trainer.TrainerId));
            DatabaseUtility.DeleteTrainer(trainer.TrainerId);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        public void DeletePokemonByTrainerId_ValidTrainerIdWithTrainer_PokemonCount(long pokemonCount)
        {
            var trainer = GetTestTrainer();
            Logger.WriteLine($"Adding trainer with trainer id {trainer.TrainerId}");
            DatabaseUtility.TryAddTrainer(trainer, out _);
            Logger.WriteLine($"Adding {pokemonCount} pokemon to trainer id {trainer.TrainerId}");
            for (var i = 0; i < pokemonCount; i++)
            {
                var pokemon = GetTestPokemon();
                pokemon.TrainerId = trainer.TrainerId;
                DatabaseUtility.TryAddPokemon(pokemon, out _);
            }

            Logger.WriteLine($"Attempting to delete all pokemon from trainer id {trainer.TrainerId}");
            Assert.Equal(pokemonCount, DatabaseUtility.DeletePokemonByTrainerId(trainer.TrainerId));
            DatabaseUtility.DeleteTrainer(trainer.TrainerId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("Invalid id")]
        public void DeletePokemonByTrainerId_InvalidTrainerId_Zero(string id)
        {
            Logger.WriteLine($"Attempting to delete all pokemon with trainer id {id}");
            Assert.Equal(0, DatabaseUtility.DeletePokemonByTrainerId(id));
        }

        [Fact]
        public void DeleteTrainer_ValidId_True()
        {
            var trainer = GetTestTrainer();
            Logger.WriteLine($"Inserting trainer with trainer id {trainer.TrainerId}");
            DatabaseUtility.TryAddTrainer(trainer, out _);
            Logger.WriteLine($"Attempting to delete trainer with trainer id {trainer.TrainerId}");
            Assert.True(DatabaseUtility.DeleteTrainer(trainer.TrainerId));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("Invalid id")]
        public void DeleteTrainer_InvalidId_False(string id)
        {
            Logger.WriteLine($"Attempting to delete trainer with trainer id {id}");
            Assert.False(DatabaseUtility.DeleteTrainer(id));
        }

        [Fact]
        public void DeleteTrainersByGameid_ValidGameIdNoTrainers_Zero()
        {
            var game = GetTestGame();
            Logger.WriteLine($"Adding game with game id {game.GameId}");
            DatabaseUtility.TryAddGame(game, out _);
            Logger.WriteLine($"Attempting to delete all trainers with game id {game.GameId}");
            Assert.Equal(0, DatabaseUtility.DeleteTrainersByGameId(game.GameId));
            DatabaseUtility.DeleteGame(game.GameId);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        public void DeleteTrainersByGameId_ValidGameIdWithTrainer_TrainerCount(long trainerCount)
        {
            var game = GetTestGame();
            Logger.WriteLine($"Adding game with game id {game.GameId}");
            DatabaseUtility.TryAddGame(game, out _);
            Logger.WriteLine($"Adding {trainerCount} trainers to game id {game.GameId}");
            for (var i = 0; i < trainerCount; i++)
            {
                var trainer = GetTestTrainer();
                trainer.GameId = game.GameId;
                DatabaseUtility.TryAddTrainer(trainer, out _);
            }
            
            Logger.WriteLine($"Attempting to delete all trainers from game id {game.GameId}");
            Assert.Equal(trainerCount, DatabaseUtility.DeleteTrainersByGameId(game.GameId));
            DatabaseUtility.DeleteGame(game.GameId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("Invalid id")]
        public void DeleteTrainersByGameId_InvalidGameId_Zero(string id)
        {
            Logger.WriteLine($"Attempting to delete all trainers with game id {id}");
            Assert.Equal(0, DatabaseUtility.DeleteTrainersByGameId(id));
        }
    }
}
