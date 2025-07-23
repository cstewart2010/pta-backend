using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using PokemonTabletopAdventures.Models.Pokemons;

namespace PokemonTabletopAdventures.CoreApi.Domain;

public class PokemonService(
    IRepositoryService repositoryService,
    IPokedexService pokedexService,
    IDtoToModelMapper dtoToModelMapper,
    IModelToDtoMapper modelToDtoMapper) : AbstractMongoService<PokemonDto>(repositoryService, MongoCollection.Pokemon), IPokemonService
{
    private readonly IPokedexService _pokedexService = pokedexService;
    private readonly IDtoToModelMapper _dtoToModelMapper = dtoToModelMapper;
    private readonly IModelToDtoMapper _modelToDtoMapper = modelToDtoMapper;

    public async Task DeletePokemonByTrainerId(Guid gameId, Guid trainerId)
    {
        var result = Collection.DeleteManyAsync(pokemon => pokemon.TrainerId == trainerId && pokemon.GameId == gameId);
        await _pokedexService.DeleteDexItemForTrainer(trainerId, gameId);
        await Task.CompletedTask;
    }

    public async Task<Pokemon> GetPokemonById(Guid id)
    {
        var dto = await ThrowIfNull(
            id,
            id => Collection.GetOneAsync(pokemon => pokemon.PokemonId == id),
            PropertyNames.PokemonId);

        return await _dtoToModelMapper.ParseFromDto(dto);
    }

    public async Task<IEnumerable<Pokemon>> GetPokemonByTrainerId(Guid trainerId)
    {
        var dtos = await ThrowIfNull(
            trainerId,
            id => Collection.GetManyAsync(pokemon => pokemon.TrainerId == id),
            PropertyNames.TrainerId);

        return await Task.WhenAll(dtos.Select(_dtoToModelMapper.ParseFromDto));
    }

    public async Task<IEnumerable<Pokemon>> GetPokemonByTrainerId(Guid trainerId, Guid gameId)
    {
        var dtos = await ThrowIfNull(
            gameId,
            id => Collection.GetManyAsync(pokemon => pokemon.TrainerId == trainerId && pokemon.GameId == id),
            PropertyNames.GameId);

        return await Task.WhenAll(dtos.Select(_dtoToModelMapper.ParseFromDto));
    }

    public async Task PostPokemon(Pokemon pokemon)
    {
        var dto = await _modelToDtoMapper.ParseFromModel(pokemon);
        await PostDocument(dto);
    }

    public async Task<Pokemon> UpdatePokemon(Pokemon updatePokemon)
    {
        var dto = await _modelToDtoMapper.ParseFromModel(updatePokemon);
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

        return await _dtoToModelMapper.ParseFromDto(dto);
    }

    public async Task<Pokemon> UpdatePokemonHP(Guid pokemonId, int hp)
    {
        var dto = await UpdateDocument(
            pokemonId,
            pokemon => pokemon.PokemonId == pokemonId,
            new Models.UpdateData(PropertyNames.CurrentHP, hp));

        return await _dtoToModelMapper.ParseFromDto(dto);
    }

    public async Task<Pokemon> UpdatePokemonLocation(Guid pokemonId, bool isOnActiveTeam)
    {
        var dto = await UpdateDocument(
            pokemonId,
            pokemon => pokemon.PokemonId == pokemonId,
            new Models.UpdateData(PropertyNames.IsOnActiveTeam, isOnActiveTeam));

        return await _dtoToModelMapper.ParseFromDto(dto);
    }

    public async Task<Pokemon> UpdatePokemonTrainerId(Guid pokemonId, Guid trainerId)
    {
        var dto = await UpdateDocument(
            pokemonId,
            pokemon => pokemon.PokemonId == pokemonId,
            new Models.UpdateData(PropertyNames.TrainerId, trainerId));

        return await _dtoToModelMapper.ParseFromDto(dto);
    }

    public async Task DeletePokemon(Guid id)
    {
        await ThrowIfNull(
            id,
            pokemonId => Collection.DeleteAsync(pokemon => pokemon.PokemonId == pokemonId),
            PropertyNames.PokemonId);
    }
}
