using MongoDB.Bson;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB.Interfaces;

namespace PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;

/// <summary>
/// Represents a container for all pokedex entries
/// </summary>
internal class PokeDexItemDto : IDocument
{
    /// <inheritdoc/>
    public ObjectId _id { get; set; }

    /// <summary>
    /// The trainer's unique id
    /// </summary>
    public Guid TrainerId { get; set; }

    /// <summary>
    /// The game's unique id
    /// </summary>
    public Guid GameId { get; set; }

    /// <summary>
    /// The Pokemon species' dex number
    /// </summary>
    public int DexNo { get; set; }

    /// <summary>
    /// Whether or not the pokemon was seen in any circumstance
    /// </summary>
    public bool IsSeen { get; set; }

    /// <summary>
    /// Whether or not the trainer caught the pokemon
    /// </summary>
    public bool IsCaught { get; set; }
}
