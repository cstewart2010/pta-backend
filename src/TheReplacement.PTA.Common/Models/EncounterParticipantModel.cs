namespace TheReplacement.PTA.Common.Models
{
    /// <summary>
    /// Represents a participant to an encounter during a PTA session
    /// </summary>
    public class EncounterParticipantModel
    {
        /// <summary>
        /// The paticipant's id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The paticipant's name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The paticipant's type (Trainer/Pokemon/Npc)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The paticipant's hp
        /// </summary>
        public int HP { get; set; }
    }
}
