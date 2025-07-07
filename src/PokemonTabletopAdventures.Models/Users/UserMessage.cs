namespace PokemonTabletopAdventures.Models.Users;

/// <summary>
/// Represents a message in Pokemon Tabletop Adventures 
/// </summary>
public class UserMessage
{
    /// <summary>
    /// User that sent the message
    /// </summary>
    public required Guid User { get; set; }

    /// <summary>
    /// Contents of what was sent
    /// </summary>
    public required string Message { get; set; }

    /// <summary>
    /// Timestamp for when the message was created
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }
}