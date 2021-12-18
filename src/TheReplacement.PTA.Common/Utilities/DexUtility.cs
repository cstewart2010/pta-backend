using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheReplacement.PTA.Common.Enums;
using TheReplacement.PTA.Common.Interfaces;
using TheReplacement.PTA.Common.Internal;
using TheReplacement.PTA.Common.Models;

namespace TheReplacement.PTA.Common.Utilities
{
    /// <summary>
    /// Provides a collection of methods for performing CRUD interactions on Dex Collections
    /// </summary>
    public static class DexUtility
    {
        /// <summary>
        /// Attempts to evolve a pokemon to its next stage
        /// </summary>
        /// <param name="pokemon">The current form</param>
        /// <param name="keptMoves">The moves you wish to keep</param>
        /// <param name="evolvedName">The name of the evolved form</param>
        /// <param name="newMoves">The moves you wish to add</param>
        public static PokemonModel GetEvolved(
            PokemonModel pokemon,
            IEnumerable<string> keptMoves,
            string evolvedName,
            IEnumerable<string> newMoves)
        {
            if (pokemon == null) throw ExceptionHandler.ArgumentNull(nameof(pokemon));
            if (keptMoves == null) throw ExceptionHandler.ArgumentNull(nameof(keptMoves));
            if (string.IsNullOrEmpty(evolvedName)) throw ExceptionHandler.IsNullOrEmpty(nameof(evolvedName));
            if (evolvedName == null) throw ExceptionHandler.ArgumentNull(nameof(evolvedName));

            var basePokemon = GetDexEntry<BasePokemonModel>(DexType.BasePokemon, evolvedName);
            if (!string.Equals(basePokemon?.EvolvesFrom, pokemon.SpeciesName, StringComparison.CurrentCultureIgnoreCase))
            {
                return null;
            }

            var evolvedMoves = basePokemon.Moves.Select(move => move.ToLower());
            if (!newMoves.All(move => evolvedMoves.Contains(move.ToLower())))
            {
                return null;
            }

            return new PokemonModel
            {
                PokemonId = pokemon.PokemonId,
                DexNo = basePokemon.DexNo,
                SpeciesName = basePokemon.Name,
                Nickname = pokemon.Nickname,
                Gender = pokemon.Gender,
                PokemonStatus = pokemon.PokemonStatus,
                Moves = keptMoves.Union(newMoves),
                Type = basePokemon.Type,
                CatchRate = GetCatchRate(basePokemon),
                Nature = pokemon.Nature,
                IsShiny = pokemon.IsShiny,
                PokemonStats = basePokemon.PokemonStats,
                Size = basePokemon.Size,
                Weight = basePokemon.Weight,
                Skills = basePokemon.Skills,
                Passives = basePokemon.Passives,
                Proficiencies = basePokemon.Proficiencies,
                EggGroups = basePokemon.EggGroups,
                EggHatchRate = basePokemon.EggHatchRate,
                Habitats = basePokemon.Habitats,
                Diet = basePokemon.Diet,
                Rarity = basePokemon.Rarity,
                GMaxMove = basePokemon.GMaxMove,
                EvolvedFrom = basePokemon.EvolvesFrom,
                LegendaryStats = basePokemon.LegendaryStats
            };
        }

        /// <summary>
        /// Builds a <see cref="PokemonModel"/> using information from the <see cref="BasePokemonModel"/>
        /// </summary>
        /// <param name="name">The pokemon's species name</param>
        /// <param name="nickname">The pokemon's nickname, if applicable</param>
        public static PokemonModel GetNewPokemon(string name, string nickname)
        {
            var random = new Random();
            var nature = (Nature)random.Next(1, 21);
            var gender = (Gender)random.Next(3);
            var status = Status.Normal;
            return GetNewPokemon(name, nature, gender, status, nickname);
        }

        /// <summary>
        /// Builds a <see cref="PokemonModel"/> using information from the <see cref="BasePokemonModel"/>
        /// </summary>
        /// <param name="name">The pokemon's species name</param>
        /// <param name="nature">The nature to give the pokemon</param>
        /// <param name="gender">The pokemon's gender</param>
        /// <param name="status">The pokemon's status</param>
        /// <param name="nickname">The pokemon's nickname, if applicable</param>
        public static PokemonModel GetNewPokemon(
            string name,
            Nature nature,
            Gender gender,
            Status status,
            string nickname)
        {
            var basePokemon = GetDexEntry<BasePokemonModel>(DexType.BasePokemon, name);
            return GetPokemonFromBase(basePokemon, nature, gender, status, nickname);
        }

        /// <summary>
        /// Returns all Dex extries for a specific Dex collection
        /// </summary>
        /// <param name="documentType">The dex collection you wish to return data from</param>
        public static IEnumerable<TDocument> GetDexEntries<TDocument>(DexType documentType) where TDocument : IDexDocument
        {
            var collection = MongoCollectionHelper.Database.GetCollection<TDocument>(documentType.ToString());
            return collection.Find(document => true).ToEnumerable();
        }

        /// <summary>
        /// Returns a specific Dex entry from a specific Dex collection
        /// </summary>
        /// <param name="documentType">The dex collection you wish to return data from</param>
        /// <param name="name">The name of the dex entry</param>
        public static TDocument GetDexEntry<TDocument>(
            DexType documentType,
            string name) where TDocument : IDexDocument
        {
            var collection = MongoCollectionHelper.Database.GetCollection<TDocument>(documentType.ToString());
            return collection.Find(document => document.Name.ToLower() == name.ToLower()).FirstOrDefault();
        }

        /// <summary>
        /// Adds a collection of dex entry to a specific dex collection
        /// </summary>
        /// <param name="collectionName">The name of the collection to add document</param>
        /// <param name="documents">The documents to add to collection</param>
        /// <param name="writer">The writer to write logs to</param>
        public static void AddDexEntries<TDocument>(
            string collectionName,
            IEnumerable<TDocument> documents,
            TextWriter writer) where TDocument : IDexDocument
        {
            var collection = MongoCollectionHelper.Database.GetCollection<TDocument>(collectionName);
            foreach (var document in documents)
            {
                if (collection.Find(currentDocument => document.Name == currentDocument.Name).Any())
                {
                    continue;
                }

                writer.WriteLine($"Adding {document.Name} to {collectionName}");
                var isSuccessful = TryAddDexEntry
                (
                    () => collection.InsertOne(document),
                    out var error
                );

                if (!isSuccessful)
                {
                    writer.WriteLine($"Failed to add {document.Name} to {collectionName}");
                    writer.WriteLine(error.WriteErrorJsonString);
                }
            }
        }

        private static PokemonModel GetPokemonFromBase(
            BasePokemonModel basePokemon,
            Nature nature,
            Gender gender,
            Status status,
            string nickname)
        {
            if (basePokemon == null)
            {
                return null;
            }

            var updatedNickname = string.IsNullOrWhiteSpace(nickname)
                ? basePokemon.Name
                : nickname;

            var modifier = nature.GetNatureModifier();
            var stats = new StatsModel
            {
                HP = basePokemon.PokemonStats.HP,
                Attack = basePokemon.PokemonStats.Attack + modifier.AttackModifier,
                Defense = basePokemon.PokemonStats.Defense + modifier.DefenseModifier,
                SpecialAttack = basePokemon.PokemonStats.SpecialAttack + modifier.SpecialAttackModifier,
                SpecialDefense = basePokemon.PokemonStats.SpecialDefense + modifier.SpecialDefenseModifier,
                Speed = basePokemon.PokemonStats.Speed + modifier.SpeedModifier,
            };

            return new PokemonModel
            {
                PokemonId = Guid.NewGuid().ToString(),
                DexNo = basePokemon.DexNo,
                SpeciesName = basePokemon.Name,
                Nickname = updatedNickname,
                Gender = gender.ToString(),
                PokemonStatus = status.ToString(),
                Moves = basePokemon.Moves,
                Type = basePokemon.Type,
                CatchRate = GetCatchRate(basePokemon),
                Nature = nature.ToString(),
                IsShiny = new Random().Next(420) == 69,
                PokemonStats = stats,
                Size = basePokemon.Size,
                Weight = basePokemon.Weight,
                Skills = basePokemon.Skills,
                Passives = basePokemon.Passives,
                Proficiencies = basePokemon.Proficiencies,
                EggGroups = basePokemon.EggGroups,
                EggHatchRate = basePokemon.EggHatchRate,
                Habitats = basePokemon.Habitats,
                Diet = basePokemon.Diet,
                Rarity = basePokemon.Rarity,
                GMaxMove = basePokemon.GMaxMove,
                EvolvedFrom = basePokemon.EvolvesFrom,
                LegendaryStats = basePokemon.LegendaryStats
            };
        }

        private static int GetCatchRate(BasePokemonModel basePokemon)
        {
            Enum.TryParse(basePokemon.Rarity, true, out Rarity rarity);
            return rarity switch
            {
                Rarity.Common => 50,
                Rarity.Uncommon => 40,
                _ => 30,
            } - (15 * (basePokemon.Stage - 1));
        }

        private static bool TryAddDexEntry(
            Action action,
            out MongoWriteError error)
        {
            try
            {
                action();
                error = null;
                return true;
            }
            catch (MongoWriteException exception)
            {
                error = new MongoWriteError(exception.WriteError.Details.GetValue("details").AsBsonDocument.ToString());
                return false;
            }
        }
    }
}
