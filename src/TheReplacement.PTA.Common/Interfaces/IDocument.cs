﻿using MongoDB.Bson;

namespace TheReplacement.PTA.Common.Interfaces
{
    /// <summary>
    /// Provides a collection of properties for MongoDB documents
    /// </summary>
    public interface IDocument
    {
        /// <summary>
        /// MongoDB id
        /// </summary>
        public ObjectId _id { get; set; }
    }
}
