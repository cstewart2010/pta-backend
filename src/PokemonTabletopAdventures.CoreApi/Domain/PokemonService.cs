using MongoDB.Driver;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Domain.Handlers;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Pokemons;

namespace PokemonTabletopAdventures.CoreApi.Domain;

internal class PokemonService(IPokedexService pokedexService) : AbstractService<PokemonDto>(MongoCollection.Pokemon), IPokemonService
{
    private readonly IPokedexService _pokedexService = pokedexService;

    public async Task DeletePokemonByTrainerId(Guid gameId, Guid trainerId)
    {
        var result = Collection.DeleteMany(pokemon => pokemon.TrainerId == trainerId && pokemon.GameId == gameId);
        await _pokedexService.DeleteDexItemForTrainer(trainerId, gameId);
        await Task.CompletedTask;
    }

    public async Task<Pokemon> GetPokemonById(Guid id)
    {
        var dto = await ThrowIfNull(
            id,
            id => Collection.Find(pokemon => pokemon.PokemonId == id).SingleOrDefault(),
            PropertyNames.PokemonId);

        return DtoHandler.ParseFromDto(dto);
    }

    public async Task<IEnumerable<Pokemon>> GetPokemonByTrainerId(Guid trainerId)
    {
        var dtos = await ThrowIfNull(
            trainerId,
            id => Collection.Find(pokemon => pokemon.TrainerId == id).ToEnumerable(),
            PropertyNames.TrainerId);

        return dtos.Select(DtoHandler.ParseFromDto);
    }

    public async Task<IEnumerable<Pokemon>> GetPokemonByTrainerId(Guid trainerId, Guid gameId)
    {
        var dtos = await ThrowIfNull(
            gameId,
            id => Collection.Find(pokemon => pokemon.TrainerId == trainerId && pokemon.GameId == id).ToEnumerable(),
            PropertyNames.GameId);

        return dtos.Select(DtoHandler.ParseFromDto);
    }

    public async Task PostPokemon(Pokemon pokemon)
    {
        var dto = DtoHandler.ParseFromModel(pokemon);
        await PostDocument(dto);
    }

    public async Task<Pokemon> UpdatePokemon(Pokemon updatePokemon)
    {
        var dto = DtoHandler.ParseFromModel(updatePokemon);
        await UpsertDocument(
            Builders<PokemonDto>.Filter.Eq(pokemon => pokemon.PokemonId, updatePokemon.PokemonId),
            dto);

        return await GetPokemonById(updatePokemon.PokemonId);
    }

    public async Task<Pokemon> UpdatePokemonEvolvability(Guid pokemonId, bool isEvolvable)
    {
        var dto = await UpdateDocument(
            pokemonId,
            pokemon => pokemon.PokemonId == pokemonId,
            Builders<PokemonDto>.Update.Set(PropertyNames.CanEvolve, isEvolvable));

        return DtoHandler.ParseFromDto(dto);
    }

    public async Task<Pokemon> UpdatePokemonHP(Guid pokemonId, int hp)
    {
        var dto = await UpdateDocument(
            pokemonId,
            pokemon => pokemon.PokemonId == pokemonId,
            Builders<PokemonDto>.Update.Set(PropertyNames.CurrentHP, hp));

        return DtoHandler.ParseFromDto(dto);
    }

    public async Task<Pokemon> UpdatePokemonLocation(Guid pokemonId, bool isOnActiveTeam)
    {
        var dto = await UpdateDocument(
            pokemonId,
            pokemon => pokemon.PokemonId == pokemonId,
            Builders<PokemonDto>.Update.Set(PropertyNames.IsOnActiveTeam, isOnActiveTeam));

        return DtoHandler.ParseFromDto(dto);
    }

    public async Task<Pokemon> UpdatePokemonTrainerId(Guid pokemonId, Guid trainerId)
    {
        var dto = await UpdateDocument(
            pokemonId,
            pokemon => pokemon.PokemonId == pokemonId,
            Builders<PokemonDto>.Update.Set(PropertyNames.TrainerId, trainerId));

        return DtoHandler.ParseFromDto(dto);
    }

    public async Task DeletePokemon(Guid id)
    {
        await ThrowIfNull(
            id,
            pokemonId => Collection.FindOneAndDelete(pokemon => pokemon.PokemonId == pokemonId),
            PropertyNames.PokemonId);
    }
}
