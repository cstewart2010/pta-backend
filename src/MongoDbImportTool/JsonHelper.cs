using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TheReplacement.PTA.Common.Enums;
using TheReplacement.PTA.Common.Exceptions;
using TheReplacement.PTA.Common.Models;

namespace MongoDbImportTool
{
    internal static class JsonHelper
    {
        internal static string CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        internal static JToken GetToken(string path)
        {
            using var textReader = new StreamReader(path);
            using var reader = new JsonTextReader(textReader);
            return JToken.ReadFrom(reader);
        }

        internal static BaseItemModel BuildItem(JToken itemToken)
        {
            return new BaseItemModel
            {
                Name = GetNameFromToken(itemToken),
                Price = GetIntFromToken(itemToken, "Price"),
                Effects = GetEffectsFromToken(itemToken)
            };
        }

        internal static FeatureModel BuildFeature(JToken featureToken)
        {
            return new FeatureModel
            {
                Name = GetNameFromToken(featureToken),
                Effects = GetEffectsFromToken(featureToken)
            };
        }

        public static string GetNameFromToken(JToken token)
        {
            return GetStringFromToken(token, "Name");
        }

        public static string GetEffectsFromToken(JToken token)
        {
            return GetStringFromToken(token, "Effects");
        }

        internal static string BuildType(JToken token)
        {
            return token.Children<JProperty>()
                .Where(child => child.Name.StartsWith("Type"))
                .Select(child => child.Value.ToString())
                .Aggregate(PokemonTypes.None, (prev, curr) =>
                {
                    if (!Enum.TryParse(curr.Replace('/', '_'), out PokemonTypes type))
                    {
                        return prev;
                    }

                    return prev | type;
                })
                .ToString().Replace('_', '/');
        }

        internal static IEnumerable<string> BuildSkills(JToken pokemonToken)
        {
            return pokemonToken.Children<JProperty>()
                .Where(child => child.Name.Contains("Skill"))
                .Select(child => child.Value.ToString())
                .Where(IsStringWithValue);
        }

        public static string GetStringFromTokenOrDefault(
            JToken token,
            string property)
        {
            return token[property]?.ToString() ?? string.Empty;
        }

        public static string GetStringFromToken(
            JToken token,
            string property)
        {
            return token[property]?.ToString()
                ?? throw new MissingJsonPropertyException(property);
        }

        public static int GetIntFromToken(
            JToken token,
            string property)
        {
            var propertyValue = token[property]?.ToString()
                ?? throw new MissingJsonPropertyException(property);

            if (!int.TryParse(propertyValue, out var result))
            {
                throw new InvalidJsonPropertyException((JProperty)token, typeof(int));
            }

            return result;
        }

        public static bool IsStringWithValue(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }
    }
}
