using MongoDB.Bson;
using TheReplacement.PTA.Common.Interfaces;

namespace TheReplacement.PTA.Common.Models
{
    /// <summary>
    /// Represents an origin in the OriginDex
    /// </summary>
    public class OriginModel : IDocument, IDexDocument
    {
        /// <inheritdoc/>
        public ObjectId _id { get; set; }

        /// <inheritdoc/>
        public string Name { get; set; }

        /// <summary>
        /// The Skill granted by the Origin
        /// </summary>
        public string Skill { get; set; }

        /// <summary>
        /// The lifestyle granted by the Origin
        /// </summary>
        public string Lifestyle { get; set; }

        /// <summary>
        /// The trainer's starting funds
        /// </summary>
        public int Savings { get; set; }

        /// <summary>
        /// The trainer's starting equipment
        /// </summary>
        public string Equipment { get; set; }

        /// <summary>
        /// The trainer's starting pokemon, if applicable
        /// </summary>
        public string StartingPokemon { get; set; }

        /// <summary>
        /// The Origin's specialized feature
        /// </summary>
        public FeatureModel Feature { get; set; }
    }
}
