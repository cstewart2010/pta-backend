using TheReplacement.PTA.Common.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace TheReplacement.PTA.Common.Tests.Utilities
{
    public class DeleteTests : TestsBase
    {
        private readonly ITestOutputHelper _logger;
        public override ITestOutputHelper Logger { get => _logger; }

        public DeleteTests(ITestOutputHelper logger)
        {
            _logger = logger;
        }

        [Fact, Trait("Category", "smoke")]
        public void DeleteGame_SmokeTest_True()
        {
            var game = GetTestGame();
            Logger.WriteLine($"Inserting game with game id {game.GameId}");
            DatabaseUtility.TryAddGame(game, out _);
            Logger.WriteLine($"Attempting to delete game with game id {game.GameId}");
            Assert.True(DatabaseUtility.DeleteGame(game.GameId));
        }

        [Fact, Trait("Category", "smoke")]
        public void DeleteNpc_SmokeTest_True()
        {
            var npc = GetTestNpc();
            Logger.WriteLine($"Inserting npc with npc id {npc.NPCId}");
            DatabaseUtility.TryAddNpc(npc, out _);
            Logger.WriteLine($"Attempting to delete npc with npc id {npc.NPCId}");
            Assert.True(DatabaseUtility.DeleteNpc(npc.NPCId));
        }

        [Fact, Trait("Category", "smoke")]
        public void DeletePokemon_SmokeTest_True()
        {
            var pokemon = GetTestPokemon();
            Logger.WriteLine($"Inserting pokemon with pokemon id {pokemon.PokemonId}");
            DatabaseUtility.TryAddPokemon(pokemon, out _);
            Logger.WriteLine($"Attempting to delete pokemon with pokemon id {pokemon.PokemonId}");
            Assert.True(DatabaseUtility.DeletePokemon(pokemon.PokemonId));
        }

        [Fact]
        public void DeletePokemonByTrainerId_ValidTrainerIdNoTrainers_Zero()
        {
            var trainer = GetTestTrainer();
            Logger.WriteLine($"Adding trainer with trainer id {trainer.TrainerId}");
            DatabaseUtility.TryAddTrainer(trainer, out _);
            Logger.WriteLine($"Attempting to delete all pokemon with trainer id {trainer.TrainerId}");
            Assert.Equal(0, DatabaseUtility.DeletePokemonByTrainerId(trainer.GameId, trainer.TrainerId));
            DatabaseUtility.DeleteTrainer(trainer.GameId, trainer.TrainerId);
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
            Assert.Equal(pokemonCount, DatabaseUtility.DeletePokemonByTrainerId(trainer.GameId, trainer.TrainerId));
            DatabaseUtility.DeleteTrainer(trainer.GameId, trainer.TrainerId);
        }

        [Fact, Trait("Category", "smoke")]
        public void DeleteTrainer_SmokeTest_True()
        {
            var trainer = GetTestTrainer();
            Logger.WriteLine($"Inserting trainer with trainer id {trainer.TrainerId}");
            DatabaseUtility.TryAddTrainer(trainer, out _);
            Logger.WriteLine($"Attempting to delete trainer with trainer id {trainer.TrainerId}");
            Assert.True(DatabaseUtility.DeleteTrainer(trainer.GameId, trainer.TrainerId));
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
    }
}
