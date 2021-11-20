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
        private static readonly Random Random = new Random();

        /// <summary>
        /// Returns an updated pokemon document for its evolution if valid, else null
        /// </summary>
        /// <param name="currentForm">The document for the current pokemon form</param>
        /// <param name="nextForm">The species name for the pokemon's evolution</param>
        public static PokemonModel GetEvolved(
            PokemonModel currentForm,
            string nextForm)
        {
            CheckedGetEvolvedInput
            (
                currentForm,
                nextForm
            );

            var pokemonName = CheckGetEvolvedInputs
            (
                currentForm,
                nextForm
            );

            if (string.IsNullOrEmpty(pokemonName))
            {
                return null;
            }

            var evolvedPokemon = InvokePokeAPI($"pokemon/{nextForm.ToLower()}");
            var stats = evolvedPokemon["stats"];
            var evolvedForm = GetEvolvedPokemon
            (
                currentForm,
                evolvedPokemon,
                stats
            );

            evolvedForm.DexNo = (int)evolvedPokemon["id"];
            evolvedForm.Nickname = GetUpdatedNicknameForEvolvedPokemon
            (
                currentForm.Nickname,
                pokemonName,
                nextForm
            ) ?? currentForm.Nickname;
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
            CheckGetPokemonInputs
            (
                pokemonName,
                natureName
            );

            var pokemonData = InvokePokeAPI($"pokemon/{pokemonName.ToLower()}");
            if (pokemonData == null || !Enum.TryParse(natureName, true, out Nature nature))
            {
                return null;
            }

            var pokemon = new PokemonModel
            {
                PokemonId = Guid.NewGuid().ToString(),
                DexNo = (int)pokemonData["id"],
                Type = (int)pokemonData["types"].Aggregate(PokemonTypes.None, AggregatePokemonTypes),
                Ability = Random.Next(2) + 1,
                Gender = GetRandomizerGender(pokemonName),
                Nickname = ((string)pokemonData["name"]).ToUpper(),
                Nature = (int)nature,
                IsShiny = Random.Next(420) == 69
            };

            AddStats
            (
                pokemon,
                pokemonData["stats"],
                nature
            );

            return pokemon;
        }

        private static PokemonModel GetEvolvedPokemon(
            PokemonModel currentForm,
            JToken evolvedPokemon,
            JToken stats)
        {
            var pokemon = new PokemonModel
            {
                PokemonId = currentForm.PokemonId,
                DexNo = (int)evolvedPokemon["id"],
                Type = (int)evolvedPokemon["types"].Aggregate(PokemonTypes.None, (prev, curr) =>
                {
                    Enum.TryParse((string)curr["type"]["name"], true, out PokemonTypes result);
                    return prev | result;
                }),
                Gender = currentForm.Gender,
                Nature = currentForm.Nature,
                IsShiny = currentForm.IsShiny,
                HP = currentForm.HP,
                Attack = currentForm.Attack,
                Defense = currentForm.Defense,
                SpecialAttack = currentForm.SpecialAttack,
                SpecialDefense = currentForm.SpecialDefense,
                Speed = currentForm.Speed
            };

            UpdateEvolvedPokemonStats
            (
                pokemon,
                stats
            );

            return pokemon;
        }

        private static void UpdateEvolvedPokemonStats(
            PokemonModel evolvedForm,
            JToken stats)
        {
            evolvedForm.HP.Base = (int)stats[0]["base_stat"] / 10;
            evolvedForm.Attack.Base = (int)stats[1]["base_stat"] / 10;
            evolvedForm.Defense.Base = (int)stats[2]["base_stat"] / 10;
            evolvedForm.SpecialAttack.Base = (int)stats[3]["base_stat"] / 10;
            evolvedForm.SpecialDefense.Base = (int)stats[4]["base_stat"] / 10;
            evolvedForm.Speed.Base = (int)stats[5]["base_stat"] / 10;
        }

        private static string GetUpdatedNicknameForEvolvedPokemon(
            string currentFormNickname,
            string currentFormName,
            string evolvedFormName)
        {
            var isNicknameUnchanged = string.Equals
            (
                currentFormNickname,
                currentFormName,
                StringComparison.CurrentCultureIgnoreCase
            );

            if (isNicknameUnchanged)
            {
                return evolvedFormName.ToUpper();
            }

            return null;
        }

        private static string CheckGetEvolvedInputs(
            PokemonModel currentForm,
            string nextForm)
        {
            CheckedGetEvolvedInput
            (
                currentForm,
                nextForm
            );

            var pokemon = InvokePokeAPI($"pokemon-species/{currentForm.DexNo}");
            var isDataValid = ValidateEvolutionDataFromPokeAPI
            (
                pokemon,
                nextForm
            );
            if (!isDataValid)
            {
                return null;
            }

            return (string)pokemon["name"];
        }

        private static bool ValidateEvolutionDataFromPokeAPI(
            JToken pokemon,
            string nextForm)
        {
            if (pokemon == null)
            {
                return false;
            }
            var pokemonName = (string)pokemon["name"];
            return GetEvolutionChain(pokemon, pokemonName)?
                ["evolves_to"]?
                .FirstOrDefault(possible => ValidatePokemonSpeciesName(possible, nextForm)) != null;
        }

        private static void CheckedGetEvolvedInput(
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
        }

        private static void CheckGetPokemonInputs(
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
        }

        private static Gender GetRandomizerGender(string pokemonName)
        {
            var genderRate = (int)InvokePokeAPI($"pokemon-species/{pokemonName.ToLower()}")["gender_rate"];
            return genderRate switch
            {
                -1 => Gender.Genderless,
                0 => Gender.Male,
                8 => Gender.Female,
                _ => Random.Next(8) >= genderRate
                    ? Gender.Male
                    : Gender.Female
            };
        }

        private static void AddStats(
            PokemonModel pokemon,
            JToken stats,
            Nature nature)
        {
            var modifier = Attribute.GetCustomAttribute(typeof(Nature).GetField(nature.ToString()), typeof(NatureModifierAttribute)) as NatureModifierAttribute;
            pokemon.HP = GetStat(stats, "hp", modifier.HpModifier);
            pokemon.Attack = GetStat(stats, "attack", modifier.AttackModifier);
            pokemon.Defense = GetStat(stats, "defense", modifier.DefenseModifier);
            pokemon.SpecialAttack = GetStat(stats, "special-attack", modifier.SpecialAttackModifier);
            pokemon.SpecialDefense = GetStat(stats, "special-defense", modifier.DefenseModifier);
            pokemon.Speed = GetStat(stats, "speed", modifier.SpeedModifier);
        }

        private static PokemonStatModel GetStat(
            JToken stats,
            string statName,
            int natureModifier)
        {
            var correctStat = stats.First(stat => (string)stat["stat"]["name"] == statName);
            return new PokemonStatModel
            {
                Nature = natureModifier,
                Base = ((int)correctStat["base_stat"]) / 10
            };
        }

        private static PokemonTypes AggregatePokemonTypes(
            PokemonTypes currentTypes,
            JToken nextType)
        {
            Enum.TryParse
            (
                (string)nextType["type"]["name"],
                true,
                out PokemonTypes result
            );

            return currentTypes | result;
        }

        private static JToken GetEvolutionChain(
            JToken pokemon,
            string name)
        {
            JToken evolutionChain = InvokePokeAPI(CreateHttpRequest((string)pokemon["evolution_chain"]["url"]))?["chain"];

            if (evolutionChain == null)
            {
                return null;
            }

            if (ValidatePokemonSpeciesName(evolutionChain, name))
            {
                return evolutionChain;
            }

            return SearchEvolvesTo(evolutionChain, name);
        }

        private static JToken SearchEvolvesTo(
            JToken evolutionChain,
            string name)
        {
            JToken found = null;
            foreach (var possible in evolutionChain["evolves_to"].TakeWhile(possible => found == null))
            {
                var possibleName = (string)possible["species"]["name"];
                if (ValidatePokemonSpeciesName(possible, name))
                {
                    found = possible;
                    break;
                }

                found = SearchEvolvesTo(possible, name);
            }

            return found;
        }

        private static bool ValidatePokemonSpeciesName(
            JToken pokemon,
            string name)
        {
            return string.Equals
            (
                (string)pokemon["species"]["name"],
                name,
                StringComparison.CurrentCultureIgnoreCase
            );
        }

        private static JObject InvokePokeAPI(
            string endpoint,
            string query)
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
