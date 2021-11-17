using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace TheReplacements.PTA.Common.Utilities.Tests
{
    public class UpdatePokemonTests : TestsBase
    {
        private readonly ITestOutputHelper _logger;
        public override ITestOutputHelper Logger { get => _logger; }

        public UpdatePokemonTests(ITestOutputHelper logger)
        {
            _logger = logger;
        }

        [Fact, Trait("Category", "smoke")]
        public void UpdatePokemonStats_SmokeTest_True()
        {
            var pokemon = GetTestPokemon();
            var query = new Dictionary<string, string>
            {
                { "experience", "2000" },
                { "hpAdded", "8" },
                { "attackAdded", "9" },
                { "defenseAdded", "10" },
                { "specialAttackAdded", "11" },
                { "specialDefenseAdded", "12" },
                { "speedAdded", "13" },
                { "nickname", "UpdateTest" }
            };

            Logger.WriteLine($"Adding pokemon id {pokemon.PokemonId}");
            DatabaseUtility.TryAddPokemon(pokemon, out _);

            Logger.WriteLine($"Updating pokemon id {pokemon.PokemonId} with {query.Aggregate("", (prev, curr) => $"{curr.Key}: {curr.Value}\n")}");
            Assert.True(DatabaseUtility.UpdatePokemonStats(pokemon.PokemonId, query));
            var updatedPokemon = DatabaseUtility.FindPokemonById(pokemon.PokemonId);
            DatabaseUtility.DeletePokemon(pokemon.PokemonId);
            
            Logger.WriteLine($"Verify pokemon id {pokemon.PokemonId} was updated properly");
            Assert.Equal(2000, updatedPokemon.Experience);
            Assert.Equal(8, updatedPokemon.HP.Added);
            Assert.Equal(9, updatedPokemon.Attack.Added);
            Assert.Equal(10, updatedPokemon.Defense.Added);
            Assert.Equal(11, updatedPokemon.SpecialAttack.Added);
            Assert.Equal(12, updatedPokemon.SpecialDefense.Added);
            Assert.Equal(13, updatedPokemon.Speed.Added);
            Assert.Equal("UpdateTest", updatedPokemon.Nickname);
        }

        [Fact]
        public void UpdatePokemonStats_PartialUpdates_True()
        {
            var pokemon = GetTestPokemon();
            var query = new Dictionary<string, string>
            {
                { "experience", "2000" },
                { "speedAdded", "13" },
                { "nickname", "UpdateTest" }
            };

            Logger.WriteLine($"Adding pokemon id {pokemon.PokemonId}");
            DatabaseUtility.TryAddPokemon(pokemon, out _);

            Logger.WriteLine($"Updating pokemon id {pokemon.PokemonId} with {query.Aggregate("", (prev, curr) => $"{curr.Key}: {curr.Value}\n")}");
            Assert.True(DatabaseUtility.UpdatePokemonStats(pokemon.PokemonId, query));
            var updatedPokemon = DatabaseUtility.FindPokemonById(pokemon.PokemonId);
            DatabaseUtility.DeletePokemon(pokemon.PokemonId);

            Logger.WriteLine($"Verify pokemon id {pokemon.PokemonId} was updated properly");
            Assert.Equal(2000, updatedPokemon.Experience);
            Assert.Equal(pokemon.HP.Added, updatedPokemon.HP.Added);
            Assert.Equal(pokemon.Attack.Added, updatedPokemon.Attack.Added);
            Assert.Equal(pokemon.Defense.Added, updatedPokemon.Defense.Added);
            Assert.Equal(pokemon.SpecialAttack.Added, updatedPokemon.SpecialAttack.Added);
            Assert.Equal(pokemon.SpecialDefense.Added, updatedPokemon.SpecialDefense.Added);
            Assert.Equal(13, updatedPokemon.Speed.Added);
            Assert.Equal("UpdateTest", updatedPokemon.Nickname);
        }

        [Fact]
        public void UpdatePokemonStats_PartialUpdatesWithInvalidData_True()
        {
            var pokemon = GetTestPokemon();
            var query = new Dictionary<string, string>
            {
                { "experience", "2000" },
                { "invalidData", "something wrong" },
                { "speedAdded", "13" },
                { "nickname", "UpdateTest" }
            };

            Logger.WriteLine($"Adding pokemon id {pokemon.PokemonId}");
            DatabaseUtility.TryAddPokemon(pokemon, out _);

            Logger.WriteLine($"Updating pokemon id {pokemon.PokemonId} with {query.Aggregate("", (prev, curr) => $"{curr.Key}: {curr.Value}\n")}");
            Assert.True(DatabaseUtility.UpdatePokemonStats(pokemon.PokemonId, query));
            var updatedPokemon = DatabaseUtility.FindPokemonById(pokemon.PokemonId);
            DatabaseUtility.DeletePokemon(pokemon.PokemonId);

            Logger.WriteLine($"Verify pokemon id {pokemon.PokemonId} was updated properly");
            Assert.Equal(2000, updatedPokemon.Experience);
            Assert.Equal(pokemon.HP.Added, updatedPokemon.HP.Added);
            Assert.Equal(pokemon.Attack.Added, updatedPokemon.Attack.Added);
            Assert.Equal(pokemon.Defense.Added, updatedPokemon.Defense.Added);
            Assert.Equal(pokemon.SpecialAttack.Added, updatedPokemon.SpecialAttack.Added);
            Assert.Equal(pokemon.SpecialDefense.Added, updatedPokemon.SpecialDefense.Added);
            Assert.Equal(13, updatedPokemon.Speed.Added);
            Assert.Equal("UpdateTest", updatedPokemon.Nickname);
        }

        [Fact]
        public void UpdatePokemonStats_InvalidUpdates_ThrowsArgumentNullException()
        {
            var pokemon = GetTestPokemon();
            var query = new Dictionary<string, string>
            {
                { "invalidData", "something wrong" }
            };

            Logger.WriteLine($"Adding pokemon id {pokemon.PokemonId}");
            DatabaseUtility.TryAddPokemon(pokemon, out _);

            Logger.WriteLine($"Updating pokemon id {pokemon.PokemonId} with {query.Aggregate("", (prev, curr) => $"{curr.Key}: {curr.Value}\n")}");
            Assert.Throws<ArgumentNullException>(() => DatabaseUtility.UpdatePokemonStats(pokemon.PokemonId, query));
            var updatedPokemon = DatabaseUtility.FindPokemonById(pokemon.PokemonId);
            DatabaseUtility.DeletePokemon(pokemon.PokemonId);

            Logger.WriteLine($"Verify pokemon id {pokemon.PokemonId} was not updated");
            Assert.Equal(pokemon.Experience, updatedPokemon.Experience);
            Assert.Equal(pokemon.HP.Added, updatedPokemon.HP.Added);
            Assert.Equal(pokemon.Attack.Added, updatedPokemon.Attack.Added);
            Assert.Equal(pokemon.Defense.Added, updatedPokemon.Defense.Added);
            Assert.Equal(pokemon.SpecialAttack.Added, updatedPokemon.SpecialAttack.Added);
            Assert.Equal(pokemon.SpecialDefense.Added, updatedPokemon.SpecialDefense.Added);
            Assert.Equal(pokemon.Speed.Added, updatedPokemon.Speed.Added);
            Assert.Equal(pokemon.Nickname, updatedPokemon.Nickname);
        }

        [Fact]
        public void UpdatePokemonStats_ValidKeyInvalidValue_ThrowsMongoCommandException()
        {
            var pokemon = GetTestPokemon();
            var query = new Dictionary<string, string>
            {
                { "nickname", "something very very very very long" }
            };

            Logger.WriteLine($"Adding pokemon id {pokemon.PokemonId}");
            DatabaseUtility.TryAddPokemon(pokemon, out _);

            Logger.WriteLine($"Updating pokemon id {pokemon.PokemonId} with {query.Aggregate("", (prev, curr) => $"{curr.Key}: {curr.Value}\n")}");
            Assert.Throws<MongoCommandException>(() => DatabaseUtility.UpdatePokemonStats(pokemon.PokemonId, query));
            var updatedPokemon = DatabaseUtility.FindPokemonById(pokemon.PokemonId);
            DatabaseUtility.DeletePokemon(pokemon.PokemonId);

            Logger.WriteLine($"Verify pokemon id {pokemon.PokemonId} was not updated");
            Assert.Equal(pokemon.Experience, updatedPokemon.Experience);
            Assert.Equal(pokemon.HP.Added, updatedPokemon.HP.Added);
            Assert.Equal(pokemon.Attack.Added, updatedPokemon.Attack.Added);
            Assert.Equal(pokemon.Defense.Added, updatedPokemon.Defense.Added);
            Assert.Equal(pokemon.SpecialAttack.Added, updatedPokemon.SpecialAttack.Added);
            Assert.Equal(pokemon.SpecialDefense.Added, updatedPokemon.SpecialDefense.Added);
            Assert.Equal(pokemon.Speed.Added, updatedPokemon.Speed.Added);
            Assert.Equal(pokemon.Nickname, updatedPokemon.Nickname);
        }

        [Fact]
        public void UpdatePokemonStats_NullUpdates_ThrowsArgumentNullException()
        {
            var pokemon = GetTestPokemon();
            Logger.WriteLine($"Adding pokemon id {pokemon.PokemonId}");
            DatabaseUtility.TryAddPokemon(pokemon, out _);

            Logger.WriteLine($"Updating pokemon id {pokemon.PokemonId} with a null reference");
            Assert.Throws<ArgumentNullException>(() => DatabaseUtility.UpdatePokemonStats(pokemon.PokemonId, null));
            var updatedPokemon = DatabaseUtility.FindPokemonById(pokemon.PokemonId);
            DatabaseUtility.DeletePokemon(pokemon.PokemonId);

            Logger.WriteLine($"Verify pokemon id {pokemon.PokemonId} was not updated");
            Assert.Equal(pokemon.Experience, updatedPokemon.Experience);
            Assert.Equal(pokemon.HP.Added, updatedPokemon.HP.Added);
            Assert.Equal(pokemon.Attack.Added, updatedPokemon.Attack.Added);
            Assert.Equal(pokemon.Defense.Added, updatedPokemon.Defense.Added);
            Assert.Equal(pokemon.SpecialAttack.Added, updatedPokemon.SpecialAttack.Added);
            Assert.Equal(pokemon.SpecialDefense.Added, updatedPokemon.SpecialDefense.Added);
            Assert.Equal(pokemon.Speed.Added, updatedPokemon.Speed.Added);
            Assert.Equal(pokemon.Nickname, updatedPokemon.Nickname);
        }

        [Fact, Trait("Category", "smoke")]
        public void UpdatePokemonTrainerId_SmokeTest_True()
        {
            var pokemon = GetTestPokemon();
            var newTrainer = GetTestTrainer();
            Logger.WriteLine($"Adding pokemon id {pokemon.PokemonId}");
            DatabaseUtility.TryAddPokemon(pokemon, out _);

            Logger.WriteLine($"Updating pokemon id {pokemon.PokemonId} with trainer id {newTrainer.TrainerId}");
            Assert.True(DatabaseUtility.UpdatePokemonTrainerId(pokemon.PokemonId, newTrainer.TrainerId));
            var updatedPokemon = DatabaseUtility.FindPokemonById(pokemon.PokemonId);
            DatabaseUtility.DeletePokemon(pokemon.PokemonId);

            Logger.WriteLine($"Verify pokemon id was updated with trainer id {newTrainer.TrainerId}");
            Assert.Equal(newTrainer.TrainerId, updatedPokemon.TrainerId);
        }

        [Theory]
        [InlineData("")]
        [InlineData("invalid id")]
        public void UpdatePokemonTrainerId_InvalidTrainerId_ThrowsMongoCommandException(string trainerId)
        {
            var pokemon = GetTestPokemon();
            Logger.WriteLine($"Adding pokemon id {pokemon.PokemonId}");
            DatabaseUtility.TryAddPokemon(pokemon, out _);

            Logger.WriteLine($"Updating pokemon id {pokemon.PokemonId} with trainer id {trainerId}");
            Assert.Throws<MongoCommandException>(() => DatabaseUtility.UpdatePokemonTrainerId(pokemon.PokemonId, trainerId));
            var updatedPokemon = DatabaseUtility.FindPokemonById(pokemon.PokemonId);
            DatabaseUtility.DeletePokemon(pokemon.PokemonId);

            Logger.WriteLine($"Verify pokemon id was not updated with trainer id {trainerId}");
            Assert.Equal(pokemon.TrainerId, updatedPokemon.TrainerId);
        }

        [Fact, Trait("Category", "smoke")]
        public void UpdatePokemonWithEvolution_SmokeTest_True()
        {
            var flabebe = GetTestPokemon();
            flabebe.DexNo = 669;
            Logger.WriteLine($"Adding pokemon id {flabebe.PokemonId}");
            DatabaseUtility.TryAddPokemon(flabebe, out _);

            Logger.WriteLine($"Updating pokemon id {flabebe.PokemonId} by evolving it to Floette");
            var floette = PokeAPIUtility.GetEvolved(DatabaseUtility.FindPokemonById(flabebe.PokemonId), "Floette");
            Assert.True(DatabaseUtility.UpdatePokemonWithEvolution(flabebe.PokemonId, floette));

            Logger.WriteLine($"Verify pokemon id {flabebe.PokemonId} evolved to floette");
            DatabaseUtility.DeletePokemon(flabebe.PokemonId);
            Assert.Equal(670, floette.DexNo);
            Assert.Equal(flabebe.PokemonId, floette.PokemonId);
        }

        [Fact]
        public void UpdatePokemonWithEvolution_NullEvolution_ThrowsArgumentNullException()
        {
            var flabebe = GetTestPokemon();
            flabebe.DexNo = 669;
            Logger.WriteLine($"Adding pokemon id {flabebe.PokemonId}");
            DatabaseUtility.TryAddPokemon(flabebe, out _);

            Logger.WriteLine($"Updating pokemon id {flabebe.PokemonId} with null evolution");
            Assert.Throws<ArgumentNullException>(() => DatabaseUtility.UpdatePokemonWithEvolution(flabebe.PokemonId, null));
            DatabaseUtility.DeletePokemon(flabebe.PokemonId);
        }
    }
}
