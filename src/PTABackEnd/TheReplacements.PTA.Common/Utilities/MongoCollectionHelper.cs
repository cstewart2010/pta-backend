using MongoDB.Driver;
using System;
using TheReplacements.PTA.Common.Models;

namespace TheReplacements.PTA.Common.Utilities
{
    internal static class MongoCollectionHelper
    {
        public static IMongoCollection<GameModel> Game { get; }
        public static IMongoCollection<PokemonModel> Pokemon { get; }
        public static IMongoCollection<TrainerModel> Trainer { get; }
        public static IMongoCollection<NpcModel> Npc { get; }

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
        }
    }
}
