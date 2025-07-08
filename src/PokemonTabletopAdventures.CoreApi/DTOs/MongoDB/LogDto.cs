using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;

/// <summary>
/// Represents a Pokemon Tabletop Adventures log
/// </summary>
/// <remarks>
/// Initializes a new instance of <see cref="LogDto"/>
/// </remarks>
public class LogDto(string user, string action)
{
    /// <summary>
    /// The user that the log comes from
    /// </summary>
    public string User { get; set; } = user;

    /// <summary>
    /// The action being logged
    /// </summary>
    public string Action { get; set; } = action;

    /// <summary>
    /// The timestamp for the Log
    /// </summary>
    [BsonRepresentation(BsonType.String)]
    public DateTimeOffset LogTimestamp { get; set; } = DateTimeOffset.Now;
}
