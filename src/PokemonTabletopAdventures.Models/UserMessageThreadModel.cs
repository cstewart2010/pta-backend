using MongoDB.Bson;
using PokemonTabletopAdventures.Interfaces;

namespace PokemonTabletopAdventures.Models;

/// <summary>
/// Represents a message thread in Pokemon Tabletop Adventures 
/// </summary>
public class UserMessageThreadModel : IDocument
{
    /// <inheritdoc />
    public ObjectId _id { get; set; }

    /// <summary>
    /// Id for PTA user Messages
    /// </summary>
    public Guid MessageId { get; set; }

    /// <summary>
    /// Collection of messages shared between two PTA Users
    /// </summary>
    public IEnumerable<UserMessageModel> Messages { get; set; } = [];
}
