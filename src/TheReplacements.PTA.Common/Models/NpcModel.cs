using MongoDB.Bson;
using System.Collections.Generic;
using TheReplacements.PTA.Common.Interfaces;

namespace TheReplacements.PTA.Common.Models
{
    /// <summary>
    /// Represents an NPC in Pokemon Tabletop Adventures
    /// </summary>
    public class NpcModel : IPerson, IDocument
    {
        /// <inheritdoc />
        public ObjectId _id { get; set; }

        /// <summary>
        /// The NPC's unique id
        /// </summary>
        public string NPCId { get; set; }

        /// <inheritdoc />
        public string TrainerName { get; set; }

        /// <inheritdoc />
        public IEnumerable<string> TrainerClasses { get; set; }

        /// <inheritdoc />
        public TrainerStatsModel TrainerStats { get; set; }

        /// <inheritdoc />
        public IEnumerable<string> Feats { get; set; }
    }
}
