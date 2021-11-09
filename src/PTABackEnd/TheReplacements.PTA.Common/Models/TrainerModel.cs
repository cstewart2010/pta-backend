using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheReplacements.PTA.Common.Models
{
    public class TrainerModel
    {
        public ObjectId _id { get; set; }
        public string TrainerId { get; set; }
        public string GameId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
        public bool IsGM { get; set; }
        public List<ItemModel> Items { get; set; }
    }
}
