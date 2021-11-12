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
        private static TableHelper _tableHelper = new TableHelper(27017, "localhost");
        public static TableHelper TableHelper => _tableHelper;

        public static void AddGame(GameModel game)
        {
            _tableHelper
                .Game
                .InsertOne(game);
        }

        public static void AddPokemon(PokemonModel pokemon)
        {
            _tableHelper
                .Pokemon
                .InsertOne(pokemon);
        }

        public static void AddTrainer(TrainerModel trainer)
        {
            _tableHelper.Trainer.InsertOne(trainer);
        }

        public static bool DeleteGame(string id)
        {
            return _tableHelper
                .Game
                .DeleteOne(game => game.GameId == id).IsAcknowledged;
        }

        public static bool DeletePokemon(string id)
        {
            return _tableHelper
                .Pokemon
                .DeleteOne(pokemon => pokemon.PokemonId == id).IsAcknowledged;
        }

        public static object DeletePokemon(TrainerModel trainer)
        {
            Expression<Func<PokemonModel, bool>> pokemonFiler = pokemon => pokemon.TrainerId == trainer.TrainerId;
            string message;
            if (_tableHelper.Pokemon.DeleteMany(pokemonFiler).IsAcknowledged)
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
            return _tableHelper
                .Trainer
                .DeleteMany(filter).IsAcknowledged;
        }

        public static object DeleteTrainerMons(string trainerId)
        {
            var deleteResult = _tableHelper
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
            return _tableHelper
                .Game
                .Find(game => game.GameId == id)
                .FirstOrDefault();
        } 

        public static PokemonModel FindPokemon(Expression<Func<PokemonModel, bool>> filter)
        {
            return _tableHelper
                .Pokemon
                .Find(filter)
                .FirstOrDefault();
        }
        
        public static PokemonModel FindPokemonById(string id)
        {
            return _tableHelper
                .Pokemon
                .Find(pokemon => pokemon.PokemonId == id)
                .FirstOrDefault();
        }
        
        public static TrainerModel FindTrainer(Expression<Func<TrainerModel, bool>> filter)
        {
            return _tableHelper
                .Trainer
                .Find(filter)
                .FirstOrDefault();
        }

        public static IEnumerable<TrainerModel> FindTrainers(FilterDefinition<TrainerModel> filter)
        {
            return _tableHelper
                .Trainer
                .Find(filter)
                .ToList();
        }

        public static TrainerModel FindTrainerById(string id)
        {
            return _tableHelper
                .Trainer
                .Find(trainer => trainer.TrainerId == id)
                .FirstOrDefault();
        }

        public static TrainerModel FindTrainerByUsername(string username)
        {
            return _tableHelper
                .Trainer
                .Find(trainer => trainer.Username == username)
                .FirstOrDefault();
        }

        public static TrainerModel FindTrainerByUsername(
            string username,
            string gameId)
        {
            Expression<Func<TrainerModel, bool>> filter = trainer => trainer.Username == username && trainer.GameId == gameId;
            return _tableHelper
                .Trainer
                .Find(filter)
                .FirstOrDefault();
        }

        public static bool HasGM(string gameId)
        {
            return _tableHelper
                .Trainer
                .Find(trainer => trainer.IsGM && trainer.GameId == gameId)
                .Any();
        }

        public static bool UpdatePokemon(FilterDefinition<PokemonModel> filter, UpdateDefinition<PokemonModel> update)
        {
            return _tableHelper
                .Pokemon
                .UpdateOne(filter, update)
                .IsAcknowledged;
        }

        public static TrainerModel UpdateTrainer(FilterDefinition<TrainerModel> filter, UpdateDefinition<TrainerModel> update)
        {
            return _tableHelper
                .Trainer
                .FindOneAndUpdate(filter, update);
        }

        public static TrainerModel UpdateTrainer(string trainerId, UpdateDefinition<TrainerModel> update)
        {
            Expression<Func<TrainerModel, bool>> filter = trainer => trainer.TrainerId == trainerId;
            return _tableHelper
                .Trainer
                .FindOneAndUpdate(filter, update);
        }

        public static GameModel UpdateGame(string gameId, UpdateDefinition<GameModel> update)
        {
            Expression<Func<GameModel, bool>> filter = game => game.GameId == gameId;
            return _tableHelper
                .Game
                .FindOneAndUpdate(filter, update);
        }
    }
}
