using PokemonTabletopAdventures.CoreApi.Domain.Models;
using System.Linq.Expressions;
using PokemonTabletopAdventures.CoreApi.DTOs;

namespace PokemonTabletopAdventures.CoreApi.Services;

public interface ICollectionService<TDto> where TDto : IDocument
{
    public Task<TDto?> GetOneAsync<TService>(Expression<Func<TDto, bool>> filter, ILogger<TService> logger);
    public Task<ICollection<TDto>> GetManyAsync<TService>(Expression<Func<TDto, bool>> filter, ILogger<TService> logger);
    public Task<ICollection<TDto>> GetManyAsync<TService>(Expression<Func<TDto, bool>> filter, int offset, int limit, ILogger<TService> logger);
    public Task PostAsync<TService>(TDto entity, ILogger<TService> logger);
    public Task PutAsync<TService>(Expression<Func<TDto, Guid>> filter, Guid id, TDto entity, ILogger<TService> logger);
    public Task<TDto?> PatchAsync<TService>(Expression<Func<TDto, bool>> filter, ILogger<TService> logger, params UpdateData[] data);
    public Task<TDto?> DeleteAsync<TService>(Expression<Func<TDto, bool>> filter, ILogger<TService> logger);
    public Task DeleteManyAsync<TService>(Expression<Func<TDto, bool>> filter, ILogger<TService> logger);
}
