using System;
using System.Collections.Generic;
using TheReplacement.PTA.Common.Models;

namespace TheReplacement.PTA.Services.Core.Objects
{
    public class PublicUser
    {
        internal PublicUser(UserModel user)
        {
            UserId = user.UserId;
            Username = user.Username;
            DateCreated = DateTime.Parse(user.DateCreated);
            Games = user.Games;
            Messages = user.Messages;
        }

        /// <summary>
        /// Id for PTA user
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Username for PTA user
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Date PTA user account was created
        /// </summary>
        public DateTime DateCreated { get; set; }

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
