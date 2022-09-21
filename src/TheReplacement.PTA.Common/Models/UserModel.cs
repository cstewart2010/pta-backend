using MongoDB.Bson;
using System;
using System.Collections.Generic;
using TheReplacement.PTA.Common.Enums;
using TheReplacement.PTA.Common.Interfaces;
using TheReplacement.PTA.Common.Utilities;

namespace TheReplacement.PTA.Common.Models
{
    /// <summary>
    /// Represents a User in Pokemon Tabletop Adventures 
    /// </summary>
    public class UserModel : IAuthenticated, IDocument
    {
        /// <summary>
        /// Default Constructor for JSON.Net
        /// </summary>
        public UserModel() { }

        /// <summary>
        /// Initializes a new instance of <see cref="UserModel"/>
        /// </summary>
        public UserModel(string username, string password)
        {
            UserId = Guid.NewGuid().ToString();
            Username = username;
            PasswordHash = EncryptionUtility.HashSecret(password);
            IsOnline = true;
            ActivityToken = EncryptionUtility.GenerateToken();
            DateCreated = DateTime.UtcNow.ToString("u");
            SiteRole = UserRoleOnSite.Active.ToString();
            Games = Array.Empty<string>();
            Messages = Array.Empty<string>();
        }

        /// <inheritdoc />
        public ObjectId _id { get; set; }

        /// <summary>
        /// Id for PTA user
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Username for PTA user
        /// </summary>
        public string Username { get; set; }

        /// <inheritdoc />
        public bool IsOnline { get; set; }

        /// <inheritdoc />
        public string PasswordHash { get; set; }

        /// <summary>
        /// The 30 minute activity token for trainers
        /// </summary>
        public string ActivityToken { get; set; }

        /// <summary>
        /// Date PTA user account was created
        /// </summary>
        public string DateCreated { get; set; }

        /// <summary>
        /// Site role for PTA user
        /// </summary>
        public string SiteRole { get; set; }

        /// <summary>
        /// Games of which the PTA user is a member
        /// </summary>
        public ICollection<string> Games { get; set; }

        /// <summary>
        /// List of PTA user's messages
        /// </summary>
        public IEnumerable<string> Messages { get; set; }
    }
}
