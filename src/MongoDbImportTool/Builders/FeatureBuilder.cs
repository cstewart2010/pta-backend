using System.Collections.Generic;
using System.Threading.Tasks;
using TheReplacement.PTA.Common.Models;

namespace MongoDbImportTool.Builders
{
    internal static class FeatureBuilder
    {
        private static readonly string FeaturesJson = $"{JsonHelper.CurrentDirectory}/json/features.min.json";
        private static readonly string LegendaryFeaturesJson = $"{JsonHelper.CurrentDirectory}/json/legendary_features.min.json";
        private static readonly string SkillsJson = $"{JsonHelper.CurrentDirectory}/json/skills.min.json";
        private static readonly string PassivesJson = $"{JsonHelper.CurrentDirectory}/json/passives.min.json";

        public static void AddFeatures()
        {
            var factory = new TaskFactory();
            var tasks = new List<Task>
            {
                factory.StartNew(() => DatabaseHelper.AddDocuments("Features", GetFeatures(FeaturesJson))),
                factory.StartNew(() => DatabaseHelper.AddDocuments("LegendaryFeatures", GetFeatures(LegendaryFeaturesJson))),
                factory.StartNew(() => DatabaseHelper.AddDocuments("Skills", GetFeatures(SkillsJson))),
                factory.StartNew(() => DatabaseHelper.AddDocuments("Passives", GetFeatures(PassivesJson))),
            };
            foreach (var task in tasks)
            {
                if (!(task.IsCompleted || task.IsFaulted))
                {
                    task.Wait();
                }
            }
        }

        private static IEnumerable<FeatureModel> GetFeatures(string path)
        {
            foreach (var child in JsonHelper.GetToken(path))
            {
                yield return JsonHelper.BuildFeature(child);
            }
        }
    }
}
