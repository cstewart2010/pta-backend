using MongoDB.Bson;
using System.Collections.Generic;
using TheReplacements.PTA.Common.Interfaces;

namespace TheReplacements.PTA.Common.Models
{
    public class TrainerModel : IAuthenticated
    {
        public ObjectId _id { get; set; }
        public string TrainerId { get; set; }
        public string GameId { get; set; }
        public string Username { get; set; }
        public bool IsOnline { get; set; }
        public string PasswordHash { get; set; }
        public bool IsGM { get; set; }
        public List<ItemModel> Items { get; set; }
    }
}
