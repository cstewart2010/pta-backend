using System;

namespace TheReplacement.PTA.Common.Models
{
    /// <summary>
    /// Represents a message in Pokemon Tabletop Adventures 
    /// </summary>
    public class UserMessageModelv1
    {
        /// <summary>
        /// The default constructor for the MongoDb Csharp Driver
        /// </summary>
        public UserMessageModelv1() { }

        /// <summary>
        /// Initializes a new instance of <see cref="UserMessageModelv1"/>
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="messageContent"></param>
        public UserMessageModelv1(Guid userId, string messageContent)
        {
            Timestamp = DateTime.UtcNow.ToString();
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
        public string Message { get; set; }

        /// <summary>
        /// Timestamp for when the message was created
        /// </summary>
        public string Timestamp { get; set; }
    }
}
