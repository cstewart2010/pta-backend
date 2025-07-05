using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using TheReplacement.PTA.Common.Models;

namespace MongoDbImportTool.Builders
{
    internal static class BerryBuilder
    {
        private static readonly string BerryJson = $"{JsonHelper.CurrentDirectory}/json/berries.min.json";

        public static void AddBerries()
        {
            DatabaseHelper.AddDocuments("Berries", GetBerries(BerryJson));
        }

        private static IEnumerable<BerryModelv1> GetBerries(string path)
        {
            foreach (var child in JsonHelper.GetToken(path))
            {
                yield return Build(child);
            }
        }

        private static BerryModelv1 Build(JToken berryToken)
        {
            return new BerryModelv1
            {
                Name = JsonHelper.GetNameFromToken(berryToken),
                Price = JsonHelper.GetIntFromToken(berryToken, "Price"),
                Effects = JsonHelper.GetEffectsFromToken(berryToken),
                Flavors = BuildFlavors(berryToken),
                Rarity = JsonHelper.GetStringFromToken(berryToken, "Rarity")
            };
        }

        private static string BuildFlavors(JToken berryToken)
        {
            return JsonHelper.GetStringFromToken(berryToken, "Flavor")
                .Replace(' ', '/');
        }
    }
}
