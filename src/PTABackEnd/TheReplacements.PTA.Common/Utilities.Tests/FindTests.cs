using System;
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
            var retrievedNpcsIds = DatabaseUtility.FindNpcs(npcIds).Select(npc => npc.NPCId);
            foreach (var npcId in retrievedNpcsIds)
            {
                DatabaseUtility.DeleteNpc(npcId);
            }

            Logger.WriteLine($"Verifying retrieved list of npcs are from the original set");
            Assert.True(retrievedNpcsIds.All(npcId => npcIds.Contains(npcId)));
            Assert.Equal(npcCount, retrievedNpcsIds.Count());
        }

        [Fact]
        public void FindNpcs_NullIds_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => DatabaseUtility.FindNpcs(null));
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

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        public void FindPokemonByTrainerId_ValidIds_PokemonCount(int pokemonCount)
        {
            var trainer = GetTestTrainer();
            DatabaseUtility.TryAddTrainer(trainer, out _);

            Logger.WriteLine($"Adding {pokemonCount} pokemon to trainer id {trainer.TrainerId}");
            var pokemonIds = new List<string>();
            for (int i = 0; i < pokemonCount; i++)
            {
                var pokemon = GetTestPokemon();
                pokemon.TrainerId = trainer.TrainerId;
                DatabaseUtility.TryAddPokemon(pokemon, out _);
                pokemonIds.Add(pokemon.PokemonId);
            }

            Logger.WriteLine($"Retrieving list of pokemonIds with trainer id {trainer.TrainerId}");
            var retrievedPokemonIds = DatabaseUtility.FindPokemonByTrainerId(trainer.TrainerId).Select(pokemon => pokemon.PokemonId);
            DatabaseUtility.DeleteTrainer(trainer.TrainerId);
            foreach (var pokemonId in retrievedPokemonIds)
            {
                DatabaseUtility.DeletePokemon(pokemonId);
            }

            Logger.WriteLine($"Verifying that the retrieved list matcheds the original set");
            Assert.True(retrievedPokemonIds.All(pokemonId => retrievedPokemonIds.Contains(pokemonId)));
            Assert.Equal(pokemonCount, retrievedPokemonIds.Count());
        }

        [Fact]
        public void FindPokemonByTrainerId_NullId_EmptyList()
        {
            Logger.WriteLine($"Retrieving pokemon list");
            var pokemonIds = DatabaseUtility.FindPokemonByTrainerId(null);
            Logger.WriteLine($"Verifying the pokemon list is empty");
            Assert.Empty(pokemonIds);
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

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        public void FindTrainerByGameId_ValidIds_TrainerCount(int trainerCount)
        {
            var game = GetTestGame();
            DatabaseUtility.TryAddGame(game, out _);

            Logger.WriteLine($"Adding {trainerCount} trainers to game id {game.GameId}");
            var trainerIds = new List<string>();
            for (int i = 0; i < trainerCount; i++)
            {
                var trainer = GetTestTrainer();
                trainer.GameId = game.GameId;
                DatabaseUtility.TryAddTrainer(trainer, out _);
                trainerIds.Add(trainer.TrainerId);
            }

            Logger.WriteLine($"Retrieving list of trainerIds with game id {game.GameId}");
            var retrievedTrainerIds = DatabaseUtility.FindTrainersByGameId(game.GameId).Select(trainer => trainer.TrainerId);
            DatabaseUtility.DeleteGame(game.GameId);
            foreach (var trainerId in retrievedTrainerIds)
            {
                DatabaseUtility.DeleteTrainer(trainerId);
            }

            Logger.WriteLine($"Verifying that the retrieved list matches the original set");
            Assert.True(retrievedTrainerIds.All(trainerId => trainerIds.Contains(trainerId)));
            Assert.Equal(trainerCount, retrievedTrainerIds.Count());
        }

        [Fact]
        public void FindTrainerByGameId_NullId_EmptyList()
        {
            Logger.WriteLine($"Retrieving trainer list");
            var trainerIds = DatabaseUtility.FindTrainersByGameId(null);
            Logger.WriteLine($"Verifying that the retrieved list is empty");
            Assert.Empty(trainerIds);
        }

        [Theory]
        [InlineData("testusername")]
        [InlineData("testusername1")]
        public void FindTrainerByUsername_ValidUsername_TrainerModel(string username)
        {
            var trainer = GetTestTrainer();
            trainer.TrainerName = username;            
            Logger.WriteLine($"Adding trainer with username {username}");
            DatabaseUtility.TryAddTrainer(trainer, out _);

            Logger.WriteLine($"Retrieving trainer with username {username}");
            var retrievedTrainer = DatabaseUtility.FindTrainerByUsername
            (
                username,
                trainer.GameId
            );

            Logger.WriteLine($"Verifying that the trainer found is the correct trainer");
            DatabaseUtility.DeleteTrainer(trainer.TrainerId);
            Assert.Equal(username, retrievedTrainer.TrainerName);
            Assert.Equal(trainer.GameId, retrievedTrainer.GameId);
            Assert.Equal(trainer.TrainerId, retrievedTrainer.TrainerId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("A name that doesn't exist")]
        public void FindTrainerByUsername_InvalidUsername_Null(string username)
        {
            var trainer = GetTestTrainer();
            Logger.WriteLine($"Adding trainer with username {trainer.TrainerName}");
            DatabaseUtility.TryAddTrainer(trainer, out _);

            Logger.WriteLine($"Retrieving trainer with username {username}");
            var retrievedTrainer = DatabaseUtility.FindTrainerByUsername
            (
                username,
                trainer.GameId
            );

            Logger.WriteLine($"Verifying that no trainer was found");
            DatabaseUtility.DeleteTrainer(trainer.TrainerId);
            Assert.Null(retrievedTrainer);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("A game that doesn't exist")]
        public void FindTrainerByUsername_InvalidGameId_Null(string gameId)
        {
            var trainer = GetTestTrainer();
            Logger.WriteLine($"Adding trainer with username {trainer.TrainerName}");
            DatabaseUtility.TryAddTrainer(trainer, out _);

            Logger.WriteLine($"Retrieving trainer with username {trainer.TrainerName}");
            var retrievedTrainer = DatabaseUtility.FindTrainerByUsername
            (
                trainer.TrainerName,
                gameId
            );

            Logger.WriteLine($"Verifying that no trainer was found");
            DatabaseUtility.DeleteTrainer(trainer.TrainerId);
            Assert.Null(retrievedTrainer);
        }
    }
}
