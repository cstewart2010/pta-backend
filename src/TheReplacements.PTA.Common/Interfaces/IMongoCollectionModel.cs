using MongoDB.Bson;

namespace TheReplacements.PTA.Common.Interfaces
{
    /// <summary>
    /// Provides a collection of properties for MongoDB documents
    /// </summary>
    public interface IMongoCollectionModel
    {
        /// <summary>
        /// MongoDB id
        /// </summary>
        public ObjectId _id { get; set; }
    }
}
