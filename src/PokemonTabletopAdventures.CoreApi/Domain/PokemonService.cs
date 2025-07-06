using MongoDB.Driver;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.Domain;

internal class PokemonService(IPokedexService pokedexService) : AbstractService<PokemonModel>(MongoCollection.Pokemon), IPokemonService
{
    private readonly IPokedexService _pokedexService = pokedexService;

    public async Task DeletePokemonByTrainerId(Guid gameId, Guid trainerId)
    {
        var result = Collection.DeleteMany(pokemon => pokemon.TrainerId == trainerId && pokemon.GameId == gameId);
        await _pokedexService.DeleteDexItemForTrainer(trainerId, gameId);
        await Task.CompletedTask;
    }

    public async Task<PokemonModel> GetPokemonById(Guid id)
    {
        return await ThrowIfNull(
            id,
            id => Collection.Find(pokemon => pokemon.PokemonId == id).SingleOrDefault(),
            PropertyNames.PokemonId);
    }

    public async Task<IEnumerable<PokemonModel>> GetPokemonByTrainerId(Guid trainerId)
    {
        return await ThrowIfNull(
            trainerId,
            id => Collection.Find(pokemon => pokemon.TrainerId == id).ToEnumerable(),
            PropertyNames.TrainerId);
    }

    public async Task<IEnumerable<PokemonModel>> GetPokemonByTrainerId(Guid trainerId, Guid gameId)
    {
        return await ThrowIfNull(
            gameId,
            id => Collection.Find(pokemon => pokemon.TrainerId == trainerId && pokemon.GameId == id).ToEnumerable(),
            PropertyNames.GameId);
    }

    public async Task PostPokemon(PokemonModel pokemon)
    {
        await PostDocument(pokemon);
    }

    public async Task<PokemonModel> UpdatePokemon(PokemonModel updatePokemon)
    {
        await UpsertDocument(
            Builders<PokemonModel>.Filter.Eq(pokemon => pokemon.PokemonId, updatePokemon.PokemonId),
            updatePokemon);

        return updatePokemon;
    }

    public async Task<PokemonModel> UpdatePokemonEvolvability(Guid pokemonId, bool isEvolvable)
    {
        return await UpdateDocument(
            pokemonId,
            pokemon => pokemon.PokemonId == pokemonId,
            Builders<PokemonModel>.Update.Set(PropertyNames.CanEvolve, isEvolvable));
    }

    public async Task<PokemonModel> UpdatePokemonHP(Guid pokemonId, int hp)
    {
        return await UpdateDocument(
            pokemonId,
            pokemon => pokemon.PokemonId == pokemonId,
            Builders<PokemonModel>.Update.Set(PropertyNames.CurrentHP, hp));
    }

    public async Task<PokemonModel> UpdatePokemonLocation(Guid pokemonId, bool isOnActiveTeam)
    {
        return await UpdateDocument(
            pokemonId,
            pokemon => pokemon.PokemonId == pokemonId,
            Builders<PokemonModel>.Update.Set(PropertyNames.IsOnActiveTeam, isOnActiveTeam));
    }

    public async Task<PokemonModel> UpdatePokemonTrainerId(Guid pokemonId, Guid trainerId)
    {
        return await UpdateDocument(
            pokemonId,
            pokemon => pokemon.PokemonId == pokemonId,
            Builders<PokemonModel>.Update.Set(PropertyNames.TrainerId, trainerId));
    }

    public async Task DeletePokemon(Guid id)
    {
        await ThrowIfNull(
            id,
            pokemonId => Collection.FindOneAndDelete(pokemon => pokemon.PokemonId == pokemonId),
            PropertyNames.PokemonId);
    }
}
