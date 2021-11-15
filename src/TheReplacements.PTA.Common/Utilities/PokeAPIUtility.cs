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
    /// <summary>
    /// Provides a collection of methods for accessing the PokeAPI service
    /// </summary>
    public static class PokeAPIUtility
    {
        /// <summary>
        /// Returns an updated pokemon document for its evolution if valid, else null
        /// </summary>
        /// <param name="currentForm">The document for the current pokemon form</param>
        /// <param name="nextForm">The species name for the pokemon's evolution</param>
        public static PokemonModel GetEvolved(
            PokemonModel currentForm,
            string nextForm)
        {
            if (currentForm == null)
            {
                throw new ArgumentNullException(nameof(currentForm));
            }

            if (string.IsNullOrWhiteSpace(nextForm))
            {
                throw new ArgumentNullException(nameof(nextForm));
            }

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
            string updatedName = null;
            if (currentForm.Nickname == ((string)pokemon["name"]).ToUpper())
            {
                updatedName = ((string)evolvedPokemon["name"]).ToUpper();
            }

            var evolvedForm = new PokemonModel
            {
                PokemonId = currentForm.PokemonId,
                DexNo = (int)evolvedPokemon["id"],
                Type = (int)evolvedPokemon["types"].Aggregate(PokemonTypes.None, (prev, curr) =>
                {
                    Enum.TryParse((string)curr["type"]["name"], true, out PokemonTypes result);
                    return prev | result;
                }),
                Gender = currentForm.Gender,
                Nickname = updatedName ?? currentForm.Nickname,
                Nature = currentForm.Nature,
                IsShiny = currentForm.IsShiny,
                HP = currentForm.HP,
                Attack = currentForm.Attack,
                Defense = currentForm.Defense,
                SpecialAttack = currentForm.SpecialAttack,
                SpecialDefense = currentForm.SpecialDefense,
                Speed = currentForm.Speed
            };
            evolvedForm.DexNo = (int)evolvedPokemon["id"];
            evolvedForm.HP.Base = (int)stats[0]["base_stat"] / 10;
            evolvedForm.Attack.Base = (int)stats[1]["base_stat"] / 10;
            evolvedForm.Defense.Base = (int)stats[2]["base_stat"] / 10;
            evolvedForm.SpecialAttack.Base = (int)stats[3]["base_stat"] / 10;
            evolvedForm.SpecialDefense.Base = (int)stats[4]["base_stat"] / 10;
            evolvedForm.Speed.Base = (int)stats[5]["base_stat"] / 10;
            return evolvedForm;
        }

        /// <summary>
        /// Returns a pokemon document with the matching species
        /// </summary>
        /// <param name="pokemonName">The species name of the pokemon</param>
        /// <param name="natureName">The name of the nature</param>
        public static PokemonModel GetPokemon(
            string pokemonName,
            string natureName)
        {
            if (string.IsNullOrWhiteSpace(pokemonName))
            {
                throw new ArgumentNullException(nameof(pokemonName));
            }

            if (string.IsNullOrWhiteSpace(natureName))
            {
                throw new ArgumentNullException(nameof(natureName));
            }

            var pokemon = InvokePokeAPI($"pokemon/{pokemonName.ToLower()}");
            if (pokemon == null)
            {
                return null;
            }

            if (!Enum.TryParse(natureName, true, out Nature nature))
            {
                return null;
            }

            var random = new Random();
            var genderRate = (int)InvokePokeAPI($"pokemon-species/{pokemonName.ToLower()}")["gender_rate"];
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
                Ability = random.Next(2) + 1,
                Gender = gender,
                Nickname = ((string)pokemon["name"]).ToUpper(),
                Nature = (int)nature,
                IsShiny = random.Next(420) == 69,
                HP = GetStat(stats, "hp", modifiers.HpModifier),
                Attack = GetStat(stats, "attack", modifiers.AttackModifier),
                Defense = GetStat(stats, "defense", modifiers.DefenseModifier),
                SpecialAttack = GetStat(stats, "special-attack", modifiers.SpecialAttackModifier),
                SpecialDefense = GetStat(stats, "special-defense", modifiers.DefenseModifier),
                Speed = GetStat(stats, "speed", modifiers.SpeedModifier)
            };
        }

        private static PokemonStatModel GetStat(JToken stats, string statName, int natureModifier)
        {
            var correctStat = stats.First(stat => (string)stat["stat"]["name"] == statName);
            return new PokemonStatModel
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
            foreach (var possible in evolutionChain["evolves_to"].TakeWhile(possible => found == null))
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
