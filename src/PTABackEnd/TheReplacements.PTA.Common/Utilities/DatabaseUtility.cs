using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TheReplacements.PTA.Common.Models;

namespace TheReplacements.PTA.Common.Utilities
{
    public static class DatabaseUtility
    {
        private static readonly TableHelper TableHelper = new TableHelper(27017, "localhost");

        public static bool TryAddGame(GameModel game, out object error)
        {
            return TryAddDocument
            (
                () => TableHelper.Game.InsertOne(game),
                out error
            );
        }

        public static bool TryAddPokemon(PokemonModel pokemon, out object error)
        {
            return TryAddDocument
            (
                () => TableHelper.Pokemon.InsertOne(pokemon),
                out error
            );
        }

        public static bool TryAddTrainer(TrainerModel trainer, out object error)
        {
            return TryAddDocument
            (
                () => TableHelper.Trainer.InsertOne(trainer),
                out error
            );
        }

        private static bool TryAddDocument(Action action, out object error)
        {
            try
            {
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
            return TableHelper
                .Game
                .FindOneAndDelete(game => game.GameId == id) != null;
        }

        public static bool DeletePokemon(string id)
        {
            return TableHelper
                .Pokemon
                .FindOneAndDelete(pokemon => pokemon.PokemonId == id) != null;
        }

        public static object DeletePokemon(TrainerModel trainer)
        {
            Expression<Func<PokemonModel, bool>> pokemonFiler = pokemon => pokemon.TrainerId == trainer.TrainerId;
            string message;
            if (TableHelper.Pokemon.DeleteMany(pokemonFiler).IsAcknowledged)
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
            return TableHelper
                .Trainer
                .DeleteMany(filter).IsAcknowledged;
        }

        public static object DeleteTrainerMons(string trainerId)
        {
            var deleteResult = TableHelper
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
            return TableHelper
                .Game
                .Find(game => game.GameId == id)
                .FirstOrDefault();
        } 

        public static PokemonModel FindPokemon(Expression<Func<PokemonModel, bool>> filter)
        {
            return TableHelper
                .Pokemon
                .Find(filter)
                .FirstOrDefault();
        }
        
        public static PokemonModel FindPokemonById(string id)
        {
            return TableHelper
                .Pokemon
                .Find(pokemon => pokemon.PokemonId == id)
                .FirstOrDefault();
        }
        
        public static TrainerModel FindTrainer(Expression<Func<TrainerModel, bool>> filter)
        {
            return TableHelper
                .Trainer
                .Find(filter)
                .FirstOrDefault();
        }

        public static IEnumerable<TrainerModel> FindTrainers(FilterDefinition<TrainerModel> filter)
        {
            return TableHelper
                .Trainer
                .Find(filter)
                .ToList();
        }

        public static TrainerModel FindTrainerById(string id)
        {
            return TableHelper
                .Trainer
                .Find(trainer => trainer.TrainerId == id)
                .FirstOrDefault();
        }

        public static TrainerModel FindTrainerByUsername(
            string username,
            string gameId)
        {
            Expression<Func<TrainerModel, bool>> filter = trainer => trainer.TrainerName == username && trainer.GameId == gameId;
            return TableHelper
                .Trainer
                .Find(filter)
                .FirstOrDefault();
        }

        public static bool HasGM(string gameId)
        {
            return TableHelper
                .Trainer
                .Find(trainer => trainer.IsGM && trainer.GameId == gameId)
                .Any();
        }

        public static bool UpdatePokemon(FilterDefinition<PokemonModel> filter, UpdateDefinition<PokemonModel> update)
        {
            return TableHelper
                .Pokemon
                .UpdateOne(filter, update)
                .IsAcknowledged;
        }

        public static TrainerModel UpdateTrainer(FilterDefinition<TrainerModel> filter, UpdateDefinition<TrainerModel> update)
        {
            return TableHelper
                .Trainer
                .FindOneAndUpdate(filter, update);
        }

        public static TrainerModel UpdateTrainer(string trainerId, UpdateDefinition<TrainerModel> update)
        {
            Expression<Func<TrainerModel, bool>> filter = trainer => trainer.TrainerId == trainerId;
            return TableHelper
                .Trainer
                .FindOneAndUpdate(filter, update);
        }

        public static GameModel UpdateGame(string gameId, UpdateDefinition<GameModel> update)
        {
            Expression<Func<GameModel, bool>> filter = game => game.GameId == gameId;
            return TableHelper
                .Game
                .FindOneAndUpdate(filter, update);
        }
    }
}
