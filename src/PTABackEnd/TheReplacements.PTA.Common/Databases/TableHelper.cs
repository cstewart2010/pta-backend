using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheReplacements.PTA.Common.Databases
{
    public class TableHelper<T>
    {
        public  IMongoCollection<T> Collection { get; }
        public int Port { get; }
        public string Uri { get; }

        public TableHelper(int port, string uri)
        {
            var client = new MongoClient($"mongodb://{uri}:{port}/?readPreference=primary&appname=MongoDB%20Compass&directConnection=true&ssl=false");
            var database = client.GetDatabase("PTA");
            Collection = database.GetCollection<T>(typeof(T).Name);
            Port = port;
            Uri = uri;
        }
    }
}
