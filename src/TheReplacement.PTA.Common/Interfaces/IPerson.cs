using System.Collections.Generic;
using TheReplacement.PTA.Common.Models;

namespace TheReplacement.PTA.Common.Interfaces
{
    /// <summary>
    /// Provides a collection of properties used for Person types
    /// </summary>
    public interface IPerson
    {
        /// <summary>
        /// The name of the Trainer
        /// </summary>
        public string TrainerName { get; set; }

        /// <summary>
        /// Collection of Trainer Classes for the Trainer
        /// </summary>
        public IEnumerable<string> TrainerClasses { get; set; }

        /// <summary>
        /// The trainers player stats
        /// </summary>
        public StatsModel TrainerStats { get; set; }

        /// <summary>
        /// Collection of Trainer Feats
        /// </summary>
        public IEnumerable<string> Feats { get; set; }
    }
}
