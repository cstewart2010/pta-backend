using MongoDB.Bson;
using TheReplacements.PTA.Common.Interfaces;

namespace TheReplacements.PTA.Common.Models
{
    public class GameModel : IAuthenticated
    {
        public ObjectId _id { get; set; }
        public string GameId { get; set; }
        public string Nickname { get; set; }
        public bool IsOnline { get; set; }
        public string PasswordHash { get; set; }
    }
}
