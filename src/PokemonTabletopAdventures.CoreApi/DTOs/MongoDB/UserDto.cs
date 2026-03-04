using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PokemonTabletopAdventures.Models.Enums;

namespace PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;

/// <summary>
/// Represents a User in Pokemon Tabletop Adventures 
/// </summary>
public class UserDto : IAuthenticated, IDocument
{
    /// <inheritdoc />
    public ObjectId Id { get; set; }

    /// <summary>
    /// Id for PTA user
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Username for PTA user
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <inheritdoc />
    public bool IsOnline { get; set; }

    /// <inheritdoc />
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// The 30 minute activity token for trainers
    /// </summary>
    public string ActivityToken { get; set; } = string.Empty;

    /// <summary>
    /// Date PTA user account was created
    /// </summary>
    [BsonRepresentation(BsonType.String)]
    public DateTimeOffset DateCreated { get; set; }

    /// <summary>
    /// Site role for PTA user
    /// </summary>
    [BsonRepresentation(BsonType.String)]
    public UserRoleOnSite SiteRole { get; set; }

    /// <summary>
    /// Games of which the PTA user is a member
    /// </summary>
    public ICollection<Guid> Games { get; set; } = [];

    /// <summary>
    /// List of PTA user's messages
    /// </summary>
    public ICollection<Guid> Messages { get; set; } = [];
}
