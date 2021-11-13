using BCrypt.Net;
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
        private static readonly MongoCollectionHelper MongoCollectionHelper = new(27017, "localhost");

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

        public static object DeletePokemon(TrainerModel trainer)
        {
            Expression<Func<PokemonModel, bool>> pokemonFiler = pokemon => pokemon.TrainerId == trainer.TrainerId;
            string message;
            if (MongoCollectionHelper.Pokemon.DeleteMany(pokemonFiler).IsAcknowledged)
            {
                message = $"Successfully deleted all pokemon";
            }
            else
            {
                message = $"Failed to delete pokemon";
            }
            return new
            {
                message,
                trainerId = trainer.TrainerId
            };
        }

        public static bool DeleteTrainers(FilterDefinition<TrainerModel> filter)
        {
            return MongoCollectionHelper
                .Trainer
                .DeleteMany(filter).IsAcknowledged;
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
        
        public static TrainerModel FindTrainer(Expression<Func<TrainerModel, bool>> filter)
        {
            return MongoCollectionHelper
                .Trainer
                .Find(filter)
                .FirstOrDefault();
        }

        public static IEnumerable<TrainerModel> FindTrainers(FilterDefinition<TrainerModel> filter)
        {
            return MongoCollectionHelper
                .Trainer
                .Find(filter)
                .ToList();
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

        public static GameModel UpdateGame(string gameId, UpdateDefinition<GameModel> update)
        {
            Expression<Func<GameModel, bool>> filter = game => game.GameId == gameId;
            return MongoCollectionHelper
                .Game
                .FindOneAndUpdate(filter, update);
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

        public static TrainerModel UpdateTrainer(FilterDefinition<TrainerModel> filter, UpdateDefinition<TrainerModel> update)
        {
            return MongoCollectionHelper
                .Trainer
                .FindOneAndUpdate(filter, update);
        }

        public static TrainerModel UpdateTrainer(string trainerId, UpdateDefinition<TrainerModel> update)
        {
            Expression<Func<TrainerModel, bool>> filter = trainer => trainer.TrainerId == trainerId;
            return MongoCollectionHelper
                .Trainer
                .FindOneAndUpdate(filter, update);
        }
    }
}
