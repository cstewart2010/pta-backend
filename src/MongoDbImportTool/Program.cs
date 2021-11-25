using MongoDbImportTool.Builders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using TheReplacement.PTA.Common.Models;
using TheReplacement.PTA.Common.Utilities;

namespace MongoDbImportTool
{
    class Program
    {
        static void Main(string[] args)
        {
            using var textReader = new StreamReader("json/base_pokemon.min.json");
            using var reader = new JsonTextReader(textReader);
            var token = JToken.ReadFrom(reader);

            foreach (var child in token)
            {
                var pokemon = BasePokemonBuilder.Build(child);
                if (DatabaseUtility.FindBasePokemonByDexNo(pokemon.DexNo) != null)
                {
                    continue;
                }
                if (!DatabaseUtility.TryAddBasePokemon(pokemon, out var error))
                {
                    Console.WriteLine(error.WriteErrorJsonString);
                }
            }
        }
    }
}
