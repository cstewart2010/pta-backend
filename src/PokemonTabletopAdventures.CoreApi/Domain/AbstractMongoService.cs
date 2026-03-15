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

    protected async Task PostDocument<TService>(TDto entity, ILogger<TService> logger)
    {
        await PostDocument(Collection, entity, logger);
    }

    protected async Task PostUniqueDocument<TService>(TDto entity, Expression<Func<TDto, bool>> filter, ILogger<TService> logger)
    {
        logger.LogInformation("Checking for an item matching filter in {CollectionName}", typeof(TDto).Name);
        var check = await Collection.GetOneAsync(filter, logger);
        if (check != null)
        {
            logger.LogWarning("Matching item found in {CollectionName}", typeof(TDto).Name);
            throw new DuplicateEntryException(typeof(TDto));
        }
        await PostDocument(Collection, entity, logger);
    }

    protected async Task PostDocument<TCollection, TService>(
        ICollectionService<TCollection> collection,
        TCollection entity,
        ILogger<TService> logger)
        where TCollection : IDocument
    {
        await collection.PostAsync(entity, logger);
    }

    protected async Task UpsertDocument<TService>(Expression<Func<TDto, Guid>> filter, Guid id, TDto entity, ILogger<TService> logger)
    {
        await Collection.PutAsync(filter, id, entity,  logger);
    }

    protected async Task<TDto> UpdateDocument<TInput, TService>(
        TInput id,
        Expression<Func<TDto, bool>> filter,
        ILogger<TService> logger,
        params UpdateData[] updates)
    {
        var item = await Collection.PatchAsync(filter, logger, updates)
            ?? throw new UpdateException($"Failed to update {typeof(TDto).Name} {id}");
        return item;
    }
}
