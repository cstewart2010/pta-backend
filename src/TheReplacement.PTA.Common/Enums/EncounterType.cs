using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheReplacement.PTA.Common.Models;

namespace TheReplacement.PTA.Common.Enums
{
    /// <summary>
    /// Container for all possible <see cref="EncounterModel"/> types
    /// </summary>
    public enum EncounterType
    {
        /// <summary>
        /// Represents a Wild Encounter 
        /// </summary>
        Wild = 1,

        /// <summary>
        /// Represents a Trainer Encounter 
        /// </summary>
        Trainer = 2,

        /// <summary>
        /// Represents an Encounter with both Wild and Trainer participants
        /// </summary>
        Hybrid = 3
    }
}
