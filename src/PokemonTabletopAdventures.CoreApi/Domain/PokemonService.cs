using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Pokemons;

namespace PokemonTabletopAdventures.CoreApi.Domain;

public class PokemonService(
    IRepositoryService repositoryService,
    IPokedexService pokedexService,
    IDtoToModelMapper dtoToModelMapper,
    IModelToDtoMapper modelToDtoMapper,
    ILogger<PokemonService> logger) : AbstractMongoService<PokemonDto>(repositoryService, MongoCollection.Pokemon), IPokemonService
{
    public async Task DeletePokemonByTrainerId(Guid gameId, Guid trainerId)
    {
        await Collection.DeleteManyAsync(pokemon => pokemon.TrainerId == trainerId && pokemon.GameId == gameId);
        await pokedexService.DeleteTrainerDex(trainerId, gameId);
    }

    public async Task<Pokemon> GetPokemonById(Guid id)
    {
        logger.LogInformation("Getting pokemon for id {id}", id);
        var dto = await ThrowIfNull(
            id,
            id => Collection.GetOneAsync(pokemon => pokemon.PokemonId == id),
            PropertyNames.PokemonId);

        return await dtoToModelMapper.ParseFromDto(dto);
    }

    public async Task<ICollection<Pokemon>> GetPokemonByTrainerId(Guid trainerId, Guid gameId)
    {
        var dtos = await Collection.GetManyAsync(pokemon => pokemon.TrainerId == trainerId && pokemon.GameId == gameId);
        return await Task.WhenAll(dtos.Select(dtoToModelMapper.ParseFromDto));
    }

    public async Task PostPokemon(Pokemon pokemon)
    {
        var dto = await modelToDtoMapper.ParseFromModel(pokemon);
        await PostUniqueDocument(dto, x => x.PokemonId == pokemon.PokemonId);
    }

    public async Task<Pokemon> UpdatePokemon(Pokemon updatePokemon)
    {
        var dto = await modelToDtoMapper.ParseFromModel(updatePokemon);
        await UpsertDocument(
            pokemon => pokemon.PokemonId,
            updatePokemon.PokemonId,
            dto);

        return await GetPokemonById(updatePokemon.PokemonId);
    }

    public async Task<Pokemon> UpdatePokemonEvolvability(Guid pokemonId, bool isEvolvable)
    {
        var dto = await UpdateDocument(
            pokemonId,
            pokemon => pokemon.PokemonId == pokemonId,
            new Models.UpdateData(PropertyNames.CanEvolve, isEvolvable));

        return await dtoToModelMapper.ParseFromDto(dto);
    }

    public async Task<Pokemon> UpdatePokemonHP(Guid pokemonId, int hp)
    {
        var dto = await UpdateDocument(
            pokemonId,
            pokemon => pokemon.PokemonId == pokemonId,
            new Models.UpdateData(PropertyNames.CurrentHP, hp));

        return await dtoToModelMapper.ParseFromDto(dto);
    }

    public async Task<Pokemon> UpdatePokemonLocation(Guid pokemonId, bool isOnActiveTeam)
    {
        var dto = await UpdateDocument(
            pokemonId,
            pokemon => pokemon.PokemonId == pokemonId,
            new Models.UpdateData(PropertyNames.IsOnActiveTeam, isOnActiveTeam));

        return await dtoToModelMapper.ParseFromDto(dto);
    }

    public async Task<Pokemon> UpdatePokemonTrainerId(Guid pokemonId, Guid trainerId)
    {
        var dto = await UpdateDocument(
            pokemonId,
            pokemon => pokemon.PokemonId == pokemonId,
            new Models.UpdateData(PropertyNames.TrainerId, trainerId));

        return await dtoToModelMapper.ParseFromDto(dto);
    }

    public async Task DeletePokemon(Guid id)
    {
        await ThrowIfNull(
            id,
            pokemonId => Collection.DeleteAsync(pokemon => pokemon.PokemonId == pokemonId),
            PropertyNames.PokemonId);
    }
}
