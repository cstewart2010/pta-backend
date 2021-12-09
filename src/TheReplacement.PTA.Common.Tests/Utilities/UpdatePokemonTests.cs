using MongoDB.Driver;
using System;
using TheReplacement.PTA.Common.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace TheReplacement.PTA.Common.Tests.Utilities
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
            Logger.WriteLine($"Adding pokemon id {flabebe.PokemonId}");
            DatabaseUtility.TryAddPokemon(flabebe, out _);

            Logger.WriteLine($"Updating pokemon id {flabebe.PokemonId} by evolving it to Floette");
            var floette = DexUtility.GetEvolved(flabebe, flabebe.Moves, "Floette", Array.Empty<string>());
            Assert.True(DatabaseUtility.UpdatePokemonWithEvolution(flabebe.PokemonId, floette));

            Logger.WriteLine($"Verify pokemon id {flabebe.PokemonId} evolved to floette");
            DatabaseUtility.DeletePokemon(flabebe.PokemonId);
            Assert.Equal(297, floette.DexNo);
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
