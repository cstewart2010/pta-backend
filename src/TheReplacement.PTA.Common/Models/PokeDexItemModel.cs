using MongoDB.Bson;
using System;
using TheReplacement.PTA.Common.Interfaces;

namespace TheReplacement.PTA.Common.Models
{
    /// <summary>
    /// Represents a container for all pokedex entries
    /// </summary>
    public class PokeDexItemModelv1 : IDocument
    {
        /// <inheritdoc/>
        public ObjectId _id { get; set; }

        /// <summary>
        /// The trainer's unique id
        /// </summary>
        public Guid TrainerId { get; set; }

        /// <summary>
        /// The game's unique id
        /// </summary>
        public Guid GameId { get; set; }

        /// <summary>
        /// The Pokemon species' dex number
        /// </summary>
        public int DexNo { get; set; }

        /// <summary>
        /// Whether or not the pokemon was seen in any circumstance
        /// </summary>
        public bool IsSeen { get; set; }

        /// <summary>
        /// Whether or not the trainer caught the pokemon
        /// </summary>
        public bool IsCaught { get; set; }
    }
}
