using MongoDB.Bson;

namespace PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;

/// <summary>
/// Represents a message thread in Pokemon Tabletop Adventures 
/// </summary>
internal class UserMessageThreadDto : IDocument
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
    public IEnumerable<UserMessageDto> Messages { get; set; } = [];
}
