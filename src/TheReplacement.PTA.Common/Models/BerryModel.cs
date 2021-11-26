﻿using MongoDB.Bson;
using TheReplacement.PTA.Common.Interfaces;

namespace TheReplacement.PTA.Common.Models
{
    public class BerryModel : IDocument, INamed
    {
        public ObjectId _id { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public string Effects { get; set; }
        public string Flavors { get; set; }
        public string Rarity { get; set; }
    }
}
