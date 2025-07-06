using MongoDB.Bson;
using PokemonTabletopAdventures.Models.Interfaces;

namespace PokemonTabletopAdventures.Models;

/// <summary>
/// Represents an feature in the FeatureDex
/// </summary>
public class FeatureModel : IDocument, IDexDocument
{
    /// <inheritdoc/>
    public ObjectId _id { get; set; }

    /// <inheritdoc/>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The effects of using the feature
    /// </summary>
    public string Effects { get; set; } = string.Empty;
}
