using System;
using System.Collections.Generic;
using TheReplacements.PTA.Common.Enums;
using TheReplacements.PTA.Common.Models;
using Xunit;
using Xunit.Abstractions;

namespace TheReplacements.PTA.Common.Utilities
{
    public abstract class TestsBase
    {
        public abstract ITestOutputHelper Logger { get; }

        public static GameModel GetTestGame()
        {
            return new GameModel
            {
                GameId = Guid.NewGuid().ToString(),
                Nickname = "Test Nickname",
                NPCs = Array.Empty<string>(),
                PasswordHash = "testpassword"
            };
        }

        public static NpcModel GetTestNpc()
        {
            return new NpcModel
            {
                NPCId = Guid.NewGuid().ToString(),
                Feats = Array.Empty<string>(),
                TrainerClasses = Array.Empty<string>(),
                TrainerName = "Test Trainername",
                TrainerStats = new TrainerStatsModel()
            };
        }

        public static PokemonModel GetTestPokemon()
        {
            return new PokemonModel
            {
                HP = new PokemonStatModel(),
                Attack = new PokemonStatModel(),
                Defense = new PokemonStatModel(),
                SpecialAttack = new PokemonStatModel(),
                SpecialDefense = new PokemonStatModel(),
                Speed = new PokemonStatModel(),
                Ability = 1,
                CatchRate = 180,
                DexNo = 143,
                Experience = 1000,
                ExpYield = 35,
                Gender = Gender.Male,
                IsShiny = false,
                Level = 8,
                NaturalMoves = new[]
{
                    "A Move"
                },
                Nature = 18,
                Nickname = "Snorlax",
                PokemonId = Guid.NewGuid().ToString(),
                TMMoves = Array.Empty<string>(),
                TrainerId = Guid.NewGuid().ToString(),
                Type = (int)PokemonTypes.Normal
            };
        }

        public static TrainerModel GetTestTrainer()
        {
            return new TrainerModel
            {
                Feats = Array.Empty<string>(),
                GameId = Guid.NewGuid().ToString(),
                Items = new List<ItemModel>(),
                PasswordHash = "testpassword",
                TrainerClasses = Array.Empty<string>(),
                TrainerId = Guid.NewGuid().ToString(),
                TrainerName = "Test Trainer",
                TrainerStats = new TrainerStatsModel()
            };
        }

        public static void PerformTryAddGamePassTest(GameModel game, ITestOutputHelper logger)
        {
            logger.WriteLine($"Adding new game with game id {game.GameId}");
            Assert.True(DatabaseUtility.TryAddGame(game, out var error));
            logger.WriteLine($"Verify that error object is null");
            Assert.Null(error);
            logger.WriteLine($"Removing game with game id {game.GameId}");
            DatabaseUtility.DeleteGame(game.GameId);
        }

        public static void PerformTryAddGameFailTest(GameModel game, ITestOutputHelper logger)
        {
            logger.WriteLine($"Adding new game with game id {game.GameId}");
            Assert.False(DatabaseUtility.TryAddGame(game, out var error));
            logger.WriteLine($"Verify that error object is null");
            Assert.NotNull(error?.WriteErrorJsonString);
            logger.WriteLine($"Verify no game is found with game id {game.GameId}");
            Assert.Null(DatabaseUtility.FindGame(game.GameId));
        }

        public static void PerformTryAddNpcPassTest(NpcModel npc, ITestOutputHelper logger)
        {
            logger.WriteLine($"Adding new game with npc id {npc.NPCId}");
            Assert.True(DatabaseUtility.TryAddNpc(npc, out var error));
            logger.WriteLine($"Verify that error object is null");
            Assert.Null(error);
            logger.WriteLine($"Removing game with game id {npc.NPCId}");
            DatabaseUtility.DeleteNpc(npc.NPCId);
        }

        public static void PerformTryAddNpcFailTest(NpcModel npc, ITestOutputHelper logger)
        {
            logger.WriteLine($"Adding new npc with npc id {npc.NPCId}");
            Assert.False(DatabaseUtility.TryAddNpc(npc, out var error));
            logger.WriteLine($"Verify that error object is null");
            Assert.NotNull(error?.WriteErrorJsonString);
            logger.WriteLine($"Verify no npc is found with npc id {npc.NPCId}");
            Assert.Empty(DatabaseUtility.FindNpcs(new[] { npc.NPCId }));
        }

        public static void PerformTryAddPokemonFailTest(PokemonModel pokemon, ITestOutputHelper logger)
        {
            logger.WriteLine($"Adding new pokemon with pokemon id {pokemon.PokemonId}");
            Assert.False(DatabaseUtility.TryAddPokemon(pokemon, out var error));
            logger.WriteLine($"Verify that error object is null");
            Assert.NotNull(error?.WriteErrorJsonString);
            logger.WriteLine($"Verify no pokemon is found with pokemon id {pokemon.PokemonId}");
            Assert.Null(DatabaseUtility.FindPokemonById(pokemon.PokemonId));
        }

        public static void PerformTryAddPokemonPassTest(PokemonModel pokemon, ITestOutputHelper logger)
        {
            logger.WriteLine($"Adding new pokemon with pokemon id {pokemon.PokemonId}");
            Assert.True(DatabaseUtility.TryAddPokemon(pokemon, out var error));
            logger.WriteLine($"Verify that error object is null");
            Assert.Null(error);
            logger.WriteLine($"Removing pokemon with pokemon id {pokemon.PokemonId}");
            DatabaseUtility.DeletePokemon(pokemon.PokemonId);
        }

        public static void PerformTryAddTrainerFailTest(TrainerModel trainer, ITestOutputHelper logger)
        {
            logger.WriteLine($"Adding new pokemon with pokemon id {trainer.TrainerId}");
            Assert.False(DatabaseUtility.TryAddTrainer(trainer, out var error));
            logger.WriteLine($"Verify that error object is null");
            Assert.NotNull(error?.WriteErrorJsonString);
            logger.WriteLine($"Verify no pokemon is found with pokemon id {trainer.TrainerId}");
            Assert.Null(DatabaseUtility.FindTrainerById(trainer.TrainerId));
        }

        public static void PerformTryAddTrainerPassTest(TrainerModel trainer, ITestOutputHelper logger)
        {
            logger.WriteLine($"Adding new pokemon with pokemon id {trainer.TrainerId}");
            Assert.True(DatabaseUtility.TryAddTrainer(trainer, out var error));
            logger.WriteLine($"Verify that error object is null");
            Assert.Null(error);
            logger.WriteLine($"Removing pokemon with pokemon id {trainer.TrainerId}");
            DatabaseUtility.DeleteTrainer(trainer.TrainerId);
        }
    }
}
