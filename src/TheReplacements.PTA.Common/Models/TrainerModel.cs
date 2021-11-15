using MongoDB.Bson;
using System.Collections.Generic;
using TheReplacements.PTA.Common.Interfaces;

namespace TheReplacements.PTA.Common.Models
{
    public class TrainerModel : IAuthenticated, IPerson
    {
        public ObjectId _id { get; set; }
        public string GameId { get; set; }
        public string TrainerId { get; set; }
        public int Level { get; set; }
        public string TrainerName { get; set; }
        public string PasswordHash { get; set; }
        public IEnumerable<string> TrainerClasses { get; set; }
        public TrainerStatsModel TrainerStats { get; set; }
        public IEnumerable<string> Feats { get; set; }
        public int Money { get; set; }
        public bool IsOnline { get; set; }
        public List<ItemModel> Items { get; set; }
        public bool IsGM { get; set; }
    }
}
