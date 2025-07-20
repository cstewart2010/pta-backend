using PokemonTabletopAdventures.CoreApi.Domain.Models;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using System.Linq.Expressions;
using System.Text;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Implementations;

internal class UserCollectionImpl : ICollectionService<UserDto>
{
    internal IEnumerable<UserDto> Users { get; set; } = [..Enumerable.Range(0, 3).Select(x =>
    {
        var id = Guid.NewGuid();
        return new UserDto
        {
            UserId = id,
            IsOnline = true,
            SiteRole = Models.Enums.UserRoleOnSite.Active,
            Messages = [],
            DateCreated = DateTime.Now,
            Username = id.ToString(),
            Games = [],
            ActivityToken = "",
            PasswordHash = ""
        };
    })];

    public Task<UserDto?> DeleteAsync(Expression<Func<UserDto, bool>> filter)
    {
        return Task.FromResult(Users.SingleOrDefault(x => filter.Compile().Invoke(x)));
    }

    public Task DeleteManyAsync(Expression<Func<UserDto, bool>> filter)
    {
        Users = Users.Where(x => !filter.Compile().Invoke(x));
        return Task.CompletedTask;
    }

    public Task<IEnumerable<UserDto>> GetManyAsync(Expression<Func<UserDto, bool>> filter)
    {
        return Task.FromResult(Users.Where(x => filter.Compile().Invoke(x)));
    }

    public Task<IEnumerable<UserDto>> GetManyAsync(Expression<Func<UserDto, bool>> filter, int offset, int limit)
    {
        return Task.FromResult(Users.Where(x => filter.Compile().Invoke(x)).Skip(offset).Take(limit));
    }

    public Task<UserDto?> GetOneAsync(Expression<Func<UserDto, bool>> filter)
    {
        return Task.FromResult(Users.SingleOrDefault(x => filter.Compile().Invoke(x)));
    }

    public Task<UserDto?> PatchAsync(Expression<Func<UserDto, bool>> filter, params UpdateData[] data)
    {
        var item = Users.SingleOrDefault(x => filter.Compile().Invoke(x));
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

    public Task PostAsync(UserDto entity)
    {
        Users = Users.Append(entity);
        return Task.CompletedTask;
    }

    public Task PutAsync(Expression<Func<UserDto, Guid>> filter, Guid id, UserDto entity)
    {
        var user = Users.SingleOrDefault(x => filter.Compile().Invoke(x) == id)!;
        user.SiteRole = entity.SiteRole;
        user.Messages = entity.Messages;
        user.Games = user.Games;
        user.IsOnline = entity.IsOnline;
        return Task.CompletedTask;
    }
}
