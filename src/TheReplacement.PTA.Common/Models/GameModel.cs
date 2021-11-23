using MongoDB.Bson;
using System.Collections.Generic;
using TheReplacement.PTA.Common.Interfaces;

namespace TheReplacement.PTA.Common.Models
{
    /// <summary>
    /// Represents a Pokemon Tabletop Adventures game session
    /// </summary>
    public class GameModel : IAuthenticated, IDocument
    {
        /// <inheritdoc />
        public ObjectId _id { get; set; }

        /// <summary>
        /// The PTA game session id
        /// </summary>
        public string GameId { get; set; }

        /// <summary>
        /// A user-friendly nickname for the game session
        /// </summary>
        public string Nickname { get; set; }

        /// <inheritdoc />
        public bool IsOnline { get; set; }

        /// <inheritdoc />
        public string PasswordHash { get; set; }

        /// <summary>
        /// Collection of NPC ids that used in this game session
        /// </summary>
        public IEnumerable<string> NPCs { get; set; }
    }
}
