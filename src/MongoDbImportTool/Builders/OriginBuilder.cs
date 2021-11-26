using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using TheReplacement.PTA.Common.Models;

namespace MongoDbImportTool.Builders
{
    internal static class OriginBuilder
    {
        private static readonly string OriginsJson = $"{JsonHelper.CurrentDirectory}/json/origins.min.json";

        public static void AddOrigins()
        {
            DatabaseHelper.AddDocuments("Origins", GetOrigins(OriginsJson));
        }

        private static IEnumerable<OriginModel> GetOrigins(string path)
        {
            foreach (var child in JsonHelper.GetToken(path))
            {
                yield return Build(child);
            }
        }

        private static OriginModel Build(JToken originToken)
        {
            return new OriginModel
            {
                Name = JsonHelper.GetNameFromToken(originToken),
                Skill = JsonHelper.GetStringFromToken(originToken, "Skill Talent"),
                Lifestyle = JsonHelper.GetStringFromToken(originToken, "Lifestyle"),
                Savings = JsonHelper.GetIntFromToken(originToken, "Savings"),
                Equipment = JsonHelper.GetStringFromToken(originToken, "Starting Equipment"),
                StartingPokemon = JsonHelper.GetStringFromToken(originToken, "Starting Pokemon"),
                Feature = new FeatureModel
                {
                    Name = JsonHelper.GetStringFromToken(originToken, "Feature Name"),
                    Effects = JsonHelper.GetStringFromToken(originToken, "Feature Effects")
                }
            };
        }
    }
}
