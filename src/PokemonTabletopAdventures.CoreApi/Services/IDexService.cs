using PokemonTabletopAdventures.CoreApi.DTOs.Indicies;
using PokemonTabletopAdventures.Models.Interfaces;
using PokemonTabletopAdventures.Models;
using PokemonTabletopAdventures.Models.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.Services;

public interface IDexService
{
    /// <summary>
    /// Returns a specific Dex entry from a specific Dex collection
    /// </summary>
    /// <param name="documentType">The dex collection you wish to return data from</param>
    /// <param name="name">The name of the dex entry</param>
    public Task<IndexResponse<TDocument>> GetDexEntry<TDocument>(
        DexType documentType,
        string name) where TDocument : IDexDocument;

    /// <summary>
    /// Returns all Dex extries for a specific Dex collection
    /// </summary>
    /// <param name="documentType">The dex collection you wish to return data from</param>
    public Task<IEnumerable<TDocument>> GetDexEntries<TDocument>(DexType documentType) where TDocument : IDexDocument;

    /// <summary>
    /// Adds a collection of dex entry to a specific dex collection
    /// </summary>
    /// <param name="collectionName">The name of the collection to add document</param>
    /// <param name="documents">The documents to add to collection</param>
    public Task PostDexEntries<TDocument>(
        string collectionName,
        IEnumerable<TDocument> documents) where TDocument : IDexDocument;

    /// <summary>
    /// Attempts to evolve a pokemon to its next stage
    /// </summary>
    /// <param name="pokemon">The current form</param>
    /// <param name="keptMoves">The moves you wish to keep</param>
    /// <param name="evolvedName">The name of the evolved form</param>
    /// <param name="newMoves">The moves you wish to add</param>
    public Task<PokemonModel> GetEvolved(
        PokemonModel pokemon,
        IEnumerable<string> keptMoves,
        string evolvedName,
        IEnumerable<string> newMoves);

    public Task<IndexCollectionResponse> GetIndexCollectionResponse<TDocument>(
        DexType documentType,
        int offset,
        int limit) where TDocument : IDexDocument;

    /// <summary>
    /// Returns a collection of possible evolutions
    /// </summary>
    /// <param name="pokemon"></param>
    public Task<IEnumerable<BasePokemonModel>> GetPossibleEvolutions(PokemonModel pokemon);

    /// <summary>
    /// Builds a <see cref="PokemonModel"/> using information from the <see cref="BasePokemonModel"/>
    /// </summary>
    /// <param name="name">The pokemon's species name</param>
    /// <param name="nickname">The pokemon's nickname, if applicable</param>
    /// <param name="form">The pokemon's form</param>
    public Task<PokemonModel> GetNewPokemon(string name, string nickname, string form);

    /// <summary>
    /// Builds a <see cref="PokemonModel"/> using information from the <see cref="BasePokemonModel"/>
    /// </summary>
    /// <param name="name">The pokemon's species name</param>
    /// <param name="nature">The nature to give the pokemon</param>
    /// <param name="gender">The pokemon's gender</param>
    /// <param name="status">The pokemon's status</param>
    /// <param name="nickname">The pokemon's nickname, if applicable</param>
    /// <param name="form">The pokemon's form</param>
    public Task<PokemonModel> GetNewPokemon(
        string name,
        Nature nature,
        Gender gender,
        Status status,
        string? nickname,
        string form);

    /// <summary>
    /// Returns a specific Dex entry from a specific Dex collection
    /// </summary>
    /// <param name="name">The name of the dex entry</param>
    /// <param name="form">The pokemon form to select</param>
    public Task<PokemonAndForms> GetPokedexEntry(
        string name,
        string form);

    /// <summary>
    /// Adds a collection of dex entry to a specific dex collection
    /// </summary>
    /// <param name="documents">The documents to add to collection</param>
    public Task PostPokedexEntries(IEnumerable<BasePokemonModel> documents);
}
