using MongoDB.Bson;

namespace TheReplacements.PTA.Common.Models
{
    public class GameModel
    {
        public ObjectId _id { get; set; }
        public string GameId { get; set; }
        public string Nickname { get; set; }
    }
}
