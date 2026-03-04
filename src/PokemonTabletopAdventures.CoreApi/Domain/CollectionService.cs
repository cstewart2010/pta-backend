using MongoDB.Driver;
using PokemonTabletopAdventures.CoreApi.Domain.Models;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.CoreApi.Services;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using PokemonTabletopAdventures.CoreApi.DTOs;

namespace PokemonTabletopAdventures.CoreApi.Domain;

[ExcludeFromCodeCoverage]
internal class CollectionService<TDto>() : ICollectionService<TDto> where TDto : IDocument
{
    private static readonly ReplaceOptions UpsertOptions = new ReplaceOptions { IsUpsert = true };

    internal required IMongoCollection<TDto> Collection { get; init; }

    public async Task<TDto?> DeleteAsync<TService>(Expression<Func<TDto, bool>> filter, ILogger<TService> logger)
    {
        logger.LogInformation("Deleting a filtered item from {CollectionName}", typeof(TDto).Name);
        var item = await Collection.FindOneAndDeleteAsync(filter);
        if (item == null)
        {
            logger.LogWarning("Filtered item was not found in {CollectionName}", typeof(TDto).Name);
        }
        return item;
    }

    public async Task DeleteManyAsync<TService>(Expression<Func<TDto, bool>> filter, ILogger<TService> logger)
    {
        logger.LogInformation("Deleting a filtered subset from {CollectionName}", typeof(TDto).Name);
        await Collection.DeleteManyAsync(filter);
    }

    public async Task<ICollection<TDto>> GetManyAsync<TService>(Expression<Func<TDto, bool>> filter, ILogger<TService> logger)
    {
        logger.LogInformation("Retrieving a filtered subset from {CollectionName}", typeof(TDto).Name);
        var items = await Collection.Find(filter).ToListAsync();
        if (items.Count == 0)
        {
            logger.LogWarning("Filtered subset was not found in {CollectionName}", typeof(TDto).Name);
        }
        return items;
    }

    public async Task<ICollection<TDto>> GetManyAsync<TService>(Expression<Func<TDto, bool>> filter, int offset, int limit, ILogger<TService> logger)
    {
        logger.LogInformation("Retrieving a filtered subset from {CollectionName}", typeof(TDto).Name);
        var items = await Collection.Find(document => true).Skip(offset).Limit(limit).ToListAsync();
        if (items.Count == 0)
        {
            logger.LogWarning("Filtered subset was not found in {CollectionName}", typeof(TDto).Name);
        }
        return items;
    }

    public async Task<TDto?> GetOneAsync<TService>(Expression<Func<TDto, bool>> filter, ILogger<TService> logger)
    {
        logger.LogInformation("Retrieving a filtered item from {CollectionName}", typeof(TDto).Name);
        var item = await Collection.Find(filter).SingleOrDefaultAsync();
        if (item == null)
        {
            logger.LogWarning("Filtered item was not found in {CollectionName}", typeof(TDto).Name);
        }
        return item;
    }

    public async Task<TDto?> PatchAsync<TService>(Expression<Func<TDto, bool>> filter, ILogger<TService> logger, params UpdateData[] data)
    {
        logger.LogInformation("Updating a filtered item from {CollectionName}", typeof(TDto).Name);
        var updates = data.Select(x => Builders<TDto>.Update.Set(x.Field, x.Value)).ToArray();
        var update = Builders<TDto>.Update.Combine(updates);
        var item = await Collection.FindOneAndUpdateAsync(filter, update);
        if (item == null)
        {
            logger.LogWarning("Filtered item was not found in {CollectionName}", typeof(TDto).Name);
        }
        return item;
    }

    public async Task PostAsync<TService>(TDto entity, ILogger<TService> logger)
    {
        try
        {
            logger.LogInformation("Adding a new item to {CollectionName}", typeof(TDto).Name);
            await Collection.InsertOneAsync(entity);
        }
        catch (MongoWriteException exception)
        {
            logger.LogWarning("Failed to add new item {CollectionName}", typeof(TDto).Name);
            throw new PtaMongoException(exception);
        }
    }

    public async Task PutAsync<TService>(Expression<Func<TDto, Guid>> filter, Guid id, TDto entity, ILogger<TService> logger)
    {
        logger.LogInformation("Upserting a new item to {CollectionName}", typeof(TDto).Name);
        var result = await Collection.ReplaceOneAsync(
            Builders<TDto>.Filter.Eq(filter, id),
            options: UpsertOptions,
            replacement: entity);
        if (!result.IsAcknowledged)
        {
            logger.LogWarning("Failed to upsert new item {CollectionName}", typeof(TDto).Name);
            throw new UpdateException($"Failed to upsert at {typeof(TDto).Name}");
        }
    }
}
