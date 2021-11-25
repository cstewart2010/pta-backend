using MongoDB.Bson;
using TheReplacement.PTA.Common.Interfaces;

namespace TheReplacement.PTA.Common.Models
{
    public class OriginModel : IDocument, INamed
    {
        public ObjectId _id { get; set; }
        public string Name { get; set; }
        public string Skill { get; set; }
        public string Lifestyle { get; set; }
        public int Savings { get; set; }
        public string Equipment { get; set; }
        public string StartingPokemon { get; set; }
        public FeatureModel Feature { get; set; }
    }
}
