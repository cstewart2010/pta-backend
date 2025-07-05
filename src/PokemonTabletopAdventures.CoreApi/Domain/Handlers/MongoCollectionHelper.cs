using MongoDB.Driver;
using PokemonTabletopAdventures.CoreApi.Constants;
using System;

namespace PokemonTabletopAdventures.CoreApi.Domain.Handlers;

internal static class MongoCollectionHelper
{
    static MongoCollectionHelper()
    {
        var settings = GetMongoClientSettings();
        var client = new MongoClient(settings);
        var databaseName = Environment.GetEnvironmentVariable(EnvironmentVariableNames.Database, EnvironmentVariableTarget.Process);
        Database = client.GetDatabase(databaseName);
    }

    /// <summary>
    /// Represents the BasePokemon Collection
    /// </summary>
    private static IMongoDatabase Database { get; }

    public static IMongoCollection<T> GetMongoCollection<T>(string collectionName)
    {
        return Database.GetCollection<T>(collectionName);
    }

    private static MongoClientSettings GetMongoClientSettings()
    {
        var connectionString = Environment.GetEnvironmentVariable(EnvironmentVariableNames.MongoDBConnectionString, EnvironmentVariableTarget.Process);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new NullReferenceException($"{EnvironmentVariableNames.MongoDBConnectionString} environment variable need to be set to access MongoDB");
        }

        var settings = MongoClientSettings.FromConnectionString(connectionString);
        settings.SslSettings = new SslSettings()
        {
            EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12
        };

        return settings;
    }
}
