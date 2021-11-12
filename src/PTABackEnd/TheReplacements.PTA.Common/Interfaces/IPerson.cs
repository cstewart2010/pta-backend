using System.Collections.Generic;
using TheReplacements.PTA.Common.Models;

namespace TheReplacements.PTA.Common.Interfaces
{
    interface IPerson
    {
        public string TrainerName { get; set; }
        public IEnumerable<string> TrainerClasses { get; set; }
        public TrainerStatsModel TrainerStats { get; set; }
        public IEnumerable<string> Feats { get; set; }
    }
}
