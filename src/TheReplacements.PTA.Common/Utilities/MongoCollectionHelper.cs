using MongoDB.Driver;
using System;
using TheReplacements.PTA.Common.Models;

namespace TheReplacements.PTA.Common.Utilities
{
    internal static class MongoCollectionHelper
    {
        static MongoCollectionHelper()
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

            var client = new MongoClient(settings);
            var database = client.GetDatabase("PTA");
            Game = database.GetCollection<GameModel>("Game");
            Pokemon = database.GetCollection<PokemonModel>("Pokemon");
            Trainer = database.GetCollection<TrainerModel>("Trainer");
            Npc = database.GetCollection<NpcModel>("NPC");
            Logs = database.GetCollection<LoggerModel>("Logs");
        }

        /// <summary>
        /// Represents the Game Collection
        /// </summary>
        public static IMongoCollection<GameModel> Game { get; }

        /// <summary>
        /// Represents the Pokemon Collection
        /// </summary>
        public static IMongoCollection<PokemonModel> Pokemon { get; }

        /// <summary>
        /// Represents the Trainer Collection
        /// </summary>
        public static IMongoCollection<TrainerModel> Trainer { get; }

        /// <summary>
        /// Represents the Npc Collection
        /// </summary>
        public static IMongoCollection<NpcModel> Npc { get; }

        /// <summary>
        /// Represents the Logs Collection
        /// </summary>
        public static IMongoCollection<LoggerModel> Logs { get; }
    }
}
