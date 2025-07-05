using MongoDB.Bson;
using PokemonTabletopAdventures.Interfaces;
using PokemonTabletopAdventures.Models.Enums;

namespace PokemonTabletopAdventures.Models;

/// <summary>
/// Represents a User in Pokemon Tabletop Adventures 
/// </summary>
public class UserModel : IAuthenticated, IDocument
{
    /// <summary>
    /// The default constructor for the MongoDB Csharp Driver
    /// </summary>
    public UserModel() { }

    /// <inheritdoc />
    public ObjectId _id { get; set; }

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
    public string DateCreated { get; set; } = string.Empty;

    /// <summary>
    /// Site role for PTA user
    /// </summary>
    public UserRoleOnSite SiteRole { get; set; }

    /// <summary>
    /// Games of which the PTA user is a member
    /// </summary>
    public ICollection<Guid> Games { get; set; } = [];

    /// <summary>
    /// List of PTA user's messages
    /// </summary>
    public IEnumerable<Guid> Messages { get; set; } = [];
}
