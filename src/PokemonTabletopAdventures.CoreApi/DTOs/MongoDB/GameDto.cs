using MongoDB.Bson;

namespace PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;

/// <summary>
/// Represents a Pokemon Tabletop Adventures game session
/// </summary>
internal class GameDto : IAuthenticated, IDocument
{
    /// <inheritdoc />
    public ObjectId _id { get; set; }

    /// <summary>
    /// The PTA game session id
    /// </summary>
    public Guid GameId { get; set; }

    /// <summary>
    /// A user-friendly nickname for the game session
    /// </summary>
    public string Nickname { get; set; } = string.Empty;

    /// <inheritdoc />
    public bool IsOnline { get; set; }

    /// <inheritdoc />
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Collection of NPC ids that used in this game session
    /// </summary>
    public ICollection<Guid> NPCs { get; set; } = [];

    /// <summary>
    /// Collection of logs related to the game
    /// </summary>
    public ICollection<LogDto> Logs { get; set; } = [];
}
