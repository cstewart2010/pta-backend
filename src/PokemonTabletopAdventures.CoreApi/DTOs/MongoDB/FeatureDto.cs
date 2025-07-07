using MongoDB.Bson;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB.Interfaces;
using PokemonTabletopAdventures.Models.Interfaces;

namespace PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;

/// <summary>
/// Represents an feature in the FeatureDex
/// </summary>
internal class FeatureDto : IDocument, IDexDocument
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
