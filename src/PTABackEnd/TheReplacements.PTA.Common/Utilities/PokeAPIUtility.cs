using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net;
using TheReplacements.PTA.Common.Attributes;
using TheReplacements.PTA.Common.Enums;
using TheReplacements.PTA.Common.Models;

namespace TheReplacements.PTA.Common.Utilities
{
    public static class PokeAPIUtility
    {
        public static PokemonModel GetPokemon(string pokemonName, string natureName)
        {
            var pokemon = InvokePokeAPI($"pokemon/{pokemonName}");
            if (pokemon == null)
            {
                return null;
            }

            if (!Enum.TryParse(natureName, true, out Nature nature))
            {
                return null;
            }

            var modifiers = Attribute.GetCustomAttribute(typeof(Nature).GetField(nature.ToString()), typeof(NatureModifierAttribute)) as NatureModifierAttribute;
            var stats = pokemon["stats"];
            return new PokemonModel
            {
                PokemonId = Guid.NewGuid().ToString(),
                DexNo = (int)pokemon["id"],
                Type = (int)pokemon["types"].Aggregate(PokemonTypes.None, (prev, curr) =>
                {
                    Enum.TryParse((string)curr["type"]["name"], true, out PokemonTypes result);
                    return prev | result;
                }),
                Nickname = (string)pokemon["name"],
                Nature = (int)nature,
                HP = GetStat(stats, "hp", modifiers.HpModifier),
                Attack = GetStat(stats, "attack", modifiers.AttackModifier),
                Defense = GetStat(stats, "defense", modifiers.DefenseModifier),
                SpecialAttack = GetStat(stats, "special-attack", modifiers.SpecialAttackModifier),
                SpecialDefense = GetStat(stats, "special-defense", modifiers.DefenseModifier),
                Speed = GetStat(stats, "speed", modifiers.SpeedModifier),
            };
        }

        private static StatModel GetStat(JToken stats, string statName, int natureModifier)
        {
            var correctStat = stats.First(stat => (string)stat["stat"]["name"] == statName);
            return new StatModel
            {
                Nature = natureModifier,
                Base = ((int)correctStat["base_stat"]) / 10
            };
        }

        private static JObject InvokePokeAPI(string endpoint, string query)
        {
            return InvokePokeAPI($"{endpoint}?{query}");
        }

        private static JObject InvokePokeAPI(string endpoint)
        {
            HttpWebRequest request = WebRequest.CreateHttp($"https://pokeapi.co/api/v2/{endpoint}");
            request.Method = "GET";
            using var response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode != HttpStatusCode.OK)
            {
                response.Close();
                return null;
            }

            using var reader = new StreamReader(response.GetResponseStream());
            var responseBody = reader.ReadToEnd();
            response.Close();

            return JObject.Parse(responseBody);
        }
    }
}
