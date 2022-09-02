using MongoDB.Bson;
using System.Collections.Generic;
using TheReplacement.PTA.Common.Interfaces;

namespace TheReplacement.PTA.Common.Models
{
    /// <summary>
    /// Represents a User in Pokemon Tabletop Adventures 
    /// </summary>
    public class UserModel : IAuthenticated, IDocument
    {
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
        /// Date PTA user account was created
        /// </summary>
        public string DateCreated { get; set; }

        /// <summary>
        /// Site role for PTA user
        /// </summary>
        public string SiteRole{ get; set; }

        /// <summary>
        /// Games of which the PTA user is a member
        /// </summary>
        public IEnumerable<string> Games { get; set; }

        /// <summary>
        /// List of PTA user's messages
        /// </summary>
        public IEnumerable<string> Messages { get; set; }
    }
}
