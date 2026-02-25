using PokemonTabletopAdventures.CoreApi.Domain.Models;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.CoreApi.Services;
using System.Linq.Expressions;
using PokemonTabletopAdventures.CoreApi.DTOs;

namespace PokemonTabletopAdventures.CoreApi.Domain;

public abstract class AbstractMongoService<TDto>(
    IRepositoryService repositoryService,
    string collectionName) where TDto : IDocument
{
    protected ICollectionService<TDto> Collection { get; } = repositoryService.GetCollection<TDto>(collectionName);

    protected async Task<TDto> ThrowIfNull<TInput>(TInput entityValue, Func<TInput, Task<TDto?>> func, string entityName)
    {
        var item =  await func(entityValue) ?? throw new UnknownEntityException<TDto>(entityName, entityValue);
        return item;
    }

    protected async Task PostDocument (TDto entity)
    {
        await PostDocument(Collection, entity);
    }

    protected async Task PostUniqueDocument(TDto entity, Expression<Func<TDto, bool>> filter)
    {
        var check = await Collection.GetOneAsync(filter);
        if (check != null)
        {
            throw new DuplicateEntryException(typeof(TDto));
        }
        await PostDocument(Collection, entity);
    }

    protected async Task PostDocument<TCollection>(
        ICollectionService<TCollection> collection,
        TCollection entity)
        where TCollection : IDocument
    {
        await collection.PostAsync(entity);
    }

    protected async Task UpsertDocument(Expression<Func<TDto, Guid>> filter, Guid id, TDto entity)
    {
        await Collection.PutAsync(filter, id, entity);
    }

    protected async Task<TDto> UpdateDocument<TInput>(
        TInput id,
        Expression<Func<TDto, bool>> filter,
        params UpdateData[] updates)
    {
        var item = await Collection.PatchAsync(filter, updates)
            ?? throw new UpdateException($"Failed to update {typeof(TDto).Name} {id}");
        return item;
    }
}
