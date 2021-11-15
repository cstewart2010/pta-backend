namespace TheReplacements.PTA.Common.Models
{
    /// <summary>
    /// Represent the parts of a Pokemon's stats
    /// </summary>
    public class TrainerStatsModel
    {
        /// <summary>
        /// Initializes a new instance of <see cref="TrainerStatsModel"/> with default trainer values values
        /// </summary>
        public TrainerStatsModel()
        {
            RawStrValue = 6;
            RawDexValue = 6;
            RawConValue = 6;
            RawIntValue = 6;
            RawWisValue = 6;
            RawChaValue = 6;
            EarnedStats = 64;
        }

        /// <summary>
        /// The Raw Strength value of the trainer stat
        /// </summary>
        public int RawStrValue { get; set; }

        /// <summary>
        /// The Raw Dexterity value of the trainer stat
        /// </summary>
        public int RawDexValue { get; set; }

        /// <summary>
        /// The Raw Constitution value of the trainer stat
        /// </summary>
        public int RawConValue { get; set; }

        /// <summary>
        /// The Raw Intelligence value of the trainer stat
        /// </summary>
        public int RawIntValue { get; set; }

        /// <summary>
        /// The Raw Wisdom value of the trainer stat
        /// </summary>
        public int RawWisValue { get; set; }

        /// <summary>
        /// The Raw Charisma value of the trainer stat
        /// </summary>
        public int RawChaValue { get; set; }

        /// <summary>
        /// The total earned stats that the user can allocate to their raw value
        /// </summary>
        public int EarnedStats { get; set; }
    }
}
