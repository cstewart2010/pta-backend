using PokemonTabletopAdventures.CoreApi.Domain.Models;
using PokemonTabletopAdventures.CoreApi.Services;
using System.Linq;
using System.Linq.Expressions;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Implementations;

internal abstract class BaseCollectionImpl<TDto> : ICollectionService<TDto>
{
    public abstract ICollection<TDto> Collection { get; set; }

    public Task<TDto?> DeleteAsync(Expression<Func<TDto, bool>> filter)
    {
        var item = Collection.FirstOrDefault(x => filter.Compile().Invoke(x));
        if (item != null)
        {
            Collection.Remove(item);
        }
        return Task.FromResult(item);
    }

    public Task DeleteManyAsync(Expression<Func<TDto, bool>> filter)
    {
        Collection = [.. Collection.Where(x => !filter.Compile().Invoke(x))];
        return Task.CompletedTask;
    }

    public Task<IEnumerable<TDto>> GetManyAsync(Expression<Func<TDto, bool>> filter)
    {
        return Task.FromResult(Collection.Where(x => filter.Compile().Invoke(x)));
    }

    public Task<IEnumerable<TDto>> GetManyAsync(Expression<Func<TDto, bool>> filter, int offset, int limit)
    {
        return Task.FromResult(Collection.Where(x => filter.Compile().Invoke(x)).Skip(offset).Take(limit));
    }

    public Task<TDto?> GetOneAsync(Expression<Func<TDto, bool>> filter)
    {
        return Task.FromResult(Collection.FirstOrDefault(x => filter.Compile().Invoke(x)));
    }

    public Task<TDto?> PatchAsync(Expression<Func<TDto, bool>> filter, params UpdateData[] data)
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

    public Task PostAsync(TDto entity)
    {
        Collection.Add(entity);
        return Task.CompletedTask;
    }

    public Task PutAsync(Expression<Func<TDto, Guid>> filter, Guid id, TDto entity)
    {
        var pokemon = Collection.FirstOrDefault(x => filter.Compile().Invoke(x) == id)!;
        var properties = pokemon.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        foreach (var property in properties)
        {
            property.SetValue(pokemon, property.GetValue(entity));
        }
        return Task.CompletedTask;
    }
}
