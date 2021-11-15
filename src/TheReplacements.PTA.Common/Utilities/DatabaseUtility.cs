using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TheReplacements.PTA.Common.Models;

namespace TheReplacements.PTA.Common.Utilities
{
    /// <summary>
    /// Provides a Collection of CRUD methods for the PTA database
    /// </summary>
    public static class DatabaseUtility
    {
        /// <summary>
        /// Searches for a game using its id, then deletes it
        /// </summary>
        /// <param name="id">The game session id</param>
        public static bool DeleteGame(string id)
        {
            return MongoCollectionHelper
                .Game
                .FindOneAndDelete(game => game.GameId == id) != null;
        }

        /// <summary>
        /// Searches for an npc using its id, then deletes it
        /// </summary>
        /// <param name="id">The npc id</param>
        public static bool DeleteNpc(string id)
        {
            return MongoCollectionHelper
                .Npc
                .FindOneAndDelete(npc => npc.NPCId == id) != null;
        }

        /// <summary>
        /// Searches for a Pokemon using its id, then deletes it
        /// </summary>
        /// <param name="id">The Pokemon id</param>
        public static bool DeletePokemon(string id)
        {
            return MongoCollectionHelper
                .Pokemon
                .FindOneAndDelete(Pokemon => Pokemon.PokemonId == id) != null;
        }

        /// <summary>
        /// Searches for all Pokemon using their trainer id, then deletes it
        /// </summary>
        /// <param name="trainerId">The trainer id</param>
        public static long DeletePokemonByTrainerId(string trainerId)
        {
            var deleteResult = MongoCollectionHelper
                .Pokemon
                .DeleteMany(Pokemon => Pokemon.TrainerId == trainerId);
            
            return deleteResult.IsAcknowledged
                ? deleteResult.DeletedCount
                : -1;
        }

        /// <summary>
        /// Searches for a trainer using their id, then deletes it
        /// </summary>
        /// <param name="id">The trainer id</param>
        public static bool DeleteTrainer(string id)
        {
            return MongoCollectionHelper
                .Trainer
                .FindOneAndDelete(trainer => trainer.TrainerId == id) != null;
        }

        /// <summary>
        /// Searches for all trainers using their game id, then deletes it
        /// </summary>
        /// <param name="gameId">The game id</param>
        public static long DeleteTrainersByGameId(string gameId)
        {
            var deleteResult =  MongoCollectionHelper
                .Trainer
                .DeleteMany(trainer => trainer.GameId == gameId);
            
            return deleteResult.IsAcknowledged
                ? deleteResult.DeletedCount
                : -1;
        }

        /// <summary>
        /// Returns a game matching the game session id
        /// </summary>
        /// <param name="id">The game session id</param>
        public static GameModel FindGame(string id)
        {
            return MongoCollectionHelper
                .Game
                .Find(game => game.GameId == id)
                .FirstOrDefault();
        }


        /// <summary>
        /// Returns an npc matching the npc id
        /// </summary>
        /// <param name="id">The npc id</param>
        public static NpcModel FindNpc(string id)
        {
            return MongoCollectionHelper
                .Npc
                .Find(npc => npc.NPCId == id)
                .FirstOrDefault();
        }


        /// <summary>
        /// Returns all npcs matching the npc ids
        /// </summary>
        /// <param name="npcIds">The npc ids</param>
        public static IEnumerable<NpcModel> FindNpcs(IEnumerable<string> npcIds)
        {
            return npcIds == null
                ? throw new ArgumentNullException(nameof(npcIds))
                : MongoCollectionHelper
                .Npc
                .Find(npc => npcIds.Contains(npc.NPCId))
                .ToList();
        }

        /// <summary>
        /// Returns a Pokemon matching the Pokemon id
        /// </summary>
        /// <param name="id">The Pokemon id</param>
        public static PokemonModel FindPokemonById(string id)
        {
            return MongoCollectionHelper
                .Pokemon
                .Find(Pokemon => Pokemon.PokemonId == id)
                .FirstOrDefault();
        }

        /// <summary>
        /// Returns all Pokemon matching the trainer id
        /// </summary>
        /// <param name="trainerId">The trainer id</param>
        public static IEnumerable<PokemonModel> FindPokemonByTrainerId(string trainerId)
        {
            return MongoCollectionHelper
                .Pokemon
                .Find(Pokemon => Pokemon.TrainerId == trainerId)
                .ToList();
        }

        /// <summary>
        /// Returns a trainer matching the trainer id
        /// </summary>
        /// <param name="id">The trainer id</param>
        public static TrainerModel FindTrainerById(string id)
        {
            return MongoCollectionHelper
                .Trainer
                .Find(trainer => trainer.TrainerId == id)
                .FirstOrDefault();
        }

        /// <summary>
        /// Returns all trainers matching the game session id
        /// </summary>
        /// <param name="gameId">The game session id</param>
        public static IEnumerable<TrainerModel> FindTrainersByGameId(string gameId)
        {
            return MongoCollectionHelper
                .Trainer
                .Find(trainer => trainer.GameId == gameId)
                .ToList();
        }

        /// <summary>
        /// Returns a trainer matching the trainer name and game session id
        /// </summary>
        /// <param name="username">The trainer name</param>
        /// <param name="gameId">The game session id</param>
        public static TrainerModel FindTrainerByUsername(
            string username,
            string gameId)
        {
            Expression<Func<TrainerModel, bool>> filter = trainer => trainer.TrainerName == username && trainer.GameId == gameId;
            return MongoCollectionHelper
                .Trainer
                .Find(filter)
                .FirstOrDefault();
        }

        /// <summary>
        /// Returns whether there is a game master for the provide game session
        /// </summary>
        /// <param name="gameId">The game session id</param>
        public static bool HasGM(string gameId)
        {
            return FindGame(gameId) != null && MongoCollectionHelper
                .Trainer
                .Find(trainer => trainer.IsGM && trainer.GameId == gameId)
                .Any();
        }

        /// <summary>
        /// Encrypts a password for storage
        /// </summary>
        /// <param name="password">The game session id</param>
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        /// <summary>
        /// Attempts to add a game using the provided document
        /// </summary>
        /// <param name="game">The document to add</param>
        /// <param name="error">Any error found</param>
        public static bool TryAddGame(
            GameModel game,
            out object error)
        {
            return TryAddDocument
            (
                () => MongoCollectionHelper.Game.InsertOne(game),
                out error
            );
        }

        /// <summary>
        /// Attempts to add an npc using the provided document
        /// </summary>
        /// <param name="npc">The document to add</param>
        /// <param name="error">Any error found</param>
        public static bool TryAddNpc(
            NpcModel npc,
            out object error)
        {
            return TryAddDocument
            (
                () => MongoCollectionHelper.Npc.InsertOne(npc),
                out error
            );
        }

        /// <summary>
        /// Attempts to add a Pokemon using the provided document
        /// </summary>
        /// <param name="pokemon">The document to add</param>
        /// <param name="error">Any error found</param>
        public static bool TryAddPokemon(
            PokemonModel pokemon,
            out object error)
        {
            return TryAddDocument
            (
                () => MongoCollectionHelper.Pokemon.InsertOne(pokemon),
                out error
            );
        }

        /// <summary>
        /// Attempts to add a trainer using the provided document
        /// </summary>
        /// <param name="trainer">The document to add</param>
        /// <param name="error">Any error found</param>
        public static bool TryAddTrainer(
            TrainerModel trainer,
            out object error)
        {
            return TryAddDocument
            (
                () => MongoCollectionHelper.Trainer.InsertOne(trainer),
                out error
            );
        }

        /// <summary>
        /// Searches for a game, then updates the npc list
        /// </summary>
        /// <param name="gameId">The game session id</param>
        /// <param name="npcIds">The updated npc list</param>
        /// <exception cref="MongoCommandException" />
        public static bool UpdateGameNpcList(string gameId, IEnumerable<string> npcIds)
        {
            return MongoCollectionHelper
                .Game
                .FindOneAndUpdate
                (
                    game => game.GameId == gameId,
                    Builders<GameModel>.Update.Set("NPCs", npcIds)
                ) != null;
        }

        /// <summary>
        /// Searches for a game, then updates its online status
        /// </summary>
        /// <param name="gameId">The game session id</param>
        /// <param name="isOnline">The updated online status</param>
        /// <exception cref="MongoCommandException" />
        public static bool UpdateGameOnlineStatus(
            string gameId,
            bool isOnline)
        {
            Expression<Func<GameModel, bool>> filter = game => game.GameId == gameId;
            var update = Builders<GameModel>.Update.Set("IsOnline", isOnline);
            return MongoCollectionHelper
                .Game
                .FindOneAndUpdate(filter, update) != null;
        }

        /// <summary>
        /// Searches for a pokemon, then updates its stats
        /// </summary>
        /// <param name="pokemonId">The pokemon id</param>
        /// <param name="query">The updates to perform</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="MongoCommandException" />
        public static bool UpdatePokemonStats(
            string pokemonId,
            Dictionary<string, string> query)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            Expression<Func<PokemonModel, bool>> filter = Pokemon => Pokemon.PokemonId == pokemonId;
            var updates = GetPokemonUpdates(pokemonId, query);
            return MongoCollectionHelper
                .Pokemon
                .FindOneAndUpdate(filter, updates) != null;
        }

        /// <summary>
        /// Searches for a pokemon, then updates its trainer id
        /// </summary>
        /// <param name="pokemonId">The pokemon id</param>
        /// <param name="trainerId">The trainer id/param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="MongoCommandException" />
        public static bool UpdatePokemonTrainerId(
            string pokemonId,
            string trainerId)
        {
            Expression<Func<PokemonModel, bool>> filter = Pokemon => Pokemon.PokemonId == pokemonId;
            var update = Builders<PokemonModel>.Update.Set("TrainerId", trainerId);
            return MongoCollectionHelper
                .Pokemon
                .FindOneAndUpdate(filter, update) != null;
        }

        /// <summary>
        /// Searches for a pokemon, then evolves it to its next stage
        /// </summary>
        /// <param name="pokemonId">The pokemon id</param>
        /// <param name="evolvedForm">The pokemon's evolution/param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="MongoCommandException" />
        public static bool UpdatePokemonWithEvolution(
            string pokemonId,
            PokemonModel evolvedForm)
        {
            if (evolvedForm == null)
            {
                throw new ArgumentNullException(nameof(pokemonId));
            }

            var updates = new[]
            {
                Builders<PokemonModel>.Update.Set("DexNo", evolvedForm.DexNo),
                Builders<PokemonModel>.Update.Set("HP", evolvedForm.HP),
                Builders<PokemonModel>.Update.Set("Attack", evolvedForm.Attack),
                Builders<PokemonModel>.Update.Set("Defense", evolvedForm.Defense),
                Builders<PokemonModel>.Update.Set("SpecialAttack", evolvedForm.SpecialAttack),
                Builders<PokemonModel>.Update.Set("SpecialDefense", evolvedForm.SpecialDefense),
                Builders<PokemonModel>.Update.Set("Speed", evolvedForm.Speed),
                Builders<PokemonModel>.Update.Set("Nickname", evolvedForm.Nickname)
            };

            Expression<Func<PokemonModel, bool>> filter = Pokemon => Pokemon.PokemonId == pokemonId;
            var update = Builders<PokemonModel>.Update.Combine(updates);
            return MongoCollectionHelper
                .Pokemon
                .FindOneAndUpdate(filter, update) != null;
        }

        /// <summary>
        /// Searches for a trainer, then updates their item list
        /// </summary>
        /// <param name="trainerId">The trainer id/param>
        /// <param name="itemList">The updated item list</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="MongoCommandException" />
        public static bool UpdateTrainerItemList(
            string trainerId,
            IEnumerable<ItemModel> itemList)
        {
            if (itemList == null)
            {
                throw new ArgumentNullException(nameof(itemList));
            }

            Expression<Func<TrainerModel, bool>> filter = trainer => trainer.TrainerId == trainerId;
            var update = Builders<TrainerModel>.Update.Set
            (
                "Items",
                itemList
            );

            return MongoCollectionHelper
                .Trainer
                .FindOneAndUpdate(filter, update) != null;
        }

        /// <summary>
        /// Searches for a trainer, then updates their online status
        /// </summary>
        /// <param name="trainerId">The trainer id/param>
        /// <param name="isOnline">The updated online status</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="MongoCommandException" />
        public static bool UpdateTrainerOnlineStatus(
            string trainerId,
            bool isOnline)
        {
            Expression<Func<TrainerModel, bool>> filter = trainer => trainer.TrainerId == trainerId;
            var update = Builders<TrainerModel>.Update.Set("IsOnline", isOnline);
            return MongoCollectionHelper
                .Trainer
                .FindOneAndUpdate(filter, update) != null;
        }

        /// <summary>
        /// Searches for a trainer, then updates their password
        /// </summary>
        /// <param name="trainerId">The trainer id/param>
        /// <param name="password">The updated password</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="MongoCommandException" />
        public static bool UpdateTrainerPassword(
            string trainerId,
            string password)
        {
            Expression<Func<TrainerModel, bool>> filter = trainer => trainer.TrainerId == trainerId;
            var update = Builders<TrainerModel>
                .Update
                .Combine(new[]
                {
                    Builders<TrainerModel>.Update.Set("PasswordHash", HashPassword(password)),
                    Builders<TrainerModel>.Update.Set("IsOnline", true)
                });
            return MongoCollectionHelper
                .Trainer
                .FindOneAndUpdate(filter, update) != null;
        }

        /// <summary>
        /// Verifys that the password matches the encryption
        /// </summary>
        /// <param name="password">The trainer id/param>
        /// <param name="hashPassword">The updated online status</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="MongoCommandException" />
        public static bool VerifyTrainerPassword(
            string password,
            string hashPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashPassword);
        }

        private static bool TryAddDocument(Action action, out object error)
        {
            try
            {
                action();
                error = null;
                return true;
            }
            catch (MongoWriteException exception)
            {
                error = new { writeErrorJsonString = exception.WriteError.Details.GetValue("details").AsBsonDocument.ToString() };
                return false;
            }
        }

        private static UpdateDefinition<PokemonModel> GetPokemonUpdates(string PokemonId, Dictionary<string, string> query)
        {
            var Pokemon = FindPokemonById(PokemonId);
            var updates = new List<UpdateDefinition<PokemonModel>>();
            var currentKey = string.Empty;
            if (query.TryGetValue("experience", out currentKey) && int.TryParse(currentKey, out var experience))
            {
                updates.Add(Builders<PokemonModel>.Update.Set("Experience", experience));
            }
            if (query.TryGetValue("hpAdded", out currentKey) && int.TryParse(currentKey, out var added))
            {
                Pokemon.HP.Added = added;
                updates.Add(Builders<PokemonModel>.Update.Set("HP", Pokemon.HP));
            }
            if (query.TryGetValue("attackAdded", out currentKey) && int.TryParse(currentKey, out added))
            {
                Pokemon.Attack.Added = added;
                updates.Add(Builders<PokemonModel>.Update.Set("Attack", Pokemon.Attack));
            }
            if (query.TryGetValue("defenseAdded", out currentKey) && int.TryParse(currentKey, out added))
            {
                Pokemon.Defense.Added = added;
                updates.Add(Builders<PokemonModel>.Update.Set("Defense", Pokemon.Defense));
            }
            if (query.TryGetValue("specialAttackAdded", out currentKey) && int.TryParse(currentKey, out added))
            {
                Pokemon.SpecialAttack.Added = added;
                updates.Add(Builders<PokemonModel>.Update.Set("SpecialAttack", Pokemon.SpecialAttack));
            }
            if (query.TryGetValue("specialDefenseAdded", out currentKey) && int.TryParse(currentKey, out added))
            {
                Pokemon.SpecialDefense.Added = added;
                updates.Add(Builders<PokemonModel>.Update.Set("SpecialDefense", Pokemon.SpecialDefense));
            }
            if (query.TryGetValue("speedAdded", out currentKey) && int.TryParse(currentKey, out added))
            {
                Pokemon.Speed.Added = added;
                updates.Add(Builders<PokemonModel>.Update.Set("Speed", Pokemon.Speed));
            }
            if (query.TryGetValue("nickname", out currentKey) && !string.IsNullOrWhiteSpace(currentKey))
            {
                updates.Add(Builders<PokemonModel>.Update.Set("Nickname", query["nickname"]));
            }

            return updates.Any()
                ? Builders<PokemonModel>.Update.Combine(updates.ToArray())
                : null;
        }
    }
}
