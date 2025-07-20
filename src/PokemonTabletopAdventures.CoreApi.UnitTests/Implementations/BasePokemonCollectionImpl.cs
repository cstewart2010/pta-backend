using PokemonTabletopAdventures.CoreApi.Domain.Models;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using System.Linq.Expressions;
using System.Text;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Implementations;

internal class BasePokemonCollectionImpl : ICollectionService<BasePokemonDto>
{
    private static readonly string[] Forms = ["Base", "Gigantamax", "Mega"];
    internal IEnumerable<BasePokemonDto> Pokemon { get; set; } = [.. Forms.Select(x => new BasePokemonDto
        {
            DexNo = 3,
            Form = x,
            Name = "Venusaur",
            EvolvesFrom = "Ivysaur",
            Moves = ["Super Cool Move"]
        })];

    public Task<BasePokemonDto?> DeleteAsync(Expression<Func<BasePokemonDto, bool>> filter)
    {
        return Task.FromResult(Pokemon.FirstOrDefault(x => filter.Compile().Invoke(x)));
    }

    public Task DeleteManyAsync(Expression<Func<BasePokemonDto, bool>> filter)
    {
        Pokemon = Pokemon.Where(x => !filter.Compile().Invoke(x));
        return Task.CompletedTask;
    }

    public Task<IEnumerable<BasePokemonDto>> GetManyAsync(Expression<Func<BasePokemonDto, bool>> filter)
    {
        return Task.FromResult(Pokemon.Where(x => filter.Compile().Invoke(x)));
    }

    public Task<IEnumerable<BasePokemonDto>> GetManyAsync(Expression<Func<BasePokemonDto, bool>> filter, int offset, int limit)
    {
        return Task.FromResult(Pokemon.Where(x => filter.Compile().Invoke(x)).Skip(offset).Take(limit));
    }

    public Task<BasePokemonDto?> GetOneAsync(Expression<Func<BasePokemonDto, bool>> filter)
    {
        return Task.FromResult(Pokemon.FirstOrDefault(x => filter.Compile().Invoke(x)));
    }

    public Task<BasePokemonDto?> PatchAsync(Expression<Func<BasePokemonDto, bool>> filter, params UpdateData[] data)
    {
        var item = Pokemon.FirstOrDefault(x => filter.Compile().Invoke(x));
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

    public Task PostAsync(BasePokemonDto entity)
    {
        Pokemon = Pokemon.Append(entity);
        return Task.CompletedTask;
    }

    public Task PutAsync(Expression<Func<BasePokemonDto, Guid>> filter, Guid id, BasePokemonDto entity)
    {
        var pokemon = Pokemon.FirstOrDefault(x => filter.Compile().Invoke(x) == id)!;
        pokemon.Name = entity.Name;
        pokemon.Form = entity.Form;
        pokemon.DexNo = entity.DexNo;
        return Task.CompletedTask;
    }
}
