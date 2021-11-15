using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TheReplacements.PTA.Common.Models;

namespace TheReplacements.PTA.Common.Utilities
{
    public static class DatabaseUtility
    {
        public static bool DeleteGame(string id)
        {
            return MongoCollectionHelper
                .Game
                .FindOneAndDelete(game => game.GameId == id) != null;
        }

        public static bool DeleteNpc(string id)
        {
            return MongoCollectionHelper
                .Npc
                .FindOneAndDelete(npc => npc.NPCId == id) != null;
        }

        public static bool DeletePokemon(string id)
        {
            return MongoCollectionHelper
                .Pokemon
                .FindOneAndDelete(pokemon => pokemon.PokemonId == id) != null;
        }

        public static long DeletePokemonByTrainerId(string trainerId)
        {
            var deleteResult = MongoCollectionHelper
                .Pokemon
                .DeleteMany(pokemon => pokemon.TrainerId == trainerId);
            
            return deleteResult.IsAcknowledged
                ? deleteResult.DeletedCount
                : -1;
        }

        public static bool DeleteTrainer(string id)
        {
            return MongoCollectionHelper
                .Trainer
                .FindOneAndDelete(trainer => trainer.TrainerId == id) != null;
        }

        public static long DeleteTrainersByGameId(string gameId)
        {
            var deleteResult =  MongoCollectionHelper
                .Trainer
                .DeleteMany(trainer => trainer.GameId == gameId);
            
            return deleteResult.IsAcknowledged
                ? deleteResult.DeletedCount
                : -1;
        }

        public static GameModel FindGame(string id)
        {
            return MongoCollectionHelper
                .Game
                .Find(game => game.GameId == id)
                .FirstOrDefault();
        }

        public static NpcModel FindNpc(string id)
        {
            return MongoCollectionHelper
                .Npc
                .Find(npc => npc.NPCId == id)
                .FirstOrDefault();
        }

        public static IEnumerable<NpcModel> FindNpcs(IEnumerable<string> npcIds)
        {
            return npcIds == null
                ? throw new ArgumentNullException(nameof(npcIds))
                : MongoCollectionHelper
                .Npc
                .Find(npc => npcIds.Contains(npc.NPCId))
                .ToList();
        }
        
        public static PokemonModel FindPokemonById(string id)
        {
            return MongoCollectionHelper
                .Pokemon
                .Find(pokemon => pokemon.PokemonId == id)
                .FirstOrDefault();
        }

        public static IEnumerable<PokemonModel> FindPokemonByTrainerId(string trainerId)
        {
            return MongoCollectionHelper
                .Pokemon
                .Find(pokemon => pokemon.TrainerId == trainerId)
                .ToList();
        }

        public static TrainerModel FindTrainerById(string id)
        {
            return MongoCollectionHelper
                .Trainer
                .Find(trainer => trainer.TrainerId == id)
                .FirstOrDefault();
        }

        public static IEnumerable<TrainerModel> FindTrainersByGameId(string gameId)
        {
            return MongoCollectionHelper
                .Trainer
                .Find(trainer => trainer.GameId == gameId)
                .ToList();
        }

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

        public static bool HasGM(string gameId)
        {
            return FindGame(gameId) != null && MongoCollectionHelper
                .Trainer
                .Find(trainer => trainer.IsGM && trainer.GameId == gameId)
                .Any();
        }

        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

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
        /// 
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="npcIds"></param>
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
        /// 
        /// </summary>
        /// <param name="pokemonId"></param>
        /// <param name="query"></param>
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

            Expression<Func<PokemonModel, bool>> filter = pokemon => pokemon.PokemonId == pokemonId;
            var updates = GetPokemonUpdates(pokemonId, query);
            return MongoCollectionHelper
                .Pokemon
                .FindOneAndUpdate(filter, updates) != null;
        }

        public static bool UpdatePokemonTrainerId(
            string pokemonId,
            string trainerId)
        {
            Expression<Func<PokemonModel, bool>> filter = pokemon => pokemon.PokemonId == pokemonId;
            var update = Builders<PokemonModel>.Update.Set("TrainerId", trainerId);
            return MongoCollectionHelper
                .Pokemon
                .FindOneAndUpdate(filter, update) != null;
        }

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

            Expression<Func<PokemonModel, bool>> filter = pokemon => pokemon.PokemonId == pokemonId;
            var update = Builders<PokemonModel>.Update.Combine(updates);
            return MongoCollectionHelper
                .Pokemon
                .FindOneAndUpdate(filter, update) != null;
        }

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

        private static UpdateDefinition<PokemonModel> GetPokemonUpdates(string pokemonId, Dictionary<string, string> query)
        {
            var pokemon = FindPokemonById(pokemonId);
            var updates = new List<UpdateDefinition<PokemonModel>>();
            var currentKey = string.Empty;
            if (query.TryGetValue("experience", out currentKey) && int.TryParse(currentKey, out var experience))
            {
                updates.Add(Builders<PokemonModel>.Update.Set("Experience", experience));
            }
            if (query.TryGetValue("hpAdded", out currentKey) && int.TryParse(currentKey, out var added))
            {
                pokemon.HP.Added = added;
                updates.Add(Builders<PokemonModel>.Update.Set("HP", pokemon.HP));
            }
            if (query.TryGetValue("attackAdded", out currentKey) && int.TryParse(currentKey, out added))
            {
                pokemon.Attack.Added = added;
                updates.Add(Builders<PokemonModel>.Update.Set("Attack", pokemon.Attack));
            }
            if (query.TryGetValue("defenseAdded", out currentKey) && int.TryParse(currentKey, out added))
            {
                pokemon.Defense.Added = added;
                updates.Add(Builders<PokemonModel>.Update.Set("Defense", pokemon.Defense));
            }
            if (query.TryGetValue("specialAttackAdded", out currentKey) && int.TryParse(currentKey, out added))
            {
                pokemon.SpecialAttack.Added = added;
                updates.Add(Builders<PokemonModel>.Update.Set("SpecialAttack", pokemon.SpecialAttack));
            }
            if (query.TryGetValue("specialDefenseAdded", out currentKey) && int.TryParse(currentKey, out added))
            {
                pokemon.SpecialDefense.Added = added;
                updates.Add(Builders<PokemonModel>.Update.Set("SpecialDefense", pokemon.SpecialDefense));
            }
            if (query.TryGetValue("speedAdded", out currentKey) && int.TryParse(currentKey, out added))
            {
                pokemon.Speed.Added = added;
                updates.Add(Builders<PokemonModel>.Update.Set("Speed", pokemon.Speed));
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
