using MongoDB.Driver;
using PokemonTabletopAdventures.CoreApi.Domain.Models;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.CoreApi.Services;
using System.Linq.Expressions;

namespace PokemonTabletopAdventures.CoreApi.Domain;

public abstract class AbstractMongoService<T>(
    IRepositoryService repositoryService,
    string collectionName)
{
    public ICollectionService<T> Collection { get; } = repositoryService.GetCollection<T>(collectionName);

    public async Task<T> ThrowIfNull<T2>(T2 entityValue, Func<T2, Task<T?>> func, string entityName)
    {
        var item =  await func(entityValue) ?? throw new UnknownEntityException<T>(entityName, entityValue);
        return item;
    }

    public async Task<IEnumerable<T>> ThrowIfNull<T2>(T2 entityValue, Func<T2, Task<IEnumerable<T>>> func, string entityName)
    {
        var result = await func(entityValue);
        if (result?.Any() != true)
        {
            throw new UnknownEntityException<T>(entityName, entityValue);
        }

        return result;
    }

    public async Task PostDocument (T entity)
    {
        try
        {
            await Collection.PostAsync(entity);
        }
        catch (MongoWriteException exception)
        {
            throw new PtaMongoException(exception);
        }
    }

    public async Task UpsertDocument(Expression<Func<T, Guid>> filter, Guid id, T entity)
    {
        await Collection.PutAsync(filter, id, entity);
    }

    public async Task<T> UpdateDocument<T2>(
        T2 id,
        Expression<Func<T, bool>> filter,
        params UpdateData[] updates)
    {
        var item = await Collection.PatchAsync(filter, updates)
            ?? throw new UpdateException($"Failed to update {typeof(T).Name} {id}");
        return item;
    }
}
