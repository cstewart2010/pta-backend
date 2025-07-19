using PokemonTabletopAdventures.CoreApi.Domain.Models;
using System.Linq.Expressions;

namespace PokemonTabletopAdventures.CoreApi.Services;

public interface ICollectionService<TDto>
{
    public Task<TDto?> GetOneAsync(Expression<Func<TDto, bool>> filter);
    public Task<IEnumerable<TDto>> GetManyAsync(Expression<Func<TDto, bool>> filter);
    public Task<IEnumerable<TDto>> GetManyAsync(Expression<Func<TDto, bool>> filter, int offset, int limit);
    public Task PostAsync(TDto entity);
    public Task PutAsync(Expression<Func<TDto, Guid>> filter, Guid id, TDto entity);
    public Task<TDto?> PatchAsync(Expression<Func<TDto, bool>> filter, params UpdateData[] data);
    public Task<TDto?> DeleteAsync(Expression<Func<TDto, bool>> filter);
    public Task DeleteManyAsync(Expression<Func<TDto, bool>> filter);
}
