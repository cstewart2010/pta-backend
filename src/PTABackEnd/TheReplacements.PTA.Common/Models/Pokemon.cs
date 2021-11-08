using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheReplacements.PTA.Common.Models
{
    public class Pokemon
    {
        public ObjectId _id { get; set; }
        public int DexNo { get; set; }
        public string Trainerid { get; set; }
        public string Nickname { get; set; }
        public IEnumerable<string> NaturalMoves { get; set; }
        public IEnumerable<string> TMMoves { get; set; }
        public PokemonType Type { get; set; }
        public int Experience { get; set; }
        public int Level { get; set; }
        public int ExpYield { get; set; }
        public int CatchRate { get; set; }
        public int Nature { get; set; }
        public Stat HP { get; set; }
        public Stat Attack { get; set; }
        public Stat Defense { get; set; }
        public Stat SpecialAttack { get; set; }
        public Stat SpecialDefense { get; set; }
        public Stat Speed { get; set; }
    }
}
