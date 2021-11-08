using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheReplacements.PTA.Common.Models
{
    public class Game
    {
        public ObjectId _id { get; set; }
        public string GameId { get; set; }
    }
}
