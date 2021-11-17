using MongoDB.Bson;
using System.Collections.Generic;
using TheReplacements.PTA.Common.Interfaces;

namespace TheReplacements.PTA.Common.Models
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

        /// <summary>
        /// The trainer's Level
        /// </summary>
        public int Level { get; set; }

        /// <inheritdoc />
        public string TrainerName { get; set; }

        /// <inheritdoc />
        public string PasswordHash { get; set; }

        /// <inheritdoc />
        public IEnumerable<string> TrainerClasses { get; set; }

        /// <inheritdoc />
        public TrainerStatsModel TrainerStats { get; set; }

        /// <inheritdoc />
        public IEnumerable<string> Feats { get; set; }
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
    }
}
