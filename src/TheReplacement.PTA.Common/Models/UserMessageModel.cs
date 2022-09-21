using System;

namespace TheReplacement.PTA.Common.Models
{
    /// <summary>
    /// Represents a message in Pokemon Tabletop Adventures 
    /// </summary>
    public class UserMessageModel
    {
        /// <summary>
        /// Default constructor for JSON.Net
        /// </summary>
        public UserMessageModel() { }

        /// <summary>
        /// Initializes a new instance of <see cref="UserMessageModel"/>
        /// </summary>
        public UserMessageModel(string userId, string messageContent)
        {
            Timestamp = DateTime.UtcNow.ToString();
            Message = messageContent;
            User = userId;
        }

        /// <summary>
        /// User that sent the message
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// Contents of what was sent
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Timestamp for when the message was created
        /// </summary>
        public string Timestamp { get; set; }
    }
}
