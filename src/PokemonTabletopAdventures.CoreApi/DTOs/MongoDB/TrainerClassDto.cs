using MongoDB.Bson;
using PokemonTabletopAdventures.Models;
using PokemonTabletopAdventures.Models.Interfaces;

namespace PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;

/// <summary>
/// Represents an trainer class in the TrainerClassDex
/// </summary>
public class TrainerClassDto :  IDocument, IDexDocument
{
    /// <inheritdoc/>
    public ObjectId _id { get; set; }

    /// <inheritdoc/>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The Class' base class, if applicable
    /// </summary>
    public string BaseClass { get; set; } = string.Empty;

    /// <summary>
    /// Whether the class is a base class
    /// </summary>
    public bool IsBaseClass { get; set; }

    /// <summary>
    /// The Features learned by the class
    /// </summary>
    public IEnumerable<TrainerClassFeat> Feats { get; set; } = [];

    /// <summary>
    /// The Class' primary stat
    /// </summary>
    public string PrimaryStat { get; set; } = string.Empty;

    /// <summary>
    /// The Class' secondary stat
    /// </summary>
    public string SecondaryStat { get; set; } = string.Empty;

    /// <summary>
    /// The Skills granted by the Class
    /// </summary>
    public string Skills { get; set; } = string.Empty;
}
