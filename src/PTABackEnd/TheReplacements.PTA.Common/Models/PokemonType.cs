﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheReplacements.PTA.Common.Models
{
    [Flags]
    public enum PokemonType
    {
        None = 0,
        Normal = 1,
        Fire = 2,
        Water = 4,
        Grass = 8,
        Electric = 16,
        Ice = 32,
        Fighting = 64,
        Poison = 128,
        Ground = 256,
        Flying = 512,
        Psychic = 1024,
        Bug = 2048,
        Rock = 4096,
        Ghost = 8192,
        Dark = 16384,
        Dragon = 32768,
        Steel = 65536,
        Fairy = 131072
    }
}
