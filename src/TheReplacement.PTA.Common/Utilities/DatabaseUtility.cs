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
        private const MongoCollection Encounter = MongoCollection.Encounters;

        /// <summary>
        /// Searches for a encounter using its id, then deletes it
        /// </summary>
        /// <param name="id">The encounter id</param>
        public static bool DeleteEncounter(string id)
        {
            var result = MongoCollectionHelper
                .Encounter
                .FindOneAndDelete(encounter => encounter.EncounterId == id) != null;

            if (result)
            {
                LoggerUtility.Info(Encounter, $"Deleted encounter session {id}");
            }

            return result;
        }

        /// <summary>
        /// Searches for encounter using their game id, then deletes them
        /// </summary>
        /// <param name="gameId">The game session id</param>
        public static bool DeleteEncountersByGameId(string gameId)
        {
            var result = MongoCollectionHelper
                .Encounter
                .DeleteMany(encounter => encounter.EncounterId == gameId)?.IsAcknowledged == true;

            if (result)
            {
                LoggerUtility.Info(Encounter, $"Deleted all encounters associated with game session {gameId}");
            }

            return result;
        }

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
        /// Searches for all Npcs using their game id, then deletes it
        /// </summary>
        /// <param name="gameId">The game id</param>
        public static bool DeleteNpcByGameId(string gameId)
        {
            var deleteResult = MongoCollectionHelper
                .Npcs
                .DeleteMany(npc => npc.GameId == gameId);

           
                return deleteResult.IsAcknowledged;
            
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
                MongoCollectionHelper.PokeDex.DeleteMany(pokeDex => pokeDex.TrainerId == trainerId);
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
        /// Returns an active encounter (if any) matching the game session id
        /// </summary>
        /// <param name="gameId">The game id</param>
        public static EncounterModel FindActiveEncounter(string gameId)
        {
            return MongoCollectionHelper.Encounter
                .Find(encounter => encounter.GameId == gameId && encounter.IsActive)
                .SingleOrDefault();
        }

        /// <summary>
        /// Returns an encounter matching the id
        /// </summary>
        /// <param name="encounterId">The encounter id</param>
        public static EncounterModel FindEncounter(string encounterId)
        {
            return MongoCollectionHelper.Encounter
                .Find(encounter => encounter.EncounterId == encounterId)
                .SingleOrDefault();
        }

        /// <summary>
        /// Returns all encounters associated with the game session
        /// </summary>
        /// <param name="gameId">The game id</param>
        public static IEnumerable<EncounterModel> FindAllEncounters(string gameId)
        {
            return MongoCollectionHelper.Encounter
                .Find(encounter => encounter.GameId == gameId)
                .ToEnumerable();
        }

        /// <summary>
        /// Returns all games in db
        /// </summary>
        public static IEnumerable<MinifiedGameModel> FindMostRecent20Games()
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

        /// <summary>
        /// Returns all games that contains the supplied nickname as a substring
        /// </summary>
        /// <param name="nickname">The nickname to search with</param>
        public static IEnumerable<GameModel> FindAllGames(string nickname)
        {
            return MongoCollectionHelper.Games
                .Find(game => game.Nickname.ToLower().Contains(nickname.ToLower()))
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
        /// Attempts to update the trainer with their appropriate starting stats
        /// </summary>
        /// <param name="trainerId">The id of the trainer being updated</param>
        /// <param name="origin">The trianer's origin</param>
        /// <param name="trainerClass">The trainer's stats class</param>
        /// <param name="feats">The trainer's starting feats</param>
        /// <param name="stats">The trainer's starting stats</param>
        /// <returns>True if successful</returns>
        public static bool CompleteTrainer(
            string trainerId,
            string origin,
            string trainerClass,
            IEnumerable<string> feats,
            StatsModel stats)
        {
            var updates = Builders<TrainerModel>.Update.Combine(new[]
            {
                Builders<TrainerModel>.Update.Set("Origin", origin),
                Builders<TrainerModel>.Update.Set("TrainerClasses", new[] { trainerClass }),
                Builders<TrainerModel>.Update.Set("Feats", feats),
                Builders<TrainerModel>.Update.Set("TrainerStats", stats),
                Builders<TrainerModel>.Update.Set("IsComplete", true)
            });

            return TryUpdateDocument
            (
                Trainer,
                MongoCollectionHelper.Trainers,
                trainer => trainer.TrainerId == trainerId,
                updates
            );
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
        /// Returns all npcs matching the game id
        /// </summary>
        /// <param name="gameId">The npc ids</param>
        public static IEnumerable<NpcModel> FindNpcsByGameId(string gameId)
        {
            return MongoCollectionHelper.Npcs.Find(npc => npc.GameId == gameId).ToEnumerable();
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
                .Find(pokemon => pokemon.TrainerId == trainerId);

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
            return FindTrainerById(trainer => trainer.TrainerId == id);
        }

        /// <summary>
        /// Search for the trainer and returns them if the trainer has not completed the new user flow
        /// </summary>
        /// <param name="id">The id of the trainer to search for</param>
        public static TrainerModel FindIncompleteTrainerById(string id)
        {
            return FindTrainerById(trainer => trainer.TrainerId == id && !trainer.IsComplete);
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
            Expression<Func<TrainerModel, bool>> filter = trainer => trainer.TrainerName.ToLower() == username.ToLower() && trainer.GameId == gameId;
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
        /// Returns all sprites
        /// </summary>
        public static IEnumerable<SpriteModel> GetAllSprites()
        {
            return MongoCollectionHelper.Sprite.Find(sprite => true).ToEnumerable();
        }

        /// <summary>
        /// Returns a game's nickname using the game id
        /// </summary>
        /// <param name="gameId">The game session id</param>
        public static string GetGameNickname(string gameId)
        {
            return FindGame(gameId)?.Nickname;
        }

        /// <summary>
        /// Compiles all pokedex entries for a specific trainer into one collection
        /// </summary>
        /// <param name="trainerId">The trainer's id to search with</param>
        public static IEnumerable<PokeDexItemModel> GetTrainerPokeDex(string trainerId)
        {
            return MongoCollectionHelper.PokeDex
                .Find(dexItem => dexItem.TrainerId == trainerId)
                .ToEnumerable();
        }

        /// <summary>
        /// Searches the database for a pokedex entry
        /// </summary>
        /// <param name="trainerId">The trainer's id to search with</param>
        /// <param name="dexNo">The dex number for the pokemon</param>
        public static PokeDexItemModel GetPokedexItem(string trainerId, int dexNo)
        {
            return MongoCollectionHelper.PokeDex
                .Find(dexItem => dexItem.TrainerId == trainerId && dexItem.DexNo == dexNo)
                .SingleOrDefault();
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
        /// Attempts to replace the previous encounter with the new data
        /// </summary>
        /// <param name="updatedEncounter">The updated encounter data</param>
        public static bool UpdateEncounter(EncounterModel updatedEncounter)
        {
            var result = MongoCollectionHelper.Encounter.ReplaceOne
            (
                encounter => encounter.EncounterId == updatedEncounter.EncounterId,
                options: new ReplaceOptions { IsUpsert = true },
                replacement: updatedEncounter
            );

            return result.IsAcknowledged;
        }

        /// <summary>
        /// Attempts to replace the previous Npc with the new data
        /// </summary>
        /// <param name="updatedNpc">The updated npc data</param>
        public static bool UpdateNpc(NpcModel updatedNpc)
        {
            var result = MongoCollectionHelper.Npcs.ReplaceOne
            (
                npc => npc.NPCId == updatedNpc.NPCId,
                options: new ReplaceOptions { IsUpsert = true },
                replacement: updatedNpc
            );

            return result.IsAcknowledged;
        }

        /// <summary>
        /// Attempts to replace the previous trainer with the new data
        /// </summary>
        /// <param name="updatedTrainer">The updated trainer data</param>
        public static bool UpdateTrainer(TrainerModel updatedTrainer)
        {
            var result = MongoCollectionHelper.Trainers.ReplaceOne
            (
                trainer => trainer.TrainerId == updatedTrainer.TrainerId,
                options: new ReplaceOptions { IsUpsert = true },
                replacement: updatedTrainer
            );

            return result.IsAcknowledged;
        }

        /// <summary>
        /// Attempts to add a encounter using the provided document
        /// </summary>
        /// <param name="encounter">The document to add</param>
        public static (bool Result, MongoWriteError Error) TryAddEncounter(EncounterModel encounter)
        {
            return (TryAddDocument
            (
                Encounter,
                () => MongoCollectionHelper.Encounter.InsertOne(encounter),
                out var error
            ), error);
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
                () => MongoCollectionHelper.Npcs.InsertOne(npc),
                out error
            );
        }

        /// <summary>
        /// Attempts to update a pokemon's form
        /// </summary>
        /// <param name="pokemon">The document to add</param>
        /// <param name="error">Any error found</param>
        public static bool TryChangePokemonForm(
            PokemonModel pokemon,
            out MongoWriteError error)
        {
            if (DeletePokemon(pokemon.PokemonId))
            {
                return TryAddPokemon(pokemon, out error);
            }

            throw new Exception();
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
                () => MongoCollectionHelper.Pokemon.InsertOne(pokemon),
                out error
            );
        }

        /// <summary>
        /// Attempts to add a sprite using the provided document
        /// </summary>
        /// <param name="sprite">The document to add</param>
        /// <param name="error">Any error found</param>
        public static bool TryAddSprite(
            SpriteModel sprite,
            out MongoWriteError error)
        {
            try
            {
                MongoCollectionHelper.Sprite.InsertOne(sprite);
                error = null;
                return true;
            }
            catch (MongoWriteException exception)
            {
                error = new MongoWriteError(exception.WriteError.Details.GetValue("details").AsBsonDocument.ToString());
                return false;
            }
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
                () => MongoCollectionHelper.Trainers.InsertOne(trainer),
                out error
            );
        }

        /// <summary>
        /// Attempts to add a dexItem using the provided document
        /// </summary>
        /// <param name="trainerId">The pokedex's trainer id</param>
        /// <param name="dexNo">The dex number</param>
        /// <param name="isSeen">Whether the pokemon was seen</param>
        /// <param name="isCaught">Whether the pokemon was caught</param>
        /// <param name="error">Any error found</param>
        public static bool TryAddDexItem(
            string trainerId,
            int dexNo,
            bool isSeen,
            bool isCaught,
            out MongoWriteError error)
        {
            var dexItem = new PokeDexItemModel
            {
                TrainerId = trainerId,
                DexNo = dexNo,
                IsSeen = isSeen,
                IsCaught = isCaught
            };

            return TryAddDocument
            (
                MongoCollection.PokeDex,
                () => MongoCollectionHelper.PokeDex.InsertOne(dexItem),
                out error
            );
        }

        /// <summary>
        /// Updates the pokedex entry for a seen pokemon
        /// </summary>
        /// <param name="trainerId">The trainer's id to search with</param>
        /// <param name="dexNo">The dex number for the pokemon</param>
        public static bool UpdateDexItemIsSeen(string trainerId, int dexNo)
        {
            return TryUpdateDocument
            (
                MongoCollection.PokeDex,
                MongoCollectionHelper.PokeDex,
                dexItem => dexItem.TrainerId == trainerId && dexItem.DexNo == dexNo,
                Builders<PokeDexItemModel>.Update.Set("IsSeen", true)
            );
        }

        /// <summary>
        /// Updates the pokedex entry for a caught pokemon
        /// </summary>
        /// <param name="trainerId">The trainer's id to search with</param>
        /// <param name="dexNo">The dex number for the pokemon</param>
        public static bool UpdateDexItemIsCaught(string trainerId, int dexNo)
        {
            var updates = Builders<PokeDexItemModel>
                .Update
                .Combine(new[]
                {
                    Builders<PokeDexItemModel>.Update.Set("IsSeen", true),
                    Builders<PokeDexItemModel>.Update.Set("IsCaught", true)
                });

            return TryUpdateDocument
            (
                MongoCollection.PokeDex,
                MongoCollectionHelper.PokeDex,
                dexItem => dexItem.TrainerId == trainerId && dexItem.DexNo == dexNo,
                updates
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
                Builders<GameModel>.Update.Set("NPCs", npcIds)
            );
        }

        /// <summary>
        /// Searches for a game, then updates the logs
        /// </summary>
        /// <param name="theGame">The game session</param>
        /// <param name="logs">The new logs to add</param>
        /// <exception cref="MongoCommandException" />
        public static bool UpdateGameLogs(GameModel theGame, params LogModel[] logs)
        {
            return TryUpdateDocument
            (
                Game,
                MongoCollectionHelper.Games,
                game => game.GameId == theGame.GameId,
                Builders<GameModel>.Update.Set("Logs", theGame.Logs?.Union(logs) ?? logs)
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
                Builders<GameModel>.Update.Set("IsOnline", isOnline)
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
                Builders<PokemonModel>.Update.Set("TrainerId", trainerId)
            );
        }
        /// <summary>
        /// Searches for a pokemon, then updates its evolvability
        /// </summary>
        /// <param name="pokemonId">The pokemon id</param>
        /// <param name="isEvolvable">Whether the pokemon is evolvable</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="MongoCommandException" />
        public static bool UpdatePokemonEvolvability(
            string pokemonId,
            bool isEvolvable)
        {
            return TryUpdateDocument
            (
                Pokemon,
                MongoCollectionHelper.Pokemon,
                pokemon => pokemon.PokemonId == pokemonId,
                Builders<PokemonModel>.Update.Set("CanEvolve", isEvolvable)
            );
        }

        /// <summary>
        /// Attempts to update a pokemon's hp
        /// </summary>
        /// <param name="pokemonId">The pokemon's id</param>
        /// <param name="hp">The pokemon's new hp</param>
        public static bool UpdatePokemonHP(string pokemonId, int hp)
        {
            return TryUpdateDocument
            (
                Pokemon,
                MongoCollectionHelper.Pokemon,
                pokemon => pokemon.PokemonId == pokemonId,
                Builders<PokemonModel>.Update.Set("CurrentHP", hp)
            );
        }

        /// <summary>
        /// Searches for a pokemon, then updates its location
        /// </summary>
        /// <param name="pokemonId">The pokemon id</param>
        /// <param name="isOnActiveTeam">Whether the pokemon is on the active team</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="MongoCommandException" />
        public static bool UpdatePokemonLocation(
            string pokemonId,
            bool isOnActiveTeam)
        {
            return TryUpdateDocument
            (
                Pokemon,
                MongoCollectionHelper.Pokemon,
                pokemon => pokemon.PokemonId == pokemonId,
                Builders<PokemonModel>.Update.Set("IsOnActiveTeam", isOnActiveTeam)
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

            if (DeletePokemon(pokemonId))
            {
                return TryAddPokemon(evolvedForm, out _);
            }

            throw new Exception();
        }

        /// <summary>
        /// Searches for a trainer, then updates their honors
        /// </summary>
        /// <param name="trainerId">The trainer id</param>
        /// <param name="honors">The trainer's honors</param>
        public static bool UpdateTrainerHonors(
            string trainerId,
            IEnumerable<string> honors)
        {
            return TryUpdateDocument
            (
                Trainer,
                MongoCollectionHelper.Trainers,
                trainer => trainer.TrainerId == trainerId,
                Builders<TrainerModel>.Update.Set("Honors", honors)
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
                Builders<TrainerModel>.Update.Set("ActivityToken", token)
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
                Builders<TrainerModel>.Update.Set("Items", itemList)
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
                TrainerStatusUpdate(isOnline)
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
                GetTrainerPasswordUpdate(password)
            );
        }

        private static TrainerModel FindTrainerById(
            Expression<Func<TrainerModel, bool>> searchPattern)
        {
            var trainer = MongoCollectionHelper
                .Trainers
                .Find(searchPattern)
                .SingleOrDefault();

            return trainer;
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

        private static bool TryAddDocument(
            MongoCollection dbCollection,
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
                LoggerUtility.Error(dbCollection, error.WriteErrorJsonString);
                return false;
            }
        }

        private static bool TryUpdateDocument<TMongoCollection>(
            MongoCollection dbCollection,
            IMongoCollection<TMongoCollection> collection,
            Expression<Func<TMongoCollection, bool>> filter,
            UpdateDefinition<TMongoCollection> update)
        {
            try
            {
                if (collection.FindOneAndUpdate(filter, update) == null)
                {
                    return false;
                }

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
