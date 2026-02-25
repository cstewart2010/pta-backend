using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using PokemonTabletopAdventures.CoreApi.Constants;
using PokemonTabletopAdventures.CoreApi.Services;
using System.Diagnostics.CodeAnalysis;
using PokemonTabletopAdventures.CoreApi.DTOs;

namespace PokemonTabletopAdventures.CoreApi.Domain;

[ExcludeFromCodeCoverage]
internal class RepositoryService : IRepositoryService
{
    public RepositoryService()
    {
        BsonSerializer.RegisterSerializer(new GuidSerializer(MongoDB.Bson.GuidRepresentation.Standard));
        var settings = GetMongoClientSettings();
        var client = new MongoClient(settings);
        var databaseName = Environment.GetEnvironmentVariable(EnvironmentVariableNames.Database, EnvironmentVariableTarget.Process);
        Database = client.GetDatabase(databaseName);
    }

    /// <summary>
    /// Represents the BasePokemon Collection
    /// </summary>
    private IMongoDatabase Database { get; }

    public ICollectionService<T> GetCollection<T>(string collectionName) where T : IDocument
    {
        return new CollectionService<T>()
        {
            Collection = GetMongoCollection<T>(collectionName)
        };
    }

    private IMongoCollection<T> GetMongoCollection<T>(string collectionName)
    {
        return Database.GetCollection<T>(collectionName);
    }

    #region Helper methods
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
    #endregion
}
