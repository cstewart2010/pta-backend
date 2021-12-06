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
    public static class DexUtility
    {
        public static PokemonModel GetEvolved(
            PokemonModel pokemon,
            IEnumerable<string> keptMoves,
            string evolvedName,
            IEnumerable<string> newMoves)
        {
            var basePokemon = GetStaticDocument<BasePokemonModel>(DexType.BasePokemon, evolvedName);
            if (!string.Equals(basePokemon?.EvolvesFrom, pokemon.SpeciesName, StringComparison.CurrentCultureIgnoreCase))
            {
                return null;
            }

            var moveComparer = basePokemon.Moves.Select(move => move.ToLower());
            if (!newMoves.All(move => basePokemon.Moves.Contains(move.ToLower())))
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

        public static PokemonModel GetNewPokemon(
            int dexNo,
            Nature nature,
            Gender gender,
            Status status,
            string nickname)
        {
            var basePokemon = GetBasePokemon(dexNo);
            return GetPokemonFromBase(basePokemon, nature, gender, status, nickname);
        }

        public static PokemonModel GetNewPokemon(
            string name,
            Nature nature,
            Gender gender,
            Status status,
            string nickname)
        {
            var basePokemon = GetStaticDocument<BasePokemonModel>(DexType.BasePokemon, name);
            return GetPokemonFromBase(basePokemon, nature, gender, status, nickname);
        }

        public static IEnumerable<TDocument> GetStaticDocuments<TDocument>(DexType documentType) where TDocument : INamed
        {
            var collection = MongoCollectionHelper.Database.GetCollection<TDocument>(documentType.ToString());
            return collection.Find(document => true).ToEnumerable();
        }

        public static TDocument GetStaticDocument<TDocument>(
            DexType documentType,
            string name) where TDocument : INamed
        {
            var collection = MongoCollectionHelper.Database.GetCollection<TDocument>(documentType.ToString());
            return collection.Find(document => document.Name.ToLower() == name.ToLower()).FirstOrDefault();
        }

        public static void AddStaticDocuments<TDocument>(
            string collectionName,
            IEnumerable<TDocument> documents,
            TextWriter writer) where TDocument : INamed
        {
            var collection = MongoCollectionHelper.Database.GetCollection<TDocument>(collectionName);
            foreach (var document in documents)
            {
                if (collection.Find(currentDocument => document.Name == (currentDocument as INamed).Name).Any())
                {
                    continue;
                }

                writer.WriteLine($"Adding {document.Name} to {collectionName}");
                var isSuccessful = TryAddStaticDocument
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

        private static BasePokemonModel GetBasePokemon(int dexNo)
        {
            var collection = MongoCollectionHelper.Database.GetCollection<BasePokemonModel>("BasePokemon");
            return collection.Find(document => document.DexNo == dexNo).FirstOrDefault();
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

        private static bool TryAddStaticDocument(
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
