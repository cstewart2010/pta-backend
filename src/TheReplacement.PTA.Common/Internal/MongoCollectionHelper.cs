using MongoDB.Driver;
using System;
using TheReplacement.PTA.Common.Models;

namespace TheReplacement.PTA.Common.Internal
{
    internal static class MongoCollectionHelper
    {
        static MongoCollectionHelper()
        {
            var settings = GetMongoClientSettings();
            var client = new MongoClient(settings);
            var databaseName = Environment.GetEnvironmentVariable("Database", EnvironmentVariableTarget.Process);
            var database = client.GetDatabase(databaseName);
            Games = database.GetCollection<GameModel>("Games");
            Pokemon = database.GetCollection<PokemonModel>("Pokemon");
            Trainers = database.GetCollection<TrainerModel>("Trainers");
            Npcs = database.GetCollection<NpcModel>("NPCs");
            Logs = database.GetCollection<LoggerModel>("Logs");
            BasePokemon = database.GetCollection<BasePokemon>("BasePokemon");
        }

        /// <summary>
        /// Represents the Npc Collection
        /// </summary>
        public static IMongoCollection<BasePokemon> BasePokemon { get; }

        /// <summary>
        /// Represents the Game Collection
        /// </summary>
        public static IMongoCollection<GameModel> Games { get; }

        /// <summary>
        /// Represents the Pokemon Collection
        /// </summary>
        public static IMongoCollection<PokemonModel> Pokemon { get; }

        /// <summary>
        /// Represents the Trainer Collection
        /// </summary>
        public static IMongoCollection<TrainerModel> Trainers { get; }

        /// <summary>
        /// Represents the Npc Collection
        /// </summary>
        public static IMongoCollection<NpcModel> Npcs { get; }

        /// <summary>
        /// Represents the Logs Collection
        /// </summary>
        public static IMongoCollection<LoggerModel> Logs { get; }

        private static MongoClientSettings GetMongoClientSettings()
        {
            var connectionString = Environment.GetEnvironmentVariable("MongoDBConnectionString");
            if (connectionString == null)
            {
                throw new NullReferenceException("MongoDBConnectionString environment variable need to be set to access MongoDB");
            }

            var settings = MongoClientSettings.FromConnectionString(connectionString);
            settings.SslSettings = new SslSettings()
            {
                EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12
            };

            return settings;
        }
    }
}
