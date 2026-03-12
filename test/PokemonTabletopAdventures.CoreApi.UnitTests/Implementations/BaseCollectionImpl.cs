using PokemonTabletopAdventures.CoreApi.Domain.Models;
using PokemonTabletopAdventures.CoreApi.Services;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using PokemonTabletopAdventures.CoreApi.DTOs;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Implementations;

internal abstract class BaseCollectionImpl<TDto> : ICollectionService<TDto> where TDto : IDocument
{
    public abstract ICollection<TDto> Collection { get; protected set; }

    public Task<TDto?> DeleteAsync<TService>(Expression<Func<TDto, bool>> filter, ILogger<TService> logger)
    {
        var item = Collection.FirstOrDefault(x => filter.Compile().Invoke(x));
        if (item != null)
        {
            Collection.Remove(item);
        }
        return Task.FromResult(item);
    }

    public Task DeleteManyAsync<TService>(Expression<Func<TDto, bool>> filter, ILogger<TService> logger)
    {
        Collection = [.. Collection.Where(x => !filter.Compile().Invoke(x))];
        return Task.CompletedTask;
    }

    public Task<ICollection<TDto>> GetManyAsync<TService>(Expression<Func<TDto, bool>> filter, ILogger<TService> logger)
    {
        ICollection<TDto> items = [.. Collection.Where(x => filter.Compile().Invoke(x))];
        return Task.FromResult(items);
    }

    public Task<ICollection<TDto>> GetManyAsync<TService>(Expression<Func<TDto, bool>> filter, int offset, int limit, ILogger<TService> logger)
    {
        ICollection<TDto> items = [.. Collection.Where(x => filter.Compile().Invoke(x)).Skip(offset).Take(limit)];
        return Task.FromResult(items);
    }

    public Task<TDto?> GetOneAsync<TService>(Expression<Func<TDto, bool>> filter, ILogger<TService> logger)
    {
        return Task.FromResult(Collection.FirstOrDefault(x => filter.Compile().Invoke(x)));
    }

    public Task<TDto?> PatchAsync<TService>(Expression<Func<TDto, bool>> filter, ILogger<TService> logger, params UpdateData[] data)
    {
        var item = Collection.FirstOrDefault(x => filter.Compile().Invoke(x));
        if (item == null)
        {
            return Task.FromResult(item);
        }
        var properties = item.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        foreach (var part in data)
        {
            var property = properties.FirstOrDefault(x => x.Name.Equals(part.Field, StringComparison.OrdinalIgnoreCase));
            property?.SetValue(item, part.Value);
        }
        return Task.FromResult(item)!;
    }

    public Task PostAsync<TService>(TDto entity, ILogger<TService> logger)
    {
        Collection.Add(entity);
        return Task.CompletedTask;
    }

    public Task PutAsync<TService>(Expression<Func<TDto, Guid>> filter, Guid id, TDto entity, ILogger<TService> logger)
    {
        var item = Collection.FirstOrDefault(x => filter.Compile().Invoke(x) == id);
        if (item == null)
        {
            return Task.FromResult(item);
        }
        var properties = item.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        foreach (var property in properties)
        {
            property.SetValue(item, property.GetValue(entity));
        }
        return Task.CompletedTask;
    }
}
