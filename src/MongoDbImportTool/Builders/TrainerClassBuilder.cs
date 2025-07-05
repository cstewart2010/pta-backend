using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using TheReplacement.PTA.Common.Models;

namespace MongoDbImportTool.Builders
{
    internal static class TrainerClassBuilder
    {
        private static readonly string TrainerClassesJson = $"{JsonHelper.CurrentDirectory}/json/trainer_classes.min.json";

        public static void AddClasses()
        {
            DatabaseHelper.AddDocuments("TrainerClasses", GetClassess(TrainerClassesJson));
        }

        private static IEnumerable<TrainerClassModelv1> GetClassess(string path)
        {
            foreach (var child in JsonHelper.GetToken(path))
            {
                yield return Build(child);
            }
        }

        private static TrainerClassModelv1 Build(JToken trainerClassToken)
        {
            var baseClass = JsonHelper.GetStringFromToken(trainerClassToken, "BaseClass");
            return new TrainerClassModelv1
            {
                Name = JsonHelper.GetNameFromToken(trainerClassToken),
                BaseClass = baseClass,
                IsBaseClass = string.IsNullOrEmpty(baseClass),
                Feats = trainerClassToken.Children<JProperty>()
                    .Where(child => child.Name.StartsWith("Level") && JsonHelper.IsStringWithValue(child.Value.ToString()))
                    .Select(child =>
                    {
                        var levelString = child.Name[6..].Replace("__1", "");
                        return BuildFeat(child.Value.ToString(), int.Parse(levelString));
                    }),
                PrimaryStat = JsonHelper.GetStringFromToken(trainerClassToken, "Primary Stat"),
                SecondaryStat = JsonHelper.GetStringFromToken(trainerClassToken, "Secondary Stat"),
                Skills = JsonHelper.GetStringFromToken(trainerClassToken, "Skill Talents"),
            };
        }

        private static TrainerClassFeatModelv1 BuildFeat(string feat, int level)
        {
            return new TrainerClassFeatModelv1
            {
                Name = feat,
                LevelLearned = level
            };
        }
    }
}
