namespace TheReplacements.PTA.Common.Models
{
    public class TrainerStatsModel
    {
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

        public int RawStrValue { get; set; }
        public int RawDexValue { get; set; }
        public int RawConValue { get; set; }
        public int RawIntValue { get; set; }
        public int RawWisValue { get; set; }
        public int RawChaValue { get; set; }
        public int EarnedStats { get; set; }
    }
}
