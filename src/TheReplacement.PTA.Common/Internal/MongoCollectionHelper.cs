using MongoDB.Driver;
using System;
using TheReplacement.PTA.Common.Enums;
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
            Database = client.GetDatabase(databaseName);
            Games = Database.GetCollection<GameModel>(MongoCollection.Games.ToString());
            Pokemon = Database.GetCollection<PokemonModel>(MongoCollection.Pokemon.ToString());
            Trainers = Database.GetCollection<TrainerModel>(MongoCollection.Trainers.ToString());
            Npcs = Database.GetCollection<NpcModel>("NPCs");
            Logs = Database.GetCollection<LoggerModel>("Logs");
            PokeDex = Database.GetCollection<PokeDexItemModel>(MongoCollection.PokeDex.ToString());
            Encounter = Database.GetCollection<EncounterModel>(MongoCollection.Encounters.ToString());
            Sprite = Database.GetCollection<SpriteModel>("Sprites");
        }

        /// <summary>
        /// Represents the BasePokemon Collection
        /// </summary>
        public static IMongoDatabase Database { get; }

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
        /// Represents the PokeDex Collection
        /// </summary>
        public static IMongoCollection<PokeDexItemModel> PokeDex { get; }

        /// <summary>
        /// Represents the Encounter Collection
        /// </summary>
        public static IMongoCollection<EncounterModel> Encounter { get; }

        /// <summary>
        /// Represents the Sprites Collection
        /// </summary>
        public static IMongoCollection<SpriteModel> Sprite { get; }

        /// <summary>
        /// Represents the Logs Collection
        /// </summary>
        public static IMongoCollection<LoggerModel> Logs { get; }

        private static MongoClientSettings GetMongoClientSettings()
        {
            var connectionString = Environment.GetEnvironmentVariable("MongoDBConnectionString");
            if (string.IsNullOrWhiteSpace(connectionString))
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
