using MongoDB.Driver;
using TheReplacements.PTA.Common.Models;

namespace TheReplacements.PTA.Common.Utilities
{
    public class TableHelper
    {
        public IMongoCollection<GameModel> Game { get; }
        public IMongoCollection<PokemonModel> Pokemon { get; }
        public IMongoCollection<TrainerModel> Trainer { get; }
        public int Port { get; }
        public string Uri { get; }

        internal TableHelper(int port, string uri)
        {
            var client = new MongoClient($"mongodb://{uri}:{port}/?readPreference=primary&appname=MongoDB%20Compass&directConnection=true&ssl=false");
            var database = client.GetDatabase("PTA");
            Game = database.GetCollection<GameModel>("Game");
            Pokemon = database.GetCollection<PokemonModel>("Pokemon");
            Trainer = database.GetCollection<TrainerModel>("Trainer");
            Port = port;
            Uri = uri;
        }
    }
}
