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
        public static PokemonModel GetEvolved(PokemonModel currentForm, string nextForm)
        {
            var pokemon = InvokePokeAPI($"pokemon-species/{currentForm.DexNo}");
            if (pokemon == null)
            {
                return null;
            }

            var pokemonName = (string)pokemon["name"];
            var evolutionChain = GetEvolutionChain(pokemon, pokemonName);
            if (evolutionChain?["evolves_to"].FirstOrDefault(possible => string.Equals((string)possible["species"]["name"], nextForm, StringComparison.CurrentCultureIgnoreCase)) == null)
            {
                return null;
            }

            var evolvedPokemon = InvokePokeAPI($"pokemon/{nextForm.ToLower()}");
            var stats = evolvedPokemon["stats"];
            currentForm.DexNo = (int)evolvedPokemon["id"];
            currentForm.HP.Base = (int)stats[0]["base_stat"] / 10;
            currentForm.Attack.Base = (int)stats[1]["base_stat"] / 10;
            currentForm.Defense.Base = (int)stats[2]["base_stat"] / 10;
            currentForm.SpecialAttack.Base = (int)stats[3]["base_stat"] / 10;
            currentForm.SpecialDefense.Base = (int)stats[4]["base_stat"] / 10;
            currentForm.Speed.Base = (int)stats[5]["base_stat"] / 10;
            if (currentForm.Nickname == ((string)pokemon["name"]).ToUpper())
            {
                currentForm.Nickname = ((string)evolvedPokemon["name"]).ToUpper();
            }
            return currentForm;
        }
        public static PokemonModel GetShinyPokemon(string pokemonName, string natureName)
        {
            var pokemon = GetPokemon(pokemonName, natureName);
            pokemon.IsShiny = true;
            return pokemon;
        }

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

            var random = new Random();
            var genderRate = (int)InvokePokeAPI($"pokemon-species/{pokemonName}")["gender_rate"];
            var gender = genderRate switch
            {
                -1 => Gender.Genderless,
                0 => Gender.Male,
                8 => Gender.Female,
                _ => random.Next(8) >= genderRate
                    ? Gender.Male
                    : Gender.Female
            };
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
                Gender = gender,
                Nickname = ((string)pokemon["name"]).ToUpper(),
                Nature = (int)nature,
                IsShiny = random.Next(420) == 69,
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

        private static JToken GetEvolutionChain(JToken pokemon, string name)
        {
            JToken evolutionChain = InvokePokeAPI(CreateHttpRequest((string)pokemon["evolution_chain"]["url"]))?["chain"];

            if (evolutionChain == null)
            {
                return null;
            }

            if (string.Equals((string)evolutionChain["species"]["name"], name, StringComparison.InvariantCultureIgnoreCase))
            {
                return evolutionChain;
            }

            return SearchEvolvesTo(evolutionChain, name);
        }

        private static JToken SearchEvolvesTo(JToken evolutionChain, string name)
        {
            JToken found = null;
            foreach (var possible in evolutionChain["evolvesTo"].TakeWhile(possible => found == null))
            {
                var possibleName = (string)possible["species"]["name"];
                if (string.Equals(possibleName, name, StringComparison.InvariantCultureIgnoreCase))
                {
                    found = possible;
                    break;
                }

                found = SearchEvolvesTo(possible, name);
            }

            return found;
        }

        private static JObject InvokePokeAPI(string endpoint, string query)
        {
            return InvokePokeAPI($"{endpoint}?{query}");
        }

        private static JObject InvokePokeAPI(string endpoint)
        {
            HttpWebRequest request = CreateHttpRequest($"https://pokeapi.co/api/v2/{endpoint}");
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

        private static JObject InvokePokeAPI(HttpWebRequest request)
        {
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

        private static HttpWebRequest CreateHttpRequest(string endpoint)
        {
            return WebRequest.CreateHttp(endpoint);
        }
    }
}
