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
            Games = Database.GetCollection<GameModelv1>(MongoCollection.Games.ToString());
            Pokemon = Database.GetCollection<PokemonModelv1>(MongoCollection.Pokemon.ToString());
            Trainers = Database.GetCollection<TrainerModelv1>(MongoCollection.Trainers.ToString());
            Users = Database.GetCollection<UserModelv1>(MongoCollection.Users.ToString());
            UserMessageThreads = Database.GetCollection<UserMessageThreadModelv1>(MongoCollection.UserMessageThreads.ToString());
            Npcs = Database.GetCollection<NpcModelv1>("NPCs");
            PokeDex = Database.GetCollection<PokeDexItemModelv1>(MongoCollection.PokeDex.ToString());
            Settings = Database.GetCollection<SettingModelv1>(MongoCollection.Settings.ToString());
            Shops = Database.GetCollection<ShopModelv1>("Shops");
            Sprite = Database.GetCollection<SpriteModelv1>("Sprites");
        }

        /// <summary>
        /// Represents the BasePokemon Collection
        /// </summary>
        public static IMongoDatabase Database { get; }

        /// <summary>
        /// Represents the Game Collection
        /// </summary>
        public static IMongoCollection<GameModelv1> Games { get; }

        /// <summary>
        /// Represents the Pokemon Collection
        /// </summary>
        public static IMongoCollection<PokemonModelv1> Pokemon { get; }

        /// <summary>
        /// Represents the Trainer Collection
        /// </summary>
        public static IMongoCollection<TrainerModelv1> Trainers { get; }

        /// <summary>
        /// Represents the User Collection
        /// </summary>
        public static IMongoCollection<UserModelv1> Users { get; }

        /// <summary>
        /// Represents the User Collection
        /// </summary>
        public static IMongoCollection<UserMessageThreadModelv1> UserMessageThreads { get; }

        /// <summary>
        /// Represents the Npc Collection
        /// </summary>
        public static IMongoCollection<NpcModelv1> Npcs { get; }

        /// <summary>
        /// Represents the PokeDex Collection
        /// </summary>
        public static IMongoCollection<PokeDexItemModelv1> PokeDex { get; }

        /// <summary>
        /// Represents the Settings Collection
        /// </summary>
        public static IMongoCollection<SettingModelv1> Settings { get; }

        /// <summary>
        /// Represents the Shops Collection
        /// </summary>
        public static IMongoCollection<ShopModelv1> Shops { get; }

        /// <summary>
        /// Represents the Sprites Collection
        /// </summary>
        public static IMongoCollection<SpriteModelv1> Sprite { get; }

        private static MongoClientSettings GetMongoClientSettings()
        {
            var connectionString = Environment.GetEnvironmentVariable("MongoDBConnectionString", EnvironmentVariableTarget.Process);
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
