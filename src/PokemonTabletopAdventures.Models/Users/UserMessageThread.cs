namespace PokemonTabletopAdventures.Models.Users;

/// <summary>
/// Represents a message thread in Pokemon Tabletop Adventures 
/// </summary>
public class UserMessageThread
{
    /// <summary>
    /// Id for PTA user Messages
    /// </summary>
    public required Guid MessageId { get; set; }

    /// <summary>
    /// Collection of messages shared between two PTA Users
    /// </summary>
    public required ICollection<UserMessage> Messages { get; set; } = [];
}