using MongoDB.Bson;
using PokemonTabletopAdventures.Models.Interfaces;

namespace PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;

/// <summary>
/// Represents an feature in the FeatureDex
/// </summary>
public class FeatureDto : IDocument, IDexDocument
{
    /// <inheritdoc/>
    public ObjectId Id { get; set; }

    /// <inheritdoc/>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The effects of using the feature
    /// </summary>
    public string Effects { get; set; } = string.Empty;
}
