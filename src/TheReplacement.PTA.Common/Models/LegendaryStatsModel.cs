using System;
using System.Collections.Generic;

namespace TheReplacement.PTA.Common.Models
{
    public class LegendaryStatsModel
    {
        public static LegendaryStatsModel GetNonLegendaryStats()
        {
            return new LegendaryStatsModel
            {
                Moves = Array.Empty<string>(),
                LegendaryMoves = Array.Empty<string>(),
                Passives = Array.Empty<string>(),
                Features = Array.Empty<string>()
            };
        }

        public int HP { get; set; }
        public IEnumerable<string> Moves { get; set; }
        public IEnumerable<string> LegendaryMoves { get; set; }
        public IEnumerable<string> Passives { get; set; }
        public IEnumerable<string> Features { get; set; }
    }
}
