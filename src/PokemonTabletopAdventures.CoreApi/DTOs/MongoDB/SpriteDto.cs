using MongoDB.Bson;

namespace PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;

/// <summary>
/// Represents a sprite in the Pokemon Tabletop adventures app
/// </summary>
internal class SpriteDto
{
    /// <summary>
    /// MongoDB id
    /// </summary>
    public ObjectId _id { get; set; }

    /// <summary>
    /// Friendly text for the select
    /// </summary>
    public string FriendlyText { get; set; } = string.Empty;

    /// <summary>
    /// Value for the Pokemon Showdown sprite
    /// </summary>
    public string Value { get; set; } = string.Empty;
}
