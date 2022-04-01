using MongoDB.Bson;
using System.Collections.Generic;
using TheReplacement.PTA.Common.Interfaces;

namespace TheReplacement.PTA.Common.Models
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
        public StatsModel TrainerStats { get; set; }

        /// <inheritdoc />
        public IEnumerable<string> Feats { get; set; }

        /// <inheritdoc />
        public string GameId { get; set; }

        /// <inheritdoc />
        public int Level { get; set; }

        /// <inheritdoc />
        public IEnumerable<TrainerSkill> TrainerSkills { get; set; }

        /// <inheritdoc />
        public int Age { get; set; }

        /// <inheritdoc />
        public string Gender { get; set; }

        /// <inheritdoc />
        public int Height { get; set; }

        /// <inheritdoc />
        public int Weight { get; set; }

        /// <inheritdoc />
        public string Description { get; set; }

        /// <inheritdoc />
        public string Personality { get; set; }

        /// <inheritdoc />
        public string Background { get; set; }

        /// <inheritdoc />
        public string Goals { get; set; }

        /// <inheritdoc />
        public string Species { get; set; }
    }
}
