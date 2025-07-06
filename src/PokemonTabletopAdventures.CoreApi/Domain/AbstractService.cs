using MongoDB.Driver;
using PokemonTabletopAdventures.CoreApi.Domain.Handlers;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PokemonTabletopAdventures.CoreApi.Domain;

public abstract class AbstractService<T>(string collectionName)
{
    private static readonly ReplaceOptions ReplaceOptions = new ReplaceOptions { IsUpsert = true };

    public IMongoCollection<T> Collection { get; } = MongoCollectionHelper.GetMongoCollection<T>(collectionName);

    public async Task<T> ThrowIfNull<T2>(T2 entityValue, Func<T2, T> func, string entityName)
    {
        var item =  func(entityValue) ?? throw new UnknownEntityException<T>(entityName, entityValue);
        return await Task.FromResult(item);
    }

    public async Task<IEnumerable<T>> ThrowIfNull<T2>(T2 entityValue, Func<T2, IEnumerable<T>> func, string entityName)
    {
        var result = func(entityValue);
        if (result?.Any() != true)
        {
            throw new UnknownEntityException<T>(entityName, entityValue);
        }

        return await Task.FromResult(result);
    }

    public async Task PostDocument (T entity)
    {
        try
        {
            Collection.InsertOne(entity);
        }
        catch (MongoWriteException exception)
        {
            throw new PtaMongoException(exception);
        }

        await Task.CompletedTask;
    }

    public async Task UpsertDocument(FilterDefinition<T> func, T entity)
    {
        var result = Collection.ReplaceOne(
            func,
            options: ReplaceOptions,
            replacement: entity);
        if (!result.IsAcknowledged)
        {
            throw new UpdateException($"Failed to upsert at {typeof(T).Name}");
        }

        await Task.CompletedTask;
    }

    public async Task<T> UpdateDocument<T2>(
        T2 id,
        Expression<Func<T, bool>> filter,
        params UpdateDefinition<T>[] updates)
    {
        var update = Builders<T>.Update.Combine(updates);
        var item = Collection.FindOneAndUpdate(filter, update)
            ?? throw new UpdateException($"Failed to update {typeof(T).Name} {id}");
        return await Task.FromResult(item);
    }
}
