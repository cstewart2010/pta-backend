using PokemonTabletopAdventures.CoreApi.Domain.Models;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using System.Linq.Expressions;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Implementations;

internal class PokedexCollectionImpl : ICollectionService<PokeDexItemDto>
{
    internal ICollection<PokeDexItemDto> PokedexItems { get; set; } = Shared.GameIds.Aggregate(new List<PokeDexItemDto>(), (current, next) =>
    {
        var trainerItems = Shared.UserIds.Aggregate(new List<PokeDexItemDto>(), (innerCurrent, innerNext) =>
        {
            var items = Enumerable.Range(0, 3).Select(x =>
            {
                var id = Guid.NewGuid();
                return new PokeDexItemDto
                {
                    GameId = next,
                    TrainerId = innerNext,
                    DexNo = x,
                    IsSeen = true,
                    IsCaught = Random.Shared.Next(2) == 0,
                };
            });
            innerCurrent.AddRange(items);
            return innerCurrent;
        });
        current.AddRange(trainerItems);
        return current;
    });

    public Task<PokeDexItemDto?> DeleteAsync(Expression<Func<PokeDexItemDto, bool>> filter)
    {
        return Task.FromResult(PokedexItems.SingleOrDefault(x => filter.Compile().Invoke(x)));
    }

    public Task DeleteManyAsync(Expression<Func<PokeDexItemDto, bool>> filter)
    {
        PokedexItems = [.. PokedexItems.Where(x => !filter.Compile().Invoke(x))];
        return Task.CompletedTask;
    }

    public Task<IEnumerable<PokeDexItemDto>> GetManyAsync(Expression<Func<PokeDexItemDto, bool>> filter)
    {
        return Task.FromResult(PokedexItems.Where(x => filter.Compile().Invoke(x)));
    }

    public Task<IEnumerable<PokeDexItemDto>> GetManyAsync(Expression<Func<PokeDexItemDto, bool>> filter, int offset, int limit)
    {
        return Task.FromResult(PokedexItems.Where(x => filter.Compile().Invoke(x)).Skip(offset).Take(limit));
    }

    public Task<PokeDexItemDto?> GetOneAsync(Expression<Func<PokeDexItemDto, bool>> filter)
    {
        return Task.FromResult(PokedexItems.SingleOrDefault(x => filter.Compile().Invoke(x)));
    }

    public Task<PokeDexItemDto?> PatchAsync(Expression<Func<PokeDexItemDto, bool>> filter, params UpdateData[] data)
    {
        var item = PokedexItems.SingleOrDefault(x => filter.Compile().Invoke(x));
        if (item == null)
        {
            return Task.FromResult(item);
        }
        var properties = item.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        foreach (var part in data)
        {
            var property = properties.SingleOrDefault(x => x.Name.Equals(part.Field, StringComparison.OrdinalIgnoreCase));
            property?.SetValue(item, part.Value);
        }
        return Task.FromResult(item)!;
    }

    public Task PostAsync(PokeDexItemDto entity)
    {
        PokedexItems.Add(entity);
        return Task.CompletedTask;
    }

    public Task PutAsync(Expression<Func<PokeDexItemDto, Guid>> filter, Guid id, PokeDexItemDto entity)
    {
        var item = PokedexItems.SingleOrDefault(x => filter.Compile().Invoke(x) == id)!;
        item.IsSeen = entity.IsSeen;
        item.IsCaught = entity.IsCaught;
        item.DexNo = entity.DexNo;
        item.TrainerId = entity.TrainerId;
        item.GameId = entity.GameId;
        return Task.CompletedTask;
    }
}
