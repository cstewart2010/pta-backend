using MongoDB.Bson;
using System.Collections.Generic;
using TheReplacements.PTA.Common.Enums;

namespace TheReplacements.PTA.Common.Models
{
    public class PokemonModel
    {
        public ObjectId _id { get; set; }
        public string PokemonId { get; set; }
        public int DexNo { get; set; }
        public string TrainerId { get; set; }
        public Gender Gender { get; set; }
        public string Nickname { get; set; }
        public IEnumerable<string> NaturalMoves { get; set; }
        public IEnumerable<string> TMMoves { get; set; }
        public int Type { get; set; }
        public int Experience { get; set; }
        public int Level { get; set; }
        public int ExpYield { get; set; }
        public int CatchRate { get; set; }
        public int Nature { get; set; }
        public bool IsShiny { get; set; }
        public bool IsOnActiveTeam { get; set; }
        public PokemonStatModel HP { get; set; }
        public PokemonStatModel Attack { get; set; }
        public PokemonStatModel Defense { get; set; }
        public PokemonStatModel SpecialAttack { get; set; }
        public PokemonStatModel SpecialDefense { get; set; }
        public PokemonStatModel Speed { get; set; }

        public void AggregateStats()
        {
            HP.AggregateStatValue();
            Attack.AggregateStatValue();
            Defense.AggregateStatValue();
            SpecialAttack.AggregateStatValue();
            SpecialDefense.AggregateStatValue();
            Speed.AggregateStatValue();
        }
    }
}
