using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;
using TheReplacement.PTA.Common.Interfaces;

namespace TheReplacement.PTA.Common.Models
{
    public class TrainerClassModel :  IDocument, INamed
    {
        public ObjectId _id { get; set; }
        public string Name { get; set; }
        public string BaseClass { get; set; }
        public bool IsBaseClass { get; set; }
        public IEnumerable<TrainerClassFeatModel> Feats { get; set; }
        public string PrimaryStat { get; set; }
        public string SecondaryStat { get; set; }
        public string Skills { get; set; }
    }
}
