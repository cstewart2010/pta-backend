using MongoDB.Bson;
using System;
using System.Collections.Generic;
using TheReplacement.PTA.Common.Interfaces;

namespace TheReplacement.PTA.Common.Models
{
    /// <summary>
    /// Represents a shop in a PTA session
    /// </summary>
    public class ShopModel : IDocument
    {
        /// <inheritdoc/>
        public ObjectId _id { get; set; }

        /// <summary>
        /// The shop's id
        /// </summary>
        public Guid ShopId { get; set; }

        /// <summary>
        /// The game's id
        /// </summary>
        public Guid GameId { get; set; }

        /// <summary>
        /// The shop's name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Whether the shop is active for trainers to visit
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// A collection of items on sale
        /// </summary>
        public Dictionary<string, WareModel> Inventory { get; set; }
    }
}
