using MongoDB.Bson;

namespace PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;

/// <summary>
/// Represents a sprite in the Pokemon Tabletop adventures app
/// </summary>
public class SpriteDto : IDocument
{
    /// <summary>
    /// MongoDB id
    /// </summary>
    public ObjectId Id { get; set; }

    /// <summary>
    /// Friendly text for the select
    /// </summary>
    public string FriendlyText { get; set; } = string.Empty;

    /// <summary>
    /// Value for the Pokemon Showdown sprite
    /// </summary>
    public string Value { get; set; } = string.Empty;
}
