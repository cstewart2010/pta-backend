using PokemonTabletopAdventures.CoreApi.Domain.Models;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using System.Linq.Expressions;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Implementations;

internal class GameCollectionImpl : ICollectionService<GameDto>
{
    internal IEnumerable<GameDto> Games { get; set; } = [..Enumerable.Range(0, 3).Select(x =>
    {
        var id = Guid.NewGuid();
        return new GameDto
        {
            GameId = id,
            IsOnline = true,
            Logs = [],
            Nickname = id.ToString(),
            NPCs = [],
            PasswordHash = ""
        }; 
    })];

    public Task<GameDto?> DeleteAsync(Expression<Func<GameDto, bool>> filter)
    {
        return Task.FromResult(Games.SingleOrDefault(x => filter.Compile().Invoke(x)));
    }

    public Task DeleteManyAsync(Expression<Func<GameDto, bool>> filter)
    {
        Games = Games.Where(x => !filter.Compile().Invoke(x));
        return Task.CompletedTask;
    }

    public Task<IEnumerable<GameDto>> GetManyAsync(Expression<Func<GameDto, bool>> filter)
    {
        return Task.FromResult(Games.Where(x => filter.Compile().Invoke(x)));
    }

    public Task<IEnumerable<GameDto>> GetManyAsync(Expression<Func<GameDto, bool>> filter, int offset, int limit)
    {
        return Task.FromResult(Games.Where(x => filter.Compile().Invoke(x)).Skip(offset).Take(limit));
    }

    public Task<GameDto?> GetOneAsync(Expression<Func<GameDto, bool>> filter)
    {
        return Task.FromResult(Games.SingleOrDefault(x => filter.Compile().Invoke(x)));
    }

    public Task<GameDto?> PatchAsync(Expression<Func<GameDto, bool>> filter, params UpdateData[] data)
    {
        var item = Games.SingleOrDefault(x => filter.Compile().Invoke(x));
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

    public Task PostAsync(GameDto entity)
    {
        Games = Games.Append(entity);
        return Task.CompletedTask;
    }

    public Task PutAsync(Expression<Func<GameDto, Guid>> filter, Guid id, GameDto entity)
    {
        var game = Games.SingleOrDefault(x => filter.Compile().Invoke(x) == id)!;
        game.Nickname = entity.Nickname;
        game.NPCs = entity.NPCs;
        game.Logs = entity.Logs;
        game.IsOnline = entity.IsOnline;
        return Task.CompletedTask;
    }
}
