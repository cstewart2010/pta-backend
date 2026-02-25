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

    public async Task<TDto?> DeleteAsync(Expression<Func<TDto, bool>> filter)
    {
        var item = await Collection.FindOneAndDeleteAsync(filter);
        return item;
    }

    public async Task DeleteManyAsync(Expression<Func<TDto, bool>> filter)
    {
        await Collection.DeleteManyAsync(filter);
    }

    public async Task<ICollection<TDto>> GetManyAsync(Expression<Func<TDto, bool>> filter)
    {
        var items = await Collection.Find(filter).ToListAsync();
        return items;
    }

    public async Task<ICollection<TDto>> GetManyAsync(Expression<Func<TDto, bool>> filter, int offset, int limit)
    {
        var items = await Collection.Find(document => true).Skip(offset).Limit(limit).ToListAsync();
        return items;
    }

    public async Task<TDto?> GetOneAsync(Expression<Func<TDto, bool>> filter)
    {
        var item = await Collection.Find(filter).SingleOrDefaultAsync();
        return item;
    }

    public async Task<TDto?> PatchAsync(Expression<Func<TDto, bool>> filter, params UpdateData[] data)
    {
        var updates = data.Select(x => Builders<TDto>.Update.Set(x.Field, x.Value)).ToArray();
        var update = Builders<TDto>.Update.Combine(updates);
        var item = await Collection.FindOneAndUpdateAsync(filter, update);
        return item;
    }

    public async Task PostAsync(TDto entity)
    {
        try
        {
            await Collection.InsertOneAsync(entity);
        }
        catch (MongoWriteException exception)
        {
            throw new PtaMongoException(exception);
        }
    }

    public async Task PutAsync(Expression<Func<TDto, Guid>> filter, Guid id, TDto entity)
    {
        var result = await Collection.ReplaceOneAsync(
            Builders<TDto>.Filter.Eq(filter, id),
            options: UpsertOptions,
            replacement: entity);
        if (!result.IsAcknowledged)
        {
            throw new UpdateException($"Failed to upsert at {typeof(TDto).Name}");
        }
    }
}
