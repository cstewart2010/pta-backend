using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheReplacements.PTA.Common.Enums;

namespace TheReplacements.PTA.Common.Models
{
    public class PokemonModel
    {
        public ObjectId _id { get; set; }
        public string PokemonId { get; set; }
        public int DexNo { get; set; }
        public string TrainerId { get; set; }
        public string Nickname { get; set; }
        public IEnumerable<string> NaturalMoves { get; set; }
        public IEnumerable<string> TMMoves { get; set; }
        public int Type { get; set; }
        public int Experience { get; set; }
        public int Level { get; set; }
        public int ExpYield { get; set; }
        public int CatchRate { get; set; }
        public int Nature { get; set; }
        public StatModel HP { get; set; }
        public StatModel Attack { get; set; }
        public StatModel Defense { get; set; }
        public StatModel SpecialAttack { get; set; }
        public StatModel SpecialDefense { get; set; }
        public StatModel Speed { get; set; }
    }
}
