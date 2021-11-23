using MongoDB.Bson;
using System;
using TheReplacement.PTA.Common.Enums;
using TheReplacement.PTA.Common.Interfaces;

namespace TheReplacement.PTA.Common.Models
{
    internal class LoggerModel : IDocument
    {
        public ObjectId _id { get; set; }
        public string Message { get; set; }
        public LogLevel LogLevel { get; set; }
        public DateTime Timestamp { get; set; }
        public MongoCollection AffectedCollection { get; set; }
    }
}
