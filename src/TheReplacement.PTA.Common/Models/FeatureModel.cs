using MongoDB.Bson;
using TheReplacement.PTA.Common.Interfaces;

namespace TheReplacement.PTA.Common.Models
{
    public class FeatureModel : IDocument, INamed
    {
        public ObjectId _id { get; set; }
        public string Name { get; set; }
        public string Effects { get; set; }
    }
}
