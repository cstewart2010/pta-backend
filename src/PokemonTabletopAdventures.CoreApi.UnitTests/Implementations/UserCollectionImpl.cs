using PokemonTabletopAdventures.CoreApi.Domain.Models;
using PokemonTabletopAdventures.CoreApi.DTOs.MongoDB;
using PokemonTabletopAdventures.CoreApi.Services;
using System.Linq.Expressions;
using System.Text;

namespace PokemonTabletopAdventures.CoreApi.UnitTests.Implementations;

internal class UserCollectionImpl : ICollectionService<UserDto>
{
    private static readonly Guid[] Ids = [.. Enumerable.Range(0, 3).Select(x => Guid.NewGuid())];
    internal IEnumerable<UserDto> Users { get; set; } = [.. Ids.Select(x => new UserDto
    {
        UserId = x,
        IsOnline = true,
        SiteRole = Models.Enums.UserRoleOnSite.Active,
        Messages = [],
        DateCreated = DateTime.Now,
        Username = x.ToString(),
        Games = [],
        ActivityToken = "",
        PasswordHash = ""
    })];

    public Task<UserDto?> DeleteAsync(Expression<Func<UserDto, bool>> filter)
    {
        return Task.FromResult(Users.FirstOrDefault(x => filter.Compile().Invoke(x)));
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
        return Task.FromResult(Users.FirstOrDefault(x => filter.Compile().Invoke(x)));
    }

    public Task<UserDto?> PatchAsync(Expression<Func<UserDto, bool>> filter, params UpdateData[] data)
    {
        return Task.FromResult(Users.FirstOrDefault(x => filter.Compile().Invoke(x)));
    }

    public Task PostAsync(UserDto entity)
    {
        Users = Users.Append(entity);
        return Task.CompletedTask;
    }

    public Task PutAsync(Expression<Func<UserDto, Guid>> filter, Guid id, UserDto entity)
    {
        var user = Users.FirstOrDefault(x => filter.Compile().Invoke(x) == id)!;
        user.SiteRole = entity.SiteRole;
        user.Messages = entity.Messages;
        user.Games = user.Games;
        user.IsOnline = entity.IsOnline;
        return Task.CompletedTask;
    }
}
