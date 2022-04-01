using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheReplacement.PTA.Common.Models;

namespace TheReplacement.PTA.Common.Enums
{
    /// <summary>
    /// Container for all possible <see cref="EncounterParticipantModel"/> types
    /// </summary>
    public enum EncounterParticipantType
    {
        /// <summary>
        /// Represents a <see cref="TrainerModel"/>
        /// </summary>
        Trainer = 1,

        /// <summary>
        /// Represents a <see cref="PokemonModel"/>
        /// </summary>
        Pokemon = 2,

        /// <summary>
        /// Represents an <see cref="NpcModel"/>
        /// </summary>
        Npc = 3
    }
}
