using MongoDB.Bson;
using System.Collections.Generic;
using TheReplacements.PTA.Common.Interfaces;

namespace TheReplacements.PTA.Common.Models
{
    public class NpcModel : IPerson
    {
        public ObjectId _id { get; set; }
        public string NPCId { get; set; }
        public string TrainerName { get; set; }
        public IEnumerable<string> TrainerClasses { get; set; }
        public TrainerStatsModel TrainerStats { get; set; }
        public IEnumerable<string> Feats { get; set; }
    }
}
