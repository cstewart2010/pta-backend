using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheReplacement.PTA.Common.Interfaces;

namespace TheReplacement.PTA.Common.Models
{
    /// <summary>
    /// Represents an encounter during a PTA session
    /// </summary>
    public class EncounterModel : IDocument
    {
        /// <inheritdoc/>
        public ObjectId _id { get; set; }

        /// <summary>
        /// The encounter's id
        /// </summary>
        public Guid EncounterId { get; set; }

        /// <summary>
        /// The game id associated with the encounter
        /// </summary>
        public Guid GameId { get; set; }

        /// <summary>
        /// The encounter's friendly nickname
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Whether or not the encounter is active for all trainers
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// The type of Encounter (Wild, Trainer, Hybrid)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The encounters participants
        /// </summary>
        public IEnumerable<EncounterParticipantModel> ActiveParticipants { get; set; }
    }
}
