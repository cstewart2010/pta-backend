using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheReplacement.PTA.Common.Models
{
    public class SpriteModel
    {
        /// <summary>
        /// MongoDB id
        /// </summary>
        public ObjectId _id { get; set; }

        /// <summary>
        /// Friendly text for the select
        /// </summary>
        public string FriendlyText { get; set; }

        /// <summary>
        /// Value for the Pokemon Showdown sprite
        /// </summary>
        public string Value { get; set; }
    }
}
