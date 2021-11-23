using MongoDB.Bson;
using System;
using TheReplacement.PTA.Common.Interfaces;

namespace TheReplacement.PTA.Common.Models
{
    internal class LoggerModel : IDocument
    {
        public ObjectId _id { get; set; }
        public string Message { get; set; }
        public string LogLevel { get; set; }
        public DateTime Timestamp { get; set; }
        public string AffectedCollection { get; set; }
    }
}
