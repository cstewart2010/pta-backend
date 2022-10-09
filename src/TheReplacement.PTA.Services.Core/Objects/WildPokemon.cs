using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TheReplacement.PTA.Services.Core.Objects
{
    public class WildPokemon
    {
        public string Pokemon { get; set; }
        public string Nature { get; set; }
        public string Gender { get; set; }
        public string Status { get; set; }
        public string Form { get; set; }
        public bool ForceShiny { get; set; } 
    }
}
