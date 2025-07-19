using MongoDB.Driver;
using PokemonTabletopAdventures.CoreApi.Domain.Models;
using PokemonTabletopAdventures.CoreApi.Exceptions;
using PokemonTabletopAdventures.CoreApi.Services;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace PokemonTabletopAdventures.CoreApi.Domain;

[ExcludeFromCodeCoverage]
internal class CollectionService<TDto>() : ICollectionService<TDto>
{
    private static readonly ReplaceOptions UpsertOptions = new ReplaceOptions { IsUpsert = true };

    internal required IMongoCollection<TDto> Collection { get; set; }

    public async Task<TDto?> DeleteAsync(Expression<Func<TDto, bool>> filter)
    {
        var item = Collection.FindOneAndDelete(filter);
        return await Task.FromResult(item);
    }

    public async Task DeleteManyAsync(Expression<Func<TDto, bool>> filter)
    {
        Collection.DeleteMany(filter);
        await Task.CompletedTask;
    }

    public async Task<IEnumerable<TDto>> GetManyAsync(Expression<Func<TDto, bool>> filter)
    {
        var items = Collection.Find(filter).ToEnumerable();
        return await Task.FromResult(items);
    }

    public async Task<IEnumerable<TDto>> GetManyAsync(Expression<Func<TDto, bool>> filter, int offset, int limit)
    {
        var items = Collection.Find(document => true).Skip(offset).Limit(limit).ToEnumerable();
        return await Task.FromResult(items);
    }

    public async Task<TDto?> GetOneAsync(Expression<Func<TDto, bool>> filter)
    {
        var item = Collection.Find(filter).SingleOrDefault();
        return await Task.FromResult(item);
    }

    public async Task<TDto?> PatchAsync(Expression<Func<TDto, bool>> filter, params UpdateData[] data)
    {
        var updates = data.Select(x => Builders<TDto>.Update.Set(x.Field, x.Value)).ToArray();
        var update = Builders<TDto>.Update.Combine(updates);
        var item = Collection.FindOneAndUpdate(filter, update);
        return await Task.FromResult(item);
    }

    public async Task PostAsync(TDto entity)
    {
        Collection.InsertOne(entity);
        await Task.CompletedTask;
    }

    public async Task PutAsync(Expression<Func<TDto, Guid>> filter, Guid id, TDto entity)
    {
        var result = Collection.ReplaceOne(
            Builders<TDto>.Filter.Eq(filter, id),
            options: UpsertOptions,
            replacement: entity);
        if (!result.IsAcknowledged)
        {
            throw new UpdateException($"Failed to upsert at {typeof(TDto).Name}");
        }

        await Task.CompletedTask;
    }
}
