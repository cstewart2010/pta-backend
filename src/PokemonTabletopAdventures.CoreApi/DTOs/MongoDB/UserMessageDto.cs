using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;

/// <summary>
/// Represents a message in Pokemon Tabletop Adventures 
/// </summary>
internal class UserMessageDto
{
    public UserMessageDto() { }

    /// <summary>
    /// Initializes a new instance of <see cref="UserMessageDto"/>
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="messageContent"></param>
    public UserMessageDto(Guid userId, string messageContent)
    {
        Timestamp = DateTimeOffset.Now;
        Message = messageContent;
        User = userId;
    }

    /// <summary>
    /// User that sent the message
    /// </summary>
    public Guid User { get; set; }

    /// <summary>
    /// Contents of what was sent
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp for when the message was created
    /// </summary>
    [BsonRepresentation(BsonType.String)]
    public DateTimeOffset Timestamp { get; set; }
}
