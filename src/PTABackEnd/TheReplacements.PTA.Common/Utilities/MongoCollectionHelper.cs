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
            var username = Environment.GetEnvironmentVariable("MongoUsername");
            var password = Environment.GetEnvironmentVariable("MongoPassword");
            if (username == null || password == null)
            {
                throw new NullReferenceException("MongoUsername and MongoPassword environment variables need to be set to access MongoDB");
            }
            var settings = MongoClientSettings.FromConnectionString($"mongodb+srv://{username}:{password}@ptatestcluster.1ekcs.mongodb.net/PTA?retryWrites=true&w=majority");
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
