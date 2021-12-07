using System.Collections.Generic;
using System.Threading.Tasks;
using TheReplacement.PTA.Common.Models;

namespace MongoDbImportTool.Builders
{
    internal static class BaseItemBuilder
    {
        private static readonly string KeyItemsJson = $"{JsonHelper.CurrentDirectory}/json/key_items.min.json";
        private static readonly string MedicalItemsJson = $"{JsonHelper.CurrentDirectory}/json/medical_items.min.json";
        private static readonly string PokeballsJson = $"{JsonHelper.CurrentDirectory}/json/pokeballs.min.json";
        private static readonly string PokemonItemsJson = $"{JsonHelper.CurrentDirectory}/json/pokemon_items.min.json";
        private static readonly string TrainerEquipmentJson = $"{JsonHelper.CurrentDirectory}/json/trainer_equipment.min.json";

        public static void AddItems()
        {
            var factory = new TaskFactory();
            var tasks = new List<Task>
            {
                factory.StartNew(() => DatabaseHelper.AddDocuments("KeyItems", GetItems(KeyItemsJson))),
                factory.StartNew(() => DatabaseHelper.AddDocuments("MedicalItems", GetItems(PokeballsJson))),
                factory.StartNew(() => DatabaseHelper.AddDocuments("Pokeballs", GetItems(PokeballsJson))),
                factory.StartNew(() => DatabaseHelper.AddDocuments("PokemonItems", GetItems(PokemonItemsJson))),
                factory.StartNew(() => DatabaseHelper.AddDocuments("TrainerEquipment", GetItems(TrainerEquipmentJson))),
            };
            foreach (var task in tasks)
            {
                if (!(task.IsCompleted || task.IsFaulted))
                {
                    task.Wait();
                }
            }
        }

        private static IEnumerable<BaseItemModel> GetItems(string path)
        {
            foreach (var child in JsonHelper.GetToken(path))
            {
                yield return JsonHelper.BuildItem(child);
            }
        }
    }
}
