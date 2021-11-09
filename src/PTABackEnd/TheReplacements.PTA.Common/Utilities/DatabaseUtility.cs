using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TheReplacements.PTA.Common.Models;

namespace TheReplacements.PTA.Common.Utilities
{
    public static class DatabaseUtility
    {
        private static TableHelper _tableHelper = new TableHelper(27017, "localhost");
        public static TableHelper TableHelper => _tableHelper;

        public static void AddGame(string id)
        {
            _tableHelper.Game.InsertOne(new GameModel
            {
                GameId = id
            });
        }

        public static void AddTrainer(TrainerModel trainer)
        {
            _tableHelper.Trainer.InsertOne(trainer);
        }

        public static object DeletePokemon(TrainerModel trainer)
        {
            Expression<Func<PokemonModel, bool>> pokemonFiler = pokemon => pokemon.Trainerid == trainer.TrainerId;
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

        public static GameModel FindGame(string id)
        {
            return _tableHelper
                .Game
                .Find(game => game.GameId == id)
                .FirstOrDefault();
        }        
        
        public static TrainerModel FindTrainer(Expression<Func<TrainerModel, bool>> filter)
        {
            return _tableHelper
                .Trainer
                .Find(filter)
                .FirstOrDefault();
        }

        public static IEnumerable<TrainerModel> FindTrainers(Expression<Func<TrainerModel, bool>> filter)
        {
            return _tableHelper
                .Trainer
                .Find(filter)
                .ToEnumerable();
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
            return _tableHelper.Trainer.Find(trainer => trainer.IsGM && trainer.GameId == gameId).Any();
        }
    }
}
