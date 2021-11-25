using MongoDB.Bson;
using System.Collections.Generic;
using TheReplacement.PTA.Common.Interfaces;

namespace TheReplacement.PTA.Common.Models
{
    public class MoveModel : IDocument, INamed
    {
        public ObjectId _id { get; set; }
        public string Name { get; set; }
        public string Range { get; set; }
        public string Type { get; set; }
        public string Stat { get; set; }
        public string Frequency { get; set; }
        public string DiceRoll { get; set; }
        public string Effects { get; set; }
        public IEnumerable<string> GrantedSkills { get; set; }
        public string ContestStat { get; set; }
        public string ContestKeyword { get; set; }
    }
}
