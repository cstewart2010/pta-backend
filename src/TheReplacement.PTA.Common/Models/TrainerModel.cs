using MongoDB.Bson;
using System.Collections.Generic;
using TheReplacement.PTA.Common.Interfaces;

namespace TheReplacement.PTA.Common.Models
{
    /// <summary>
    /// Represents a Trainer in Pokemon Tabletop Adventures
    /// </summary>
    public class TrainerModel : IAuthenticated, IPerson, IDocument
    {
        /// <inheritdoc />
        public ObjectId _id { get; set; }

        /// <summary>
        /// The PTA game session id
        /// </summary>
        public string GameId { get; set; }

        /// <summary>
        /// The trainer's unique id
        /// </summary>
        public string TrainerId { get; set; }

        /// <inheritdoc />
        public string TrainerName { get; set; }

        /// <inheritdoc />
        public string PasswordHash { get; set; }

        /// <summary>
        /// The 30 minute activity token for trainers
        /// </summary>
        public string ActivityToken { get; set; }

        /// <inheritdoc />
        public IEnumerable<string> TrainerClasses { get; set; }

        /// <inheritdoc />
        public StatsModel TrainerStats { get; set; }

        /// <inheritdoc />
        public IEnumerable<string> Feats { get; set; }

        /// <summary>
        /// The trainer's money (or debt)
        /// </summary>
        public int Money { get; set; }

        /// <inheritdoc />
        public bool IsOnline { get; set; }

        /// <summary>
        /// A collection of the trainer's items
        /// </summary>
        public List<ItemModel> Items { get; set; }

        /// <summary>
        /// Whether the trainer is the Game Master of the session
        /// </summary>
        public bool IsGM { get; set; }

        /// <summary>
        /// Whether the trainer has completed the new user flow
        /// </summary>
        public bool IsComplete { get; set; }

        /// <summary>
        /// Trainer's achievements
        /// </summary>
        public IEnumerable<string> Honors { get; set; }

        /// <summary>
        /// The trainer's origin
        /// </summary>
        public string Origin { get; set; }
    }
}
