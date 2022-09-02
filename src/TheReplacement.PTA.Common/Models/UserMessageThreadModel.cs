using MongoDB.Bson;
using System.Collections.Generic;
using TheReplacement.PTA.Common.Interfaces;

namespace TheReplacement.PTA.Common.Models
{
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
        public string MessageId { get; set; }

        /// <summary>
        /// Collection of messages shared between two PTA Users
        /// </summary>
        public IEnumerable<UserMessageModel> Messages { get; set; }
    }
}
