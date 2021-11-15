using MongoDB.Bson;
using System;
using TheReplacements.PTA.Common.Enums;
using TheReplacements.PTA.Common.Interfaces;

namespace TheReplacements.PTA.Common.Models
{
    internal class LoggerModel : IMongoCollectionModel
    {
        public ObjectId _id { get; set; }
        public string Message { get; set; }
        public LogLevel LogLevel { get; set; }
        public DateTime Timestamp { get; set; }
        public MongoCollection AffectedCollection { get; set; }
    }
}
