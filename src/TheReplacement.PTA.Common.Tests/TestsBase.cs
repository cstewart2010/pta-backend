using System;
using System.Collections.Generic;
using TheReplacement.PTA.Common.Enums;
using TheReplacement.PTA.Common.Models;
using TheReplacement.PTA.Common.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace TheReplacement.PTA.Common.Tests
{
    public abstract class TestsBase
    {
        public abstract ITestOutputHelper Logger { get; }

        public static GameModelv1 GetTestGame()
        {
            return new GameModelv1
            {
                GameId = Guid.NewGuid(),
                Nickname = "Test Nickname",
                NPCs = Array.Empty<Guid>(),
                PasswordHash = "testpassword"
            };
        }

        public static NpcModelv1 GetTestNpc()
        {
            return new NpcModelv1
            {
                NPCId = Guid.NewGuid(),
                Feats = Array.Empty<string>(),
                TrainerClasses = Array.Empty<string>(),
                TrainerName = "Test Trainername",
                TrainerStats = new StatsModelv1()
            };
        }

        public static PokemonModelv1 GetTestPokemon()
        {
            var pokemon = DexUtility.GetNewPokemon("Flabébé", Nature.Modest, Gender.Female, Status.Normal, "", "base");
            pokemon.TrainerId = Guid.NewGuid();
            return pokemon;
        }

        public static TrainerModelv1 GetTestTrainer()
        {
            return new TrainerModelv1
            {
                Feats = Array.Empty<string>(),
                GameId = Guid.NewGuid(),
                Items = new List<ItemModelv1>(),
                TrainerClasses = Array.Empty<string>(),
                TrainerId = Guid.NewGuid(),
                TrainerName = "Test Trainer",
                TrainerStats = new StatsModelv1
                {
                    HP = 20,
                    Attack = 1,
                    Defense = 1,
                    SpecialAttack = 1,
                    SpecialDefense = 1,
                    Speed = 1
                },
                Honors = Array.Empty<string>(),
                Origin = string.Empty
            };
        }

        public static void PerformTryAddGamePassTest(GameModelv1 game, ITestOutputHelper logger)
        {
            logger.WriteLine($"Adding new game with game id {game.GameId}");
            Assert.True(DatabaseUtility.TryAddGame(game, out var error));
            logger.WriteLine($"Verify that error object is null");
            Assert.Null(error);
            logger.WriteLine($"Removing game with game id {game.GameId}");
            DatabaseUtility.DeleteGame(game.GameId);
        }

        public static void PerformTryAddGameFailTest(GameModelv1 game, ITestOutputHelper logger)
        {
            logger.WriteLine($"Adding new game with game id {game.GameId}");
            Assert.False(DatabaseUtility.TryAddGame(game, out var error));
            logger.WriteLine($"Verify that error object is null");
            Assert.NotNull(error?.WriteErrorJsonString);
            logger.WriteLine($"Verify no game is found with game id {game.GameId}");
            Assert.Null(DatabaseUtility.FindGame(game.GameId));
        }

        public static void PerformTryAddNpcPassTest(NpcModelv1 npc, ITestOutputHelper logger)
        {
            logger.WriteLine($"Adding new game with npc id {npc.NPCId}");
            Assert.True(DatabaseUtility.TryAddNpc(npc, out var error));
            logger.WriteLine($"Verify that error object is null");
            Assert.Null(error);
            logger.WriteLine($"Removing game with game id {npc.NPCId}");
            DatabaseUtility.DeleteNpc(npc.NPCId);
        }

        public static void PerformTryAddNpcFailTest(NpcModelv1 npc, ITestOutputHelper logger)
        {
            logger.WriteLine($"Adding new npc with npc id {npc.NPCId}");
            Assert.False(DatabaseUtility.TryAddNpc(npc, out var error));
            logger.WriteLine($"Verify that error object is null");
            Assert.NotNull(error?.WriteErrorJsonString);
            logger.WriteLine($"Verify no npc is found with npc id {npc.NPCId}");
            Assert.Empty(DatabaseUtility.FindNpcs(new[] { npc.NPCId }));
        }

        public static void PerformTryAddPokemonFailTest(PokemonModelv1 pokemon, ITestOutputHelper logger)
        {
            logger.WriteLine($"Adding new pokemon with pokemon id {pokemon.PokemonId}");
            Assert.False(DatabaseUtility.TryAddPokemon(pokemon, out var error));
            logger.WriteLine($"Verify that error object is null");
            Assert.NotNull(error?.WriteErrorJsonString);
            logger.WriteLine($"Verify no pokemon is found with pokemon id {pokemon.PokemonId}");
            Assert.Null(DatabaseUtility.FindPokemonById(pokemon.PokemonId));
        }

        public static void PerformTryAddPokemonPassTest(PokemonModelv1 pokemon, ITestOutputHelper logger)
        {
            logger.WriteLine($"Adding new pokemon with pokemon id {pokemon.PokemonId}");
            Assert.True(DatabaseUtility.TryAddPokemon(pokemon, out var error));
            logger.WriteLine($"Verify that error object is null");
            Assert.Null(error);
            logger.WriteLine($"Removing pokemon with pokemon id {pokemon.PokemonId}");
            DatabaseUtility.DeletePokemon(pokemon.PokemonId);
        }

        public static void PerformTryAddTrainerFailTest(TrainerModelv1 trainer, ITestOutputHelper logger)
        {
            logger.WriteLine($"Adding new trainer with trainer id {trainer.TrainerId}");
            Assert.False(DatabaseUtility.TryAddTrainer(trainer, out var error));
            logger.WriteLine($"Verify that error object is null");
            Assert.NotNull(error?.WriteErrorJsonString);
            logger.WriteLine($"Verify no trainer is found with trainer id {trainer.TrainerId}");
            //Assert.Null(DatabaseUtility.FindTrainerById(trainer.TrainerId));
        }

        public static void PerformTryAddTrainerPassTest(TrainerModelv1 trainer, ITestOutputHelper logger)
        {
            logger.WriteLine($"Adding new trainer with trainer id {trainer.TrainerId}");
            Assert.True(DatabaseUtility.TryAddTrainer(trainer, out var error));
            logger.WriteLine($"Verify that error object is null");
            Assert.Null(error);
            logger.WriteLine($"Removing trainer with trainer id {trainer.TrainerId}");
            DatabaseUtility.DeleteTrainer(trainer.GameId, trainer.TrainerId);
        }
    }
}
