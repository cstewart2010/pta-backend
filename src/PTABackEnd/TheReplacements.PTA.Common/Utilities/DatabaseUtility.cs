﻿using BCrypt.Net;
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
        private static readonly MongoCollectionHelper MongoCollectionHelper = new MongoCollectionHelper(27017, "localhost");

        public static bool TryAddGame(GameModel game, out object error)
        {
            return TryAddDocument
            (
                () => MongoCollectionHelper.Game.InsertOne(game),
                out error
            );
        }

        public static bool TryAddNpc(NpcModel npc, out object error)
        {
            return TryAddDocument
            (
                () => MongoCollectionHelper.Npc.InsertOne(npc),
                out error
            );
        }

        public static bool TryAddPokemon(PokemonModel pokemon, out object error)
        {
            return TryAddDocument
            (
                () => MongoCollectionHelper.Pokemon.InsertOne(pokemon),
                out error
            );
        }

        public static bool TryAddTrainer(TrainerModel trainer, out object error)
        {
            return TryAddDocument
            (
                () => MongoCollectionHelper.Trainer.InsertOne(trainer),
                out error
            );
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

        public static bool DeleteTrainer(string id)
        {
            return MongoCollectionHelper
                .Trainer
                .FindOneAndDelete(trainer => trainer.TrainerId == id) != null;
        }

        public static bool DeleteTrainersByGameId(string gameId)
        {
            return MongoCollectionHelper
                .Trainer
                .DeleteMany(trainer => trainer.GameId == gameId).IsAcknowledged;
        }

        public static object DeleteTrainerMons(string trainerId)
        {
            var deleteResult = MongoCollectionHelper
                .Pokemon
                .DeleteMany(pokemon => pokemon.TrainerId == trainerId);

            if (deleteResult.IsAcknowledged)
            {
                return new
                {
                    message = "Successful delete",
                    deleteResult.DeletedCount
                };
            }

            return null;
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
            return MongoCollectionHelper
                .Npc
                .Find(npc => npcIds.Contains(npc.NPCId))
                .ToList();
        }

        public static PokemonModel FindPokemon(Expression<Func<PokemonModel, bool>> filter)
        {
            return MongoCollectionHelper
                .Pokemon
                .Find(filter)
                .FirstOrDefault();
        }
        
        public static PokemonModel FindPokemonById(string id)
        {
            return MongoCollectionHelper
                .Pokemon
                .Find(pokemon => pokemon.PokemonId == id)
                .FirstOrDefault();
        }

        public static TrainerModel FindTrainerById(string id)
        {
            return MongoCollectionHelper
                .Trainer
                .Find(trainer => trainer.TrainerId == id)
                .FirstOrDefault();
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

        public static IEnumerable<TrainerModel> FindTrainersByGameId(string gameId)
        {
            return MongoCollectionHelper
                .Trainer
                .Find(trainer => trainer.GameId == gameId)
                .ToList();
        }

        public static bool HasGM(string gameId)
        {
            return MongoCollectionHelper
                .Trainer
                .Find(trainer => trainer.IsGM && trainer.GameId == gameId)
                .Any();
        }

        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public static bool UpdateGame(string gameId, UpdateDefinition<GameModel> update)
        {
            Expression<Func<GameModel, bool>> filter = game => game.GameId == gameId;
            return MongoCollectionHelper
                .Game
                .FindOneAndUpdate(filter, update) != null;
        }

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

        public static bool UpdatePokemon(FilterDefinition<PokemonModel> filter, UpdateDefinition<PokemonModel> update)
        {
            return MongoCollectionHelper
                .Pokemon
                .FindOneAndUpdate(filter, update) != null;
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

        public static bool UpdateTrainerItemList(
            string trainerId,
            IEnumerable<ItemModel> itemList)
        {
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

        public static bool UpdateTrainersOnlineStatus(string gameId)
        {
            Expression<Func<TrainerModel, bool>> filter = trainer => trainer.GameId == gameId;
            var update = Builders<TrainerModel>.Update.Set("IsOnline", false);
            return MongoCollectionHelper
                .Trainer
                .UpdateMany(filter, update).IsAcknowledged;
        }

        public static TrainerModel UpdateTrainerPassword(
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
                .FindOneAndUpdate(filter, update);
        }

        public static bool VerifyTrainerPassword(
            string password,
            string hashPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashPassword);
        }
    }
}
