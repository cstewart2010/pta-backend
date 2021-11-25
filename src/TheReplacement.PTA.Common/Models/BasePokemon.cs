using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;
using TheReplacement.PTA.Common.Enums;
using TheReplacement.PTA.Common.Interfaces;

namespace TheReplacement.PTA.Common.Models
{
    public class BasePokemon : IDocument
    {
        public ObjectId _id { get; set; }
        public string Name { get; set; }
        public int DexNo { get; set; }
        public StatsModel PokemonStats { get; set; }
        public string Type { get; set; }
        public string Size { get; set; }
        public string Weight { get; set; }
        public IEnumerable<string> Moves { get; set; }
        public IEnumerable<string> Skills { get; set; }
        public IEnumerable<string> Passives { get; set; }
        public IEnumerable<string> Proficiencies { get; set; }
        public IEnumerable<string> EggGroups { get; set; }
        public string EggHatchRate { get; set; }
        public IEnumerable<string> Habitats { get; set; }
        public string Diet { get; set; }
        public string Rarity { get; set; }
        public int Stage { get; set; }
        public string SpecialFormName { get; set; }
        public string BaseFormName { get; set; }
        public string GMaxMove { get; set; }
        public string EvolvesFrom { get; set; }
        public LegendaryStatsModel LegendaryStats { get; set; }
    }
}
