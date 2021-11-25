using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TheReplacement.PTA.Common.Enums;
using TheReplacement.PTA.Common.Exceptions;
using TheReplacement.PTA.Common.Models;
using TheReplacement.PTA.Common.Utilities;

namespace MongoDbImportTool.Builders
{
    public static class BasePokemonBuilder
    {
        private static readonly IEnumerable<string> FirstThreeMoves = new[]
        {
            "Move 1",
            "Move 2",
            "Move 3"
        };
        private static readonly IEnumerable<string> SecondThreeMoves = new[]
        {
            "Move 4",
            "Move 5",
            "Move 6"
        };

        private static int _incrementor = 0;

        public static BasePokemon Build(JToken pokemonToken)
        {
            _incrementor++;

            var pokemon = new BasePokemon
            {
                Name = BuildPokemonName(pokemonToken),
                BaseFormName = BuildBaseForm(pokemonToken),
                Type = BuildType(pokemonToken).ToString().Replace('_', '/'),
                Skills = BuildSkills(pokemonToken),
                PokemonStats = BuildStats(pokemonToken),
                Moves = BuildMoves(pokemonToken),
                Passives = BuildPassives(pokemonToken),
                Proficiencies = BuildProficiencies(pokemonToken),
                EggGroups = BuildEggGroups(pokemonToken),
                Habitats = BuildHabitats(pokemonToken),
                Diet = BuildDiet(pokemonToken),
                Rarity = BuildRarity(pokemonToken),
                EggHatchRate = BuildEggHatchRate(pokemonToken),
                EvolvesFrom = BuildEvolvesFrom(pokemonToken),
                Stage = BuildStage(pokemonToken),
                LegendaryStats = BuildLegendaryStats(pokemonToken),
                DexNo = _incrementor
            };

            (pokemon.Size, pokemon.Weight) = GetSizeAndWeight(pokemonToken);
            pokemon.SpecialFormName = BuildSpecialFormName(pokemonToken, pokemon.BaseFormName);
            pokemon.GMaxMove = BuildGmaxMove(pokemonToken, pokemon.SpecialFormName);

            return pokemon;
        }

        private static string BuildBaseForm(JToken pokemonToken)
        {
            return GetStringFromTokenOrDefault(pokemonToken, "Base Form");
        }

        private static string BuildPokemonName(JToken pokemonToken)
        {
            return GetStringFromToken(pokemonToken, "Pokemon");
        }

        private static StatsModel BuildStats(JToken pokemonToken)
        {
            return new StatsModel
            {
                HP = (int)pokemonToken["HP"],
                Attack = (int)pokemonToken["Attack"],
                Defense = (int)pokemonToken["Defense"],
                SpecialAttack = (int)pokemonToken["Special Attack"],
                SpecialDefense = (int)pokemonToken["Special Defense"],
                Speed = (int)pokemonToken["Speed"],
            };
        }

        private static PokemonTypes BuildType(JToken pokemonToken)
        {
            return pokemonToken.Children<JProperty>()
                .Where(child => child.Name.StartsWith("Type"))
                .Select(child => child.Value.ToString())
                .Aggregate(PokemonTypes.None, (prev, curr) =>
                {
                    if (string.IsNullOrEmpty(curr))
                    {
                        return prev;
                    }

                    if (!Enum.TryParse(curr, out PokemonTypes type))
                    {
                        throw new ArgumentOutOfRangeException
                        (
                            "type",
                            curr,
                            $"Expected one of {string.Join(',', Enum.GetNames(typeof(PokemonTypes)))}"
                        );
                    }

                    return prev | type;
                });
        }

        private static (string Size, string Weight) GetSizeAndWeight(JToken pokemonToken)
        {
            var size = pokemonToken["Size"]?.ToString();
            if (!IsStringWithValue(size))
            {
                throw new MissingJsonPropertyException("Size");
            }

            var weight = pokemonToken["Weight"]?.ToString();
            if (!IsStringWithValue(weight))
            {
                throw new MissingJsonPropertyException("Weight");
            }

            if (!Enum.TryParse(size, out Size _))
            {
                throw new InvalidJsonPropertyException
                (
                    (JProperty)pokemonToken["Size"],
                    Enum.GetNames(typeof(Size))
                );
            }

            if (!Enum.TryParse(weight, out Weight _))
            {
                throw new InvalidJsonPropertyException
                (
                    (JProperty)pokemonToken["Weight"],
                    Enum.GetNames(typeof(Weight))
                );
            }

            return (size, weight);
        }

        private static IEnumerable<string> BuildMoves(JToken pokemonToken)
        {
            return pokemonToken.Children<JProperty>()
                .Where(child => FirstThreeMoves.Contains(child.Name))
                .Select(child => child.Value.ToString())
                .Where(IsStringWithValue);
        }

        private static IEnumerable<string> BuildSkills(JToken pokemonToken)
        {
            return pokemonToken.Children<JProperty>()
                .Where(child => child.Name.StartsWith("Skill"))
                .Select(child => child.Value.ToString())
                .Where(IsStringWithValue);
        }

        private static IEnumerable<string> BuildPassives(JToken pokemonToken)
        {
            return pokemonToken.Children<JProperty>()
                .Where(child => child.Name.StartsWith("Passive"))
                .Select(child => child.Value.ToString())
                .Where(IsStringWithValue);
        }

        private static IEnumerable<string> BuildProficiencies(JToken pokemonToken)
        {
            return pokemonToken.Children<JProperty>()
                .Where(child => child.Name.StartsWith("Proficiency"))
                .Select(child => child.Value.ToString())
                .Where(IsStringWithValue);
        }

        private static IEnumerable<string> BuildEggGroups(JToken pokemonToken)
        {
            return pokemonToken.Children<JProperty>()
                .Where(child => child.Name.StartsWith("EggGroup"))
                .Select(child => child.Value.ToString())
                .Where(IsStringWithValue);
        }

        private static IEnumerable<string> BuildHabitats(JToken pokemonToken)
        {
            return pokemonToken.Children<JProperty>()
                .Where(child => child.Name.StartsWith("Habitat"))
                .Select(child => child.Value.ToString())
                .Where(IsStringWithValue);
        }

        private static string BuildDiet(JToken pokemonToken)
        {
            return GetStringFromToken(pokemonToken, "Diet");
        }

        private static string BuildRarity(JToken pokemonToken)
        {
            return GetStringFromToken(pokemonToken, "Rarity");
        }

        private static string BuildEggHatchRate(JToken pokemonToken)
        {
            return GetStringFromToken(pokemonToken, "Egg Hatch Rate");
        }

        private static string BuildSpecialFormName(
            JToken pokemonToken,
            string baseForm)
        {
            return string.IsNullOrEmpty(baseForm)
                ? string.Empty
                : GetStringFromToken(pokemonToken, "Special Form Type");
        }

        private static string BuildGmaxMove(
            JToken pokemonToken,
            string specialForm)
        {
            return specialForm == "Gigantamax"
                ? GetStringFromToken(pokemonToken, "Gigantamax Move")
                : string.Empty;
        }

        private static string BuildEvolvesFrom(JToken pokemonToken)
        {
            return GetStringFromTokenOrDefault(pokemonToken, "Evolves From");
        }

        private static int BuildStage(JToken pokemonToken)
        {
            return GetIntFromToken(pokemonToken, "Stage");
        }

        private static LegendaryStatsModel BuildLegendaryStats(JToken pokemonToken)
        {
            var hp = GetStringFromTokenOrDefault(pokemonToken, "Legendary HP");
            if (string.IsNullOrEmpty(hp))
            {
                return LegendaryStatsModel.GetNonLegendaryStats();
            }

            return new LegendaryStatsModel
            {
                HP = int.Parse(hp),
                Moves = pokemonToken.Children<JProperty>()
                .Where(child => SecondThreeMoves.Contains(child.Name))
                .Select(child => child.Value.ToString())
                .Where(IsStringWithValue),
                LegendaryMoves = pokemonToken.Children<JProperty>()
                .Where(child => child.Name.StartsWith("Legendary Move"))
                .Select(child => child.Value.ToString())
                .Where(IsStringWithValue),
                Passives = pokemonToken.Children<JProperty>()
                .Where(child => child.Name.StartsWith("Legendary Passive"))
                .Select(child => child.Value.ToString())
                .Where(IsStringWithValue),
                Features = pokemonToken.Children<JProperty>()
                .Where(child => child.Name.StartsWith("Legendary Feature"))
                .Select(child => child.Value.ToString())
                .Where(IsStringWithValue),
            };
        }

        private static string GetStringFromTokenOrDefault(
            JToken token,
            string property)
        {
            return token[property]?.ToString() ?? string.Empty;
        }

        private static string GetStringFromToken(
            JToken token,
            string property)
        {
            return token[property]?.ToString()
                ?? throw new MissingJsonPropertyException(property);
        }

        private static int GetIntFromToken(
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

        private static bool IsStringWithValue(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }
    }
}
