using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TheReplacement.PTA.Common.Enums;
using TheReplacement.PTA.Common.Internal;
using TheReplacement.PTA.Common.Models;

namespace TheReplacement.PTA.Common.Utilities
{
    /// <summary>
    /// Provides a Collection of CRUD methods for the PTA database
    /// </summary>
    public static class DatabaseUtility
    {
        private const MongoCollection Game = MongoCollection.Games;
        private const MongoCollection Npc = MongoCollection.Npcs;
        private const MongoCollection Pokemon = MongoCollection.Pokemon;
        private const MongoCollection Trainer = MongoCollection.Trainers;

        /// <summary>
        /// Searches for a game using its id, then deletes it
        /// </summary>
        /// <param name="id">The game session id</param>
        public static bool DeleteGame(string id)
        {
            var result = MongoCollectionHelper
                .Games
                .FindOneAndDelete(game => game.GameId == id) != null;

            if (result)
            {
                LoggerUtility.Info(Game, $"Deleted game session {id}");
            }

            return result;
        }

        /// <summary>
        /// Searches for an npc using its id, then deletes it
        /// </summary>
        /// <param name="id">The npc id</param>
        public static bool DeleteNpc(string id)
        {
            var result = MongoCollectionHelper
                .Npcs
                .FindOneAndDelete(npc => npc.NPCId == id) != null;

            if (result)
            {
                LoggerUtility.Info(Game, $"Deleted npc {id}");
            }

            return result;
        }

        /// <summary>
        /// Searches for a Pokemon using its id, then deletes it
        /// </summary>
        /// <param name="id">The Pokemon id</param>
        public static bool DeletePokemon(string id)
        {
            var result = MongoCollectionHelper
                .Pokemon
                .FindOneAndDelete(pokemon => pokemon.PokemonId == id) != null;

            if (result)
            {
                LoggerUtility.Info(Pokemon, $"Deleted pokemon {id}");
            }

            return result;
        }

        /// <summary>
        /// Searches for all Pokemon using their trainer id, then deletes it
        /// </summary>
        /// <param name="trainerId">The trainer id</param>
        public static long DeletePokemonByTrainerId(string trainerId)
        {
            var deleteResult = MongoCollectionHelper
                .Pokemon
                .DeleteMany(pokemon => pokemon.TrainerId == trainerId);
            
            if (deleteResult.IsAcknowledged)
            {
                LoggerUtility.Info(Pokemon, $"Deleted {deleteResult.DeletedCount} pokemon associated with trainer {trainerId}");
                return deleteResult.DeletedCount;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Searches for a trainer using their id, then deletes it
        /// </summary>
        /// <param name="id">The trainer id</param>
        public static bool DeleteTrainer(string id)
        {
            var result = MongoCollectionHelper
                .Trainers
                .FindOneAndDelete(trainer => trainer.TrainerId == id) != null;

            if (result)
            {
                LoggerUtility.Info(Trainer, $"Deleted trainer {id}");
            }

            return result;
        }

        /// <summary>
        /// Searches for all trainers using their game id, then deletes it
        /// </summary>
        /// <param name="gameId">The game id</param>
        public static long DeleteTrainersByGameId(string gameId)
        {
            var deleteResult =  MongoCollectionHelper
                .Trainers
                .DeleteMany(trainer => trainer.GameId == gameId);

            if (deleteResult.IsAcknowledged)
            {
                LoggerUtility.Info(Trainer, $"Deleted {deleteResult.DeletedCount} trainer associated with game {gameId}");
                return deleteResult.DeletedCount;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Returns all games in db
        /// </summary>
        public static IEnumerable<MinifiedGameModel> FindAllGames()
        {
            var games = MongoCollectionHelper.Games
                .Find(game => true)
                .ToEnumerable()
                .Select(game => new MinifiedGameModel(game));

            if (games.Count() > 20)
            {
                return games.TakeLast(20);
            }

            return games;
        }

        public static IEnumerable<GameModel> FindAllGames(string nickname)
        {
            return MongoCollectionHelper.Games
                .Find(game => game.Nickname.ToLower() == nickname.ToLower())
                .ToEnumerable();
        }

        /// <summary>
        /// Returns a game matching the game session id
        /// </summary>
        /// <param name="id">The game session id</param>
        public static GameModel FindGame(string id)
        {
            var game =  MongoCollectionHelper
                .Games
                .Find(game => game.GameId == id)
                .SingleOrDefault();

            if (game != null)
            {
                LoggerUtility.Info(Game, $"Retrieved game session {id}");
            }

            return game;
        }


        /// <summary>
        /// Returns an npc matching the npc id
        /// </summary>
        /// <param name="id">The npc id</param>
        public static NpcModel FindNpc(string id)
        {
            var npc = MongoCollectionHelper
                .Npcs
                .Find(npc => npc.NPCId == id)
                .SingleOrDefault();

            if (npc != null)
            {
                LoggerUtility.Info(Npc, $"Retrieved npc {id}");
            }

            return npc;
        }


        /// <summary>
        /// Returns all npcs matching the npc ids
        /// </summary>
        /// <param name="npcIds">The npc ids</param>
        public static IEnumerable<NpcModel> FindNpcs(IEnumerable<string> npcIds)
        {
            var npcs = npcIds == null
                ? throw new ArgumentNullException(nameof(npcIds))
                : MongoCollectionHelper
                .Npcs
                .Find(npc => npcIds.Contains(npc.NPCId));

            return npcs.ToEnumerable();
        }

        /// <summary>
        /// Returns a Pokemon matching the Pokemon id
        /// </summary>
        /// <param name="id">The Pokemon id</param>
        public static PokemonModel FindPokemonById(string id)
        {
            var pokemon = MongoCollectionHelper
                .Pokemon
                .Find(Pokemon => Pokemon.PokemonId == id)
                .SingleOrDefault(); ;

            if (pokemon != null)
            {
                LoggerUtility.Info(Pokemon, $"Retrieved pokemon {id}");
            }

            return pokemon;
        }

        /// <summary>
        /// Returns all Pokemon matching the trainer id
        /// </summary>
        /// <param name="trainerId">The trainer id</param>
        public static IEnumerable<PokemonModel> FindPokemonByTrainerId(string trainerId)
        {
            var pokemon = MongoCollectionHelper
                .Pokemon
                .Find(Pokemon => Pokemon.TrainerId == trainerId);

            if (pokemon.Any())
            {
                LoggerUtility.Info(Pokemon, $"Retrieved {pokemon.CountDocuments()} pokemon for trainer {trainerId}");
            }

            return pokemon.ToEnumerable();
        }

        /// <summary>
        /// Returns a trainer matching the trainer id
        /// </summary>
        /// <param name="id">The trainer id</param>
        public static TrainerModel FindTrainerById(string id)
        {
            var trainer = MongoCollectionHelper
                .Trainers
                .Find(trainer => trainer.TrainerId == id)
                .SingleOrDefault(); ;

            if (trainer != null)
            {
                LoggerUtility.Info(Trainer, $"Retrieved trainer {id}");
            }

            return trainer;
        }

        /// <summary>
        /// Returns all trainers matching the game session id
        /// </summary>
        /// <param name="gameId">The game session id</param>
        public static IEnumerable<TrainerModel> FindTrainersByGameId(string gameId)
        {
            var trainers = MongoCollectionHelper
                .Trainers
                .Find(trainer => trainer.GameId == gameId);

            if (trainers.Any())
            {
                LoggerUtility.Info(Trainer, $"Retrieved {trainers.CountDocuments()} trainers for game {gameId}");
            }

            return trainers.ToEnumerable();
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
            var trainer = MongoCollectionHelper
                .Trainers
                .Find(filter)
                .SingleOrDefault();

            if (trainer != null)
            {
                LoggerUtility.Info(Trainer, $"Retrieved trainer {trainer.TrainerId} in game {gameId}");
            }

            return trainer;
        }

        /// <summary>
        /// Returns whether there is a game master for the provide game session
        /// </summary>
        /// <param name="gameId">The game session id</param>
        /// <param name="error">The error</param>
        public static bool HasGM(
            string gameId,
            out object error)
        {
            error = new
            {
                message = "No GM has been made",
                gameId
            };

            return FindGame(gameId) != null && MongoCollectionHelper
                .Trainers
                .Find(trainer => trainer.IsGM && trainer.GameId == gameId)
                .Any();
        }

        /// <summary>
        /// Attempts to add a game using the provided document
        /// </summary>
        /// <param name="game">The document to add</param>
        /// <param name="error">Any error found</param>
        public static bool TryAddGame(
            GameModel game,
            out MongoWriteError error)
        {
            return TryAddDocument
            (
                Game,
                game.GameId,
                () => MongoCollectionHelper.Games.InsertOne(game),
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
            out MongoWriteError error)
        {
            return TryAddDocument
            (
                Npc,
                npc.NPCId,
                () => MongoCollectionHelper.Npcs.InsertOne(npc),
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
            out MongoWriteError error)
        {
            return TryAddDocument
            (
                Pokemon,
                pokemon.PokemonId,
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
            out MongoWriteError error)
        {
            return TryAddDocument
            (
                Trainer,
                trainer.TrainerId,
                () => MongoCollectionHelper.Trainers.InsertOne(trainer),
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
            return TryUpdateDocument
            (
                Game,
                MongoCollectionHelper.Games,
                game => game.GameId == gameId,
                Builders<GameModel>.Update.Set("NPCs", npcIds),
                $"Updated npc list for game {gameId}"
            );
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
            return TryUpdateDocument
            (
                Game,
                MongoCollectionHelper.Games,
                game => game.GameId == gameId,
                Builders<GameModel>.Update.Set("IsOnline", isOnline),
                $"Updated online status for game {gameId}"
            );
        }

        /// <summary>
        /// Searches for a pokemon, then updates its trainer id
        /// </summary>
        /// <param name="pokemonId">The pokemon id</param>
        /// <param name="trainerId">The trainer id</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="MongoCommandException" />
        public static bool UpdatePokemonTrainerId(
            string pokemonId,
            string trainerId)
        {
            return TryUpdateDocument
            (
                Pokemon,
                MongoCollectionHelper.Pokemon,
                pokemon => pokemon.PokemonId == pokemonId,
                Builders<PokemonModel>.Update.Set("TrainerId", trainerId),
                $"Updated trainerId for pokemon {pokemonId}"
            );
        }

        /// <summary>
        /// Searches for a pokemon, then evolves it to its next stage
        /// </summary>
        /// <param name="pokemonId">The pokemon id</param>
        /// <param name="evolvedForm">The pokemon's evolution</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="MongoCommandException" />
        public static bool UpdatePokemonWithEvolution(
            string pokemonId,
            PokemonModel evolvedForm)
        {
            if (evolvedForm == null)
            {
                throw new ArgumentNullException(nameof(evolvedForm));
            }

            return TryUpdateDocument
            (
                Pokemon,
                MongoCollectionHelper.Pokemon,
                pokemon => pokemon.PokemonId == pokemonId,
                GetEvolvedUpdates(evolvedForm),
                $"Evolved pokemon {pokemonId}"
            );
        }

        /// <summary>
        /// Searches for a trainer, then updates their activity token
        /// </summary>
        /// <param name="trainerId">The trainer id</param>
        /// <param name="token">The new activity token</param>
        public static bool UpdateTrainerActivityToken(
            string trainerId,
            string token)
        {
            return TryUpdateDocument
            (
                Trainer,
                MongoCollectionHelper.Trainers,
                trainer => trainer.TrainerId == trainerId,
                Builders<TrainerModel>.Update.Set("ActivityToken", token),
                $"Granted trainer {trainerId} a new activity token"
            );
        }

        /// <summary>
        /// Searches for a trainer, then updates their item list
        /// </summary>
        /// <param name="trainerId">The trainer id</param>
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

            return TryUpdateDocument
            (
                Trainer,
                MongoCollectionHelper.Trainers,
                trainer => trainer.TrainerId == trainerId,
                Builders<TrainerModel>.Update.Set("Items", itemList),
                $"Updated item list for trainer {trainerId}"
            );
        }

        /// <summary>
        /// Searches for a trainer, then updates their online status
        /// </summary>
        /// <param name="trainerId">The trainer id</param>
        /// <param name="isOnline">The updated online status</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="MongoCommandException" />
        public static bool UpdateTrainerOnlineStatus(
            string trainerId,
            bool isOnline)
        {
            return TryUpdateDocument
            (
                Trainer,
                MongoCollectionHelper.Trainers,
                trainer => trainer.TrainerId == trainerId,
                TrainerStatusUpdate(isOnline),
                $"Updated online status for trainer {trainerId}"
            );
        }

        /// <summary>
        /// Searches for a trainer, then updates their password
        /// </summary>
        /// <param name="trainerId">The trainer id</param>
        /// <param name="password">The updated password</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="MongoCommandException" />
        public static bool UpdateTrainerPassword(
            string trainerId,
            string password)
        {
            return TryUpdateDocument
            (
                Trainer,
                MongoCollectionHelper.Trainers,
                trainer => trainer.TrainerId == trainerId,
                GetTrainerPasswordUpdate(password),
                $"Updated password for trainer {trainerId}"
            );
        }

        private static UpdateDefinition<TrainerModel> GetTrainerPasswordUpdate(string password)
        {
            return Builders<TrainerModel>
                .Update
                .Combine(new[]
                {
                    Builders<TrainerModel>.Update.Set("PasswordHash", EncryptionUtility.HashSecret(password)),
                    Builders<TrainerModel>.Update.Set("IsOnline", true)
                });
        }

        private static UpdateDefinition<PokemonModel> GetEvolvedUpdates(PokemonModel evolvedForm)
        {
            return Builders<PokemonModel>.Update.Combine(new[]
            {
                Builders<PokemonModel>.Update.Set("Nickname", evolvedForm.Nickname),
                Builders<PokemonModel>.Update.Set("DexNo", evolvedForm.DexNo),
                Builders<PokemonModel>.Update.Set("PokemonStats", evolvedForm.PokemonStats),
                Builders<PokemonModel>.Update.Set("Size", evolvedForm.Size),
                Builders<PokemonModel>.Update.Set("Weight", evolvedForm.Weight),
                Builders<PokemonModel>.Update.Set("Skills", evolvedForm.Skills),
                Builders<PokemonModel>.Update.Set("Passives", evolvedForm.Passives),
                Builders<PokemonModel>.Update.Set("Proficiencies", evolvedForm.Proficiencies),
                Builders<PokemonModel>.Update.Set("Habitats", evolvedForm.Habitats),
                Builders<PokemonModel>.Update.Set("Diet", evolvedForm.Diet),
                Builders<PokemonModel>.Update.Set("Rarity", evolvedForm.Rarity),
                Builders<PokemonModel>.Update.Set("GMaxMove", evolvedForm.GMaxMove),
                Builders<PokemonModel>.Update.Set("EvolvedFrom", evolvedForm.EvolvedFrom),
                Builders<PokemonModel>.Update.Set("LegendaryStats", evolvedForm.LegendaryStats)
            });
        }

        private static bool TryAddDocument(
            MongoCollection dbCollection,
            string id,
            Action action,
            out MongoWriteError error)
        {
            try
            {
                action();
                error = null;
                LoggerUtility.Info(dbCollection, $"Added {dbCollection} {id}");
                return true;
            }
            catch (MongoWriteException exception)
            {
                error = new MongoWriteError(exception.WriteError.Details.GetValue("details").AsBsonDocument.ToString());
                LoggerUtility.Error(dbCollection, error.WriteErrorJsonString);
                return false;
            }
        }

        private static bool TryUpdateDocument<TMongoCollection>(
            MongoCollection dbCollection,
            IMongoCollection<TMongoCollection> collection,
            Expression<Func<TMongoCollection, bool>> filter,
            UpdateDefinition<TMongoCollection> update,
            string successMessage)
        {
            try
            {
                if (collection.FindOneAndUpdate(filter, update) == null)
                {
                    return false;
                }

                LoggerUtility.Info(dbCollection, successMessage);
                return true;
            }
            catch (MongoCommandException ex)
            {
                LoggerUtility.Error(dbCollection, ex.Message);
                throw ex;
            }
        }

        private static UpdateDefinition<TrainerModel> TrainerStatusUpdate(bool isOnline)
        {
            if (isOnline)
            {
                return Builders<TrainerModel>.Update.Set("IsOnline", isOnline);
            }

            return Builders<TrainerModel>.Update.Combine
            (
                Builders<TrainerModel>.Update.Set("IsOnline", isOnline),
                Builders<TrainerModel>.Update.Set("ActivityToken", string.Empty)
            );
        }
    }
}
