using PokemonTabletopAdventures.CoreApi.Domain.Models;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.CoreApi.Services;
using System.Linq.Expressions;

namespace PokemonTabletopAdventures.CoreApi.Domain;

public abstract class AbstractMongoService<T>(
    IRepositoryService repositoryService,
    string collectionName)
{
    protected ICollectionService<T> Collection { get; } = repositoryService.GetCollection<T>(collectionName);

    protected async Task<T> ThrowIfNull<T2>(T2 entityValue, Func<T2, Task<T?>> func, string entityName)
    {
        var item =  await func(entityValue) ?? throw new UnknownEntityException<T>(entityName, entityValue);
        return item;
    }

    protected async Task<IEnumerable<T>> ThrowIfNullOrEmpty<T2>(T2 entityValue, Func<T2, Task<ICollection<T>>> func, string entityName)
    {
        var result = await func(entityValue);
        if (result == null || result.Count == 0)
        {
            throw new UnknownEntityException<T>(entityName, entityValue);
        }

        return result;
    }

    protected async Task PostDocument (T entity)
    {
        await PostDocument(Collection, entity);
    }

    protected async Task PostUniqueDocument(T entity, Expression<Func<T, bool>> filter)
    {
        var check = await Collection.GetOneAsync(filter);
        if (check != null)
        {
            throw new DuplicateEntryException(typeof(T));
        }
        await PostDocument(Collection, entity);
    }

    protected async Task PostDocument<TCollection>(ICollectionService<TCollection> collection, TCollection entity)
    {
        await collection.PostAsync(entity);
    }

    protected async Task UpsertDocument(Expression<Func<T, Guid>> filter, Guid id, T entity)
    {
        await Collection.PutAsync(filter, id, entity);
    }

    protected async Task<T> UpdateDocument<T2>(
        T2 id,
        Expression<Func<T, bool>> filter,
        params UpdateData[] updates)
    {
        var item = await Collection.PatchAsync(filter, updates)
            ?? throw new UpdateException($"Failed to update {typeof(T).Name} {id}");
        return item;
    }
}
